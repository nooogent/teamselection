namespace TeamSelection.Web

module View =

    open System
    open Suave.Html
    open Suave.Form
    open TeamSelection.Types
    open Microsoft.FSharp.Reflection
    
    let divId id = divAttr ["id", id]
    let divClass c = divAttr ["class", c]
    let h1 xml = tag "h1" [] xml
    let h2 s = tag "h2" [] (text s)
    let aHref href = tag "a" ["href", href]
    let cssLink href = linkAttr [ "href", href; " rel", "stylesheet"; " type", "text/css" ]
    let ul xml = tag "ul" [] (flatten xml)
    let ulAttr attr xml = tag "ul" attr (flatten xml)
    let li = tag "li" []
    let imgSrc src = imgAttr [ "src", src ]
    let em s = tag "em" [] (text s)
    let button s url = inputAttr ["type","button";"value",s;"onclick", (sprintf "javascript:window.location = '%s';" url)] 
    let formatDec (d : Decimal) = d.ToString(Globalization.CultureInfo.InvariantCulture)
    let form x = tag "form" ["method", "POST"] (flatten x)
    let submitInput value = inputAttr ["type", "submit"; "value", value]
    let fieldset x = tag "fieldset" [] (flatten x)
    let legend txt = tag "legend" [] (text txt)

    type Field<'a> = {
        Label : string
        Xml : Form<'a> -> Suave.Html.Xml
    }
    
    type Fieldset<'a> = {
        Legend : string
        Fields : Field<'a> list
    }
    
    type FormLayout<'a> = {
        Fieldsets : Fieldset<'a> list
        SubmitText : string
        Form : Form<'a>
    }
    
    let renderForm (layout : FormLayout<_>) =
        form [
            for set in layout.Fieldsets ->
                fieldset [
                    yield legend set.Legend
                    for field in set.Fields do
                        yield divClass "editor-label" [
                            text field.Label
                        ]
                        yield divClass "editor-field" [
                            field.Xml layout.Form
                        ]
                ]
            yield submitInput layout.SubmitText
        ]
    
    let home = [
        h2 "Home"
        renderForm {
            Form = Form.tsRequest
            Fieldsets = 
                [ 
                    { 
                        Legend = "Team Selection"
                        Fields = 
                            [
                                {
                                    Label = "Type"
                                    Xml = selectInput (fun f -> <@ f.TypeId @>) TeamSelection.Web.Form.requestTypes  (Some(fst TeamSelection.Web.Form.requestTypes.[0]))
                                }
                            ]
                    }
                ]
            SubmitText = "Generate"
        }
    ]
    
    let team (teams:Team seq) = [
        h2 (sprintf "Teams")
        ul
            [
                for t in teams do
                    match t with
                    | HomeTeam(Coach(c),cs,TeamName(n)) ->
                        yield li (text c)
                        yield li (text n)
                        yield ul [
                            for c' in cs ->
                            match c' with
                            | ChildWithParent(ChildName(cn),_)
                            | Child(ChildName(cn)) ->
                                li (text cn)
                        ]
                    | AwayTeam(TeamName(n)) ->
                        yield li (text n)
                    | NoTeamAvailable ->
                        yield li (text "none")
            ]
    ]

//    let store genres = [
//        h2 "Browse Genres"
//        p [
//            text (sprintf "Select from %d genres:" (List.length genres))
//        ]
//        ul [
//            for g in genres ->
//            li (aHref (Path.Store.browse |> Path.withParam (Path.Store.browseKey, g)) (text g))
//        ]
//    ]
//
//    let browse genre (albums:Db.Album list) = [
//        h2 (sprintf "Genre: %s" genre)
//        ul [
//            for a in albums ->
//            li (aHref (sprintf Path.Store.details a.AlbumId) (text a.Title))
//        ]
//    ]
//
//    let details (album : Db.AlbumDetails) = [
//        h2 album.Title
//        p [ imgSrc album.AlbumArtUrl ]
//        divId "album-details" [
//            for (caption,t) in ["Genre:",album.Genre;"Artist:",album.Artist;"Price:",formatDec album.Price] ->
//                p [
//                    em caption
//                    text t
//                ]
//        ]
//    ]

    let notFound = [
        h2 "Page not found"
        p [
            text "Could not find the requested resource"
        ]
        p [
            text "Back to "
            aHref Path.home (text "Home")
        ]
    ]

    let index container =
        html [
            head [
                title "Team Selection"
                cssLink "Site.css"
            ]
            body [
                divId "header" [
                    h1 (aHref Path.home (text "Team Selection Home"))
                ]

                divId "main" container

                divId "footer" [
                    text "built with "
                    aHref "http://fsharp.org" (text "F#")
                    text " and "
                    aHref "http://suave.io" (text "Suave.IO")
                ]
            ]
        ]
        |> xmlToString