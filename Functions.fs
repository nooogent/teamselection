namespace TeamSelection

    module Functions =

        open System
        open Helpers
        open TeamSelection.Types
        open TeamSelection.Data

        let getChildren = getChildren
        
        let getCoaches = getCoaches

        let getChildRatings = getChildRatings

        let getChildRatingAverages = 
            
            let allChildren = getChildren
            let childRatings = getChildRatings

            let childRatingAverages = 
                childRatings
                |> Seq.groupBy (fun (ChildRating (child,_,_)) -> child)
                |> Seq.map (fun (child,childRatings) -> 
                  (child,
                    childRatings
                    |> Seq.averageBy (fun (ChildRating (_,rating,_)) -> float rating)
                    |> Round 2))
                |> Seq.toList
            
            allChildren
            |> LeftJoin 
                (fun c (c2,_) -> Child.Equals c c2) 
                (fun c (_,r) -> (c,r))
                (fun c -> (c,(float Rating.Three)))
                childRatingAverages

//            let getChildRatingAverage child childRatingAverages =
//                childRatingAverages
//                |> List.tryPick(fun (ratingChild,averageRating) -> if Child.GetName ratingChild = Child.GetName child then Some(averageRating) else None)
//
//            allChildren
//            |> Seq.map(fun child -> (child, match getChildRatingAverage child childRatingAverages with | Some av -> av | None -> float Rating.Three))
//            |> Seq.toList
        
