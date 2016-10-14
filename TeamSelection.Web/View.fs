namespace TeamSelection.Web

module View =

    open System
    open Suave.Html
    open Suave.Form
    open TeamSelection.Types
    open TeamSelection.Child
    open Microsoft.FSharp.Reflection
    
    let meta attr = tag "meta" attr empty
    let div attr xml = divAttr attr xml
    let divId id = divAttr ["id",id]
    let divClass c = divAttr ["class",c]
    let h1 xml = tag "h1" [] xml
    let h2 s = tag "h2" [] (text s)
    let aHref href = tag "a" ["href", href]
    let cssLink href = linkAttr [ "href", href; " rel", "stylesheet"; " type", "text/css" ]
    let cssLinkHtml5 attr = linkAttr attr
    let ul xml = tag "ul" [] (flatten xml)
    let ulAttr attr xml = tag "ul" attr (flatten xml)
    let li = tag "li" []
    let imgSrc src = imgAttr [ "src", src ]
    let em s = tag "em" [] (text s)
    //let button s url = inputAttr ["type","button";"value",s;"onclick", (sprintf "javascript:window.location = '%s';" url)] 
    let formatDec (d : Decimal) = d.ToString(Globalization.CultureInfo.InvariantCulture)
    let form x = tag "form" ["method", "POST";"class","form-horizontal"] (flatten x)
    let submitInput attr value = inputAttr (List.append attr ["type", "submit"; "value", value])
    let button attr value = tag "button" (List.append attr ["type", "submit"]) (text value)
    let fieldset x = tag "fieldset" [] (flatten x)
    let legend txt = tag "legend" [] (text txt)

    let addAttributes newAttrs xml =
        match xml with
        | Element (n,ns,attrs) as e ->
             Element (n,ns,Array.append newAttrs attrs)
        | _ -> xml

    type Field<'a> = {
        Label : string
        Id : string
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
                divAttr [] [
                    yield legend set.Legend
                    for field in set.Fields do
                        yield divAttr ["class", "form-group"] [
                            yield tag "label" ["for",field.Id;"class","col-sm-2 control-label"] (text field.Label)
                            yield
                                match field.Xml layout.Form with 
                                | Xml(ns) 
                                    -> divAttr ["class", "col-sm-10"]
                                        [
                                            List.map(fun (e,x) -> ((addAttributes [|"id",field.Id;"class","form-control"|] e), x)) ns |> Xml.Xml
                                        ]
                        ]
                ]
            yield 
                divAttr ["class", "form-group"] [
                    divAttr ["class", "col-sm-offset-2 col-sm-10"] [
                        button ["class","btn btn-default"] layout.SubmitText
                    ]
                ]
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
                                    Label = "Club Name"
                                    Id = "clubName"
                                    Xml = input (fun f -> <@ f.ClubName @>) []
                                }
                                {
                                    Label = "Type"
                                    Id = "selectionType"
                                    Xml = selectInput (fun f -> <@ f.TypeId @>) TeamSelection.Web.Form.requestTypes None
                                }
                            ]
                    }
                ]
            SubmitText = "Generate"
        }
    ]
    
    let team t = [
        match t with
        | HomeTeam(Coach(co),cs,TeamName(n)) ->
            yield tag "h3" [] (text n)
            yield tag "h4" [] (text (sprintf "Coach: %s" co))
            yield ulAttr ["class","list-unstyled"] [
                for c in cs |> List.sortBy GetName ->
                match c with
                | ChildWithParent(ChildName(cn),_)
                | Child(ChildName(cn)) ->
                    li (text cn)
            ]
        | AwayTeam(TeamName(n)) ->
            yield li (text n)
        | NoTeamAvailable ->
            yield li (text "none")
    ]

    let teams (teams:Team seq) = [
        h2 (sprintf "Teams")
        div ["class","row"]
            [
                let indexedTeams = teams |> Seq.indexed
                for (i,t) in indexedTeams do
                    if(i > 0 && i % 3 = 0) then
                        yield div ["class","clearfix visible-lg-block"] [emptyText]
                    if(i > 0 && i % 2 = 0) then
                        yield div ["class","clearfix visible-sm-block visible-md-block"] [emptyText]
                    yield div ["class","col-xs-12 col-sm-6 col-md-6 col-lg-4"] (team t)
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

    let nav items =
        tag "nav" ["class","navbar navbar-inverse navbar-fixed-top"] (flatten
            [
                div ["class","container"] [
                    div ["class","navbar-header"] [
                        tag "button" ["type","button";"class","navbar-toggle collapsed";"data-toggle","collapse";"data-target","#navbar";"aria-expanded","false";"aria-controls","navbar"] (flatten 
                            [
                                spanAttr ["class","sr-only"] (text "Toggle navigation")
                                spanAttr ["class","icon-bar"] emptyText
                                spanAttr ["class","icon-bar"] emptyText
                                spanAttr ["class","icon-bar"] emptyText
                            ])
                        tag "a" ["class","navbar-brand";"href",Path.home] (text "Team Selection")
                    ]
                    div ["id","navbar";"class","collapse navbar-collapse"] [
                        ulAttr ["class","nav navbar-nav"] [
                            tag "li" ["class","active"] (aHref Path.home (text "Home"))
                            li (aHref "#about" (text "About"))
                            li (aHref "#contact" (text "Contact"))
                        ]
                    ]
                ]
            ])

    let index container =
        [
            text "<!DOCTYPE html>"
            html [
                head [
                    meta ["charset","utf-8"]
                    meta ["http-equiv","X-UA-Compatible";"content","IE=edge"]
                    meta ["name","viewport";"content","width=device-width, initial-scale=1"]
                    title "Team Selection"
                    cssLinkHtml5 ["rel","stylesheet"; "href","https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css"; " integrity","sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u"; " crossorigin","anonymous"]
                    cssLinkHtml5 ["rel","stylesheet"; "href","https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap-theme.min.css"; " integrity","sha384-rHyoN1iRsVXV4nD0JutlnGaslCJuC7uwjduW9SVrLvRYooPp2bWYgmgJQIXwl/Sp"; " crossorigin","anonymous"]
                    text "<!--[if lt IE 9]>"
                    tag "script" ["src","https://oss.maxcdn.com/html5shiv/3.7.3/html5shiv.min.js"] emptyText
                    tag "script" ["src","https://oss.maxcdn.com/respond/1.4.2/respond.min.js"] emptyText
                    text "<![endif]-->"
                ]
                body [
                    nav []
                    div ["id","main";"class","container"] 
                        [ 
                            (flatten container)
                            tag "hr" [] empty
                            tag "footer" [] (flatten
                                [
                                    text "built with "
                                    aHref "http://fsharp.org" (text "F#")
                                    text " and "
                                    aHref "http://suave.io" (text "Suave.IO")
                                ])
                        ]
                    tag "script" ["src","https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js"] emptyText
                    tag "script" ["src","https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js"; "integrity","sha384-Tc5IQib027qvyjSMfHjOMaLkfuWVxZxUPnCJA7l2mCWNIpG9mGCD8wGNIcPD7Txa"; "crossorigin","anonymous"] emptyText
                ]
            ]
        ]
        |> flatten
        |> xmlToString