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
            | _ -> Rating.Five

        let parseChild (row:ChildCsv.Row) = 
            match row.Parent with 
            | "" -> Child(ChildName(row.Name)) 
            | _ -> ChildWithParent(ChildName(row.Name),Parent(Coach(row.Parent)))
            
        let parseChildRating (row:ChildRatingCsv.Row) = 
            let rowChild = new ChildCsv.Row(row.Child,row.Coach)
            let parsedChild = parseChild rowChild
            ChildRating(parsedChild, parseRating row.Rating, Coach(row.Coach) )

        let getChildren = 
        
            let childrenCsv = ChildCsv.Load("data/TeamSelection.csv")
            
            childrenCsv.Rows
                |> Seq.map parseChild
                |> Seq.toList

        let getChildRatings = 
        
            let childRatingsCsv = ChildRatingCsv.Load("data/ChildRatings.csv")
            
            childRatingsCsv.Rows
                |> Seq.map parseChildRating
                |> Seq.toList