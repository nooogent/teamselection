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
        let generateFixtures pitches stickyTeams mobileTeams matchLength startTime tournamentLength =
            let numGames = int(floor (float (tournamentLength/matchLength)))
            //let intervalLength = 1<minute> * int((tournamentLength - (numGames * matchLength)) / (numGames - 1))
            
            let generateFixture teamsPitches mobileTeam =
                
                let generateRestFixture homeTeam awayTeam pitch n existingFixtures =
                    if(n = (List.length existingFixtures)) then
                        RestFixture(homeTeam,awayTeam,Duration(matchLength * 1<minute>),StartTime(startTime))::existingFixtures
                    else
                        let (Fixture(_,_,targetPitch,_,_)) = existingFixtures.[(List.length existingFixtures) - 1 - n]
                        Fixture(homeTeam,awayTeam,targetPitch,Duration(matchLength * 1<minute>),StartTime(startTime))::(existingFixtures
                        |> List.mapi (fun i f -> 
                            match (i,f) with
                            | (i,Fixture(ht,at,p,d,st)) when (List.length existingFixtures) - i = n -> 
                                RestFixture(ht,mobileTeam,d,st)
                            | (_,_) -> 
                                f))
                
                match teamsPitches with
                | ([],[],n,z) -> 
                    ([],[],n,generateRestFixture Team.NoTeamAvailable mobileTeam None n z)
                    //RestFixture(Team.NoTeamAvailable,mobileTeam,Duration(matchLength * 1<minute>),StartTime(startTime))::z)
                | ([],y::ys,n,z) -> 
                    ([],ys,n,generateRestFixture Team.NoTeamAvailable mobileTeam (Some y) n z)
                    //([],ys,RestFixture(Team.NoTeamAvailable,mobileTeam,Duration(matchLength * 1<minute>),StartTime(startTime))::z)
                | (x::xs,[],n,z) -> 
                    (xs,[],n,generateRestFixture x mobileTeam None n z)
                    //(xs,[],RestFixture(x,mobileTeam,Duration(matchLength * 1<minute>),StartTime(startTime))::z)
                | (x::xs,y::ys,n,z) -> 
                    (xs,ys,n,Fixture(x,mobileTeam,y,Duration(matchLength * 1<minute>),StartTime(startTime))::z)

            [
                for n in 0..(numGames - 1) do
                    yield!
                        mobileTeams
                        |> List.permute (fun i -> (i + n) % (List.length mobileTeams)) 
                        |> List.fold generateFixture (stickyTeams,pitches,n,[])
                        |> function (_,_,_,fs) -> fs
                        |> List.rev
            ]