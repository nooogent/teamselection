namespace TeamSelection.Web

module View =

    open System
    open Suave.Html
    open TeamSelection.Types

    let divId id = divAttr ["id", id]
    let h1 xml = tag "h1" [] xml
    let h2 s = tag "h2" [] (text s)
    let aHref href = tag "a" ["href", href]
    let cssLink href = linkAttr [ "href", href; " rel", "stylesheet"; " type", "text/css" ]
    let ul xml = tag "ul" [] (flatten xml)
    let li = tag "li" []
    let imgSrc src = imgAttr [ "src", src ]
    let em s = tag "em" [] (text s)
    let button s url = inputAttr ["type","button";"value",s;"onclick", (sprintf "javascript:window.location = '%s';" url)] 
    let formatDec (d : Decimal) = d.ToString(Globalization.CultureInfo.InvariantCulture)

    let home = [
        h2 "Home"
        button "generate" Path.Team.generate
    ]
    
    let team (teams:Team seq) = [
        h2 (sprintf "Teams")
        ul [
            for t in teams ->
                match t with
                | HomeTeam(Coach(c),cs,TeamName(n)) ->
                    li (text c)
                    li (text n)
                    ul [
                        for c' in cs ->
                        match c' with
                        | ChildWithParent(ChildName(cn),_)
                        | Child(ChildName(cn)) ->
                            li (text cn)
                    ]
                | AwayTeam(TeamName(n)) ->
                    li (text n)
                | NoTeamAvailable ->
                    li (text "none")
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
                cssLink "/Site.css"
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