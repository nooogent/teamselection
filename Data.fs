namespace TeamSelection
    module Data = 
        open System
        open TeamSelection.Types
        open FSharp.Data
        
        type ChildCsv = CsvProvider<"data/TeamSelection.csv", Schema = "Name, Parent">
        type ChildRatingCsv = CsvProvider<"data/ChildRatings.csv", Schema = "Child, Coach, Rating">

        let parseChild (row:ChildCsv.Row) = 
            match row.Parent with 
            | "" -> Child(ChildName(row.Name)) 
            | _ -> ChildWithParent(ChildName(row.Name),Parent(Coach(row.Parent)))
            
        let parseRating csvRating = 
            match csvRating with 
            | 1 -> Rating.One
            | 2 -> Rating.Two 
            | 3 -> Rating.Three 
            | 4 -> Rating.Four 
            | _ -> Rating.Five

        let getChildren = 
        
            let childrenCsv = ChildCsv.Load("data/TeamSelection.csv")
            
            childrenCsv.Rows
                |> Seq.map parseChild
                |> Seq.toList

        let getChildRatings = 
        
            let childrenCsv = ChildRatingCsv.Load("data/ChildRatings.csv")
            
            let parseRating (row:ChildRatingCsv.Row) = 
                ChildRating(parseChild ChildCsv.Row(name:row.Child;parent:row.Coach), parseRating row.Rating, Coach(row.Coach) )

            childrenCsv.Rows
                |> Seq.map parseChild
                |> Seq.toList