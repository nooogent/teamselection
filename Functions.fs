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
        
        // if number of teams don't match sticky club has to be the one with fewer teams 
        let generateFixtures pitches stickyTeams mobileTeams matchLength (dateTime:DateTime) tournamentLength =
            let numGames = int(floor (float (tournamentLength/matchLength)))
            //let intervalLength = 1<minute> * int((tournamentLength - (numGames * matchLength)) / (numGames - 1))
            
            let generateFixture teamsPitches mobileTeam =
                
                let generateRestFixture homeTeam awayTeam pitch startTime n (existingFixtures:Fixture list) =
//                    if(n = 0) then
//                        RestFixture(homeTeam,awayTeam,Duration(matchLength * 1<minute>),StartTime(startTime))::existingFixtures
//                    else
//                        let (Fixture(_,at,targetPitch,_,_)) = existingFixtures.[(List.length existingFixtures) - n]
//                        Fixture(homeTeam,at,targetPitch,Duration(matchLength * 1<minute>),StartTime(startTime))::(existingFixtures
//                        |> List.mapi (fun i f -> 
//                            match (i,f) with
//                            | (i,Fixture(ht,at',p,d,st)) when i = (List.length existingFixtures) - n -> 
//                                RestFixture(ht,awayTeam,d,st)
//                            | (_,_) -> 
//                                f))
                 let existingLength = List.length existingFixtures
                 if(n = 0) then
                    RestFixture(homeTeam,awayTeam,Duration(matchLength * 1<minute>),startTime)::existingFixtures
                 else
                    let (Fixture(ht,at,targetPitch,_,_)) = existingFixtures.[n - 1]
                    RestFixture(ht,awayTeam,Duration(matchLength * 1<minute>),startTime)::(existingFixtures
                    |> List.mapi (fun i f -> 
                        match (i,f) with
                        | (i,Fixture(_,at,p,d,st)) when i = (n - 1) -> 
                            Fixture(homeTeam,at,p,d,st)
                        | (_,_) -> 
                            f))

                match teamsPitches with
                | ([],[],n,st,z) -> 
                    ([],[],n,st,generateRestFixture Team.NoTeamAvailable mobileTeam None st n z)
                | ([],y::ys,n,st,z) -> 
                    ([],ys,n,st,generateRestFixture Team.NoTeamAvailable mobileTeam (Some y) st n z)
                | (x::xs,[],n,st,z) -> 
                    (xs,[],n,st,generateRestFixture x mobileTeam None st n z)
                | (x::xs,y::ys,n,st,z) -> 
                    (xs,ys,n,st,(Fixture(x,mobileTeam,y,Duration(matchLength * 1<minute>),st)::z))

            [
                for n in 0..(numGames - 1) do
                    yield!
                        mobileTeams
                        |> List.permute (fun i -> (i + n) % (List.length mobileTeams)) 
                        |> List.fold generateFixture (stickyTeams,pitches,n,StartTime(dateTime.AddMinutes(float (n * matchLength))),[]) 
                        |> function (_,_,_,_,fs) -> fs
                        |> List.rev
            ]