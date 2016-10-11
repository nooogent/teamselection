namespace TeamSelection

    module Tournament = 

        open System
        open Helpers
        open Types

        let private assignRoundRobin numTeams orderedChildren =
            orderedChildren
            |> Seq.mapi(fun i c -> (i % numTeams,c)) 
            |> Seq.groupBy fst 
            |> Seq.map (fun (g,s) -> (s |> Seq.map snd))

        let private assignSequential numTeams orderedChildren =
            orderedChildren
            |> Seq.splitInto numTeams
            |> Seq.map Array.toSeq
            
        let private childComparer child1 child2 =
            match (child1,child2) with
            | Child.ChildWithParent(c1,p), Child.Child(c2) -> -1
            | Child.ChildWithParent(c1,p1), Child.ChildWithParent(c2,p2) -> 0
            | _,_ -> 1

        let private randomSelector children =
            children
            |> Helpers.ShuffleList
            |> Seq.map fst
            
        let private randomCoachWithChildSelector children =
            children
            |> Helpers.ShuffleList
            |> Seq.map fst
            |> Seq.sortWith childComparer
            
        let private rankedSelector children =
            children 
            |> Seq.sortByDescending snd
            |> Seq.map fst
            
        let private rankedCoachWithChildSelector children =
            children 
            |> rankedSelector
            |> Seq.sortWith childComparer

        let private randomCoachAssigner (shuffledCoachList:Coach list) teams =
            teams
            |> Seq.mapi(fun i t -> (t,shuffledCoachList.[i]))

        let private withChildCoachAssigner coachList teams = 
        
            let matchTeam co matcher team =
                match team with
                | (t,None) when matcher t -> ((t,Some co),true)
                | (_,_) -> (team,false)
                
            let rec matchTeams co matcher newTeams oldTeams = 
                match oldTeams with
                | [] ->
                    Seq.rev newTeams
                | head::tail ->
                    match (matchTeam co matcher head) with
                    | (team,matched) when matched -> Seq.append (Seq.rev(team::newTeams)) tail
                    | (_,_) -> matchTeams co matcher (head::newTeams) tail

            let assignCoachToOwnChildTeam teams coach =
                let matcher = Team.ContainsChild (Coach.MatchesChild coach)
                teams |> Seq.toList |> matchTeams coach matcher []

            let assignRemainingCoaches teams coach =
                let alreadyAssigned = 
                    teams 
                    |> Seq.exists (function | (_,Some c) when c = coach -> true | (_,_) -> false)

                let matcher = (fun t -> not alreadyAssigned)

                teams |> Seq.toList |> matchTeams coach matcher []

            let teamsWithoutCoaches = teams |> Seq.map(fun t -> (t,None))

            let teamsWithOwnChildCoachesAssigned = 
                coachList
                |> Seq.fold(assignCoachToOwnChildTeam) teamsWithoutCoaches

            coachList
            |> Seq.fold(assignRemainingCoaches) teamsWithOwnChildCoachesAssigned
            |> Seq.map(function | (t,Some c) -> (t,c) | (t,None) -> (t,Coach("No Coach")))

        let calculateTeams children coachList teamSelectionType numTeams =

            let selector =
                match teamSelectionType with
                | NotStreamed -> 
                    randomSelector
                | NotStreamedCoachWithChild -> 
                    randomCoachWithChildSelector
                | Streamed | Balanced  -> 
                    rankedSelector
                | StreamedCoachWithChild | BalancedCoachWithChild ->
                    rankedCoachWithChildSelector

            let teamAssigner =
                match teamSelectionType with
                | NotStreamed | Balanced | NotStreamedCoachWithChild | BalancedCoachWithChild -> 
                    assignRoundRobin
                | Streamed | StreamedCoachWithChild -> 
                    assignSequential
                
            let coachAssigner =
                match teamSelectionType with
                | NotStreamed | Streamed | Balanced -> 
                    randomCoachAssigner
                | NotStreamedCoachWithChild | StreamedCoachWithChild | BalancedCoachWithChild -> 
                    withChildCoachAssigner

            selector children
            |> teamAssigner numTeams
            |> coachAssigner (coachList |> ShuffleList)
            |> Seq.mapi(fun i (team,coach) -> 
                HomeTeam(coach,(team |> Seq.toList),Team.GetTeamName i))
