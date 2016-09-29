namespace TeamSelection

    module Functions =

        open System
        open Helpers
        open TeamSelection.Types
        open TeamSelection.Data
        open TeamSelection.Pitch

        let getChildren = Data.getChildren
        
        let getCoaches = Data.getCoaches

        let getChildRatings = Data.getChildRatings

        let getChildRatingAverages = 
            
            let childRatingAverages = 
                getChildRatings
                |> Seq.groupBy (fun (ChildRating (child,_,_)) -> child)
                |> Seq.map (fun (child,childRatings) -> 
                  (child,
                    childRatings
                    |> Seq.averageBy (fun (ChildRating (_,rating,_)) -> float rating)
                    |> Helpers.Round 2))
                |> Seq.toList
            
            getChildren
            |> Helpers.LeftJoin 
                (fun c (c2,_) -> Child.Equals c c2) 
                (fun c (_,r) -> (c,r))
                (fun c -> (c,(float Rating.Three)))
                childRatingAverages
        
        let generatePitches numPitches =
            [1..numPitches] |> List.map Pitch.GetPitch
            
//        let generateFixtures pitches homeTeams awayTeams matchLength startTime tournamentLength intervalLength =
//            let rec check w x y z = 
//                if (x - (z * w)) < ((z - 1) * y) then check w x y (z - 1)
//                else z
//            let numGames = check matchLength tournamentLength intervalLength (int(floor (float (tournamentLength/matchLength))))
//            numGames
            
        let generateFixtures pitches homeTeams awayTeams matchLength startTime tournamentLength =
            let numGames = int(floor (float (tournamentLength/matchLength)))
            let intervalLength = 1<minute> * int((tournamentLength - (numGames * matchLength)) / (numGames - 1))

            let generateFixture teamsPitches homeTeam =
                let awayTeams,pitches,fixtures = teamsPitches
                let fixture=Fixture(homeTeam,List.head awayTeams,List.head pitches,Duration(intervalLength),StartTime(startTime))
                (List.tail awayTeams,List.tail pitches,fixture::fixtures)

            homeTeams 
            |> List.fold generateFixture (awayTeams,pitches,[])