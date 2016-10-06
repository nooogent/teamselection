namespace TeamSelection.Web

module App = 

    open Suave // always open suave
    open Suave.Form
    open Suave.Model.Binding
    open Suave.Filters
    open Suave.Operators
    open Suave.RequestErrors
    open Suave.Successful // for OK-result
    open TeamSelection.Types
    open TeamSelection.Data
    open TeamSelection.Functions
    open TeamSelection.Tournament

    let bindToForm form handler = 
        bindReq (bindForm form) handler BAD_REQUEST

    let html container = 
        OK (View.index container)
        >=> Writers.setMimeType "text/html; charset=utf-8"

    let generateTeam =
        choose [
            GET >=> html View.home
            POST >=> bindToForm Form.tsRequest (fun form ->
                warbler (fun _ ->
                    TeamSelection.Tournament.calculateTeams
                        TeamSelection.Functions.getChildRatingAverages
                        TeamSelection.Functions.getCoaches 
                        (TSMap.mapTeamSelectionType form.TypeId)
                        6
                    |> View.team
                    |> html))
        ]

    let webPart =
        choose [
            path Path.home >=> generateTeam
            path Path.Team.generate >=> generateTeam

            pathRegex "(.*)\.(css|png|gif)" >=> Files.browseHome
            html View.notFound
        ]

    startWebServer defaultConfig webPart