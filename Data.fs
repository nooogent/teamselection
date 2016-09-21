namespace TeamSelection
    module Data = 
        open System
        open TeamSelection.Types
        open FSharp.Data
        
        type ChildCsv = CsvProvider<"data/TeamSelection.csv", Schema = "Name, Parent">
        type ChildRatingCsv = CsvProvider<"data/ChildRatings.csv", Schema = "Child, Coach, Rating">

        let parseRating csvRating = 
            match csvRating with 
            | 1 -> Rating.One
            | 2 -> Rating.Two 
            | 3 -> Rating.Three 
            | 4 -> Rating.Four 
            | 5 -> Rating.Five 
            | _ -> Rating.Three // default to average

        let parseChild name parent = 
            match parent with 
            | "" -> Child(ChildName(name)) 
            | _ -> ChildWithParent(ChildName(name),Parent(Coach(parent)))
            
        let parseChildRating (row:ChildRatingCsv.Row) = 
            ChildRating(parseChild row.Child "", parseRating row.Rating, Coach(row.Coach) )

        let getChildren = 
        
            let childrenCsv = ChildCsv.Load("data/TeamSelection.csv")
            
            childrenCsv.Rows
                |> Seq.map (fun row -> parseChild row.Name row.Parent)
                |> Seq.toList

        let getChildRatings = 
        
            let childRatingsCsv = ChildRatingCsv.Load("data/ChildRatings.csv")
            
            childRatingsCsv.Rows
                |> Seq.map parseChildRating
                |> Seq.toList