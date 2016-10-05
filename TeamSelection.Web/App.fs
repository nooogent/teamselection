namespace TeamSelection.Web

module App = 

    open Suave // always open suave
    open Suave.Filters
    open Suave.Operators
    open Suave.RequestErrors
    open Suave.Successful // for OK-result
    open TeamSelection.Types
    open TeamSelection.Data
    open TeamSelection.Functions
    open TeamSelection.Tournament

    let html container = 
        OK (View.index container)
        >=> Writers.setMimeType "text/html; charset=utf-8"

    let generateTeam =
        TeamSelection.Tournament.calculateTeams
            TeamSelection.Functions.getChildRatingAverages
            TeamSelection.Functions.getCoaches 
            TeamSelectionType.StreamedCoachWithChild 
            6
        |> View.team
        |> html 

    let webPart =
        choose [
            path Path.home >=> html View.home
            path Path.Team.generate >=> generateTeam

            pathRegex "(.*)\.(css|png|gif)" >=> Files.browseHome
            html View.notFound
        ]

    startWebServer defaultConfig webPart