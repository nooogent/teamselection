namespace TeamSelection
    module Types = 
        open System

        type Coach = Coach of string
        type Parent = Parent of Coach
        type ChildName = ChildName of string
        type Child = 
            | Child of ChildName
            | ChildWithParent of ChildName * Parent

        type Rating =
            | One = 1
            | Two = 2
            | Three = 3
            | Four = 4
            | Five = 5

        type ChildRating = ChildRating of Child * Rating * Coach
        
        type ClubName = ClubName of string
        type TeamName = TeamName of string
        type Pitch = Pitch of string

        type Team = 
            | HomeTeam of Coach * Child list * TeamName
            | AwayTeam of TeamName

        type Fixture = Fixture of Team * Team * Pitch

        type Tournament = {
            HomeClub: ClubName;
            AwayClub: ClubName;
            HomeTeams: Team list;
            AwayTeams: Team list;
            Pitches: Pitch list;
            Fixtures: Fixture list;
        }

        type TeamSelectionType = 
            | NotStreamed
            | NotStreamedCoachWithChild
            | Streamed
            | StreamedCoachWithChild

    module Coach =
        open Types

        let MatchesChild coach child =
            match child with 
            | Child.ChildWithParent(_,Parent(co)) when co = coach -> true 
            | _ -> false

    module Child =
        open Types

        let GetName child =
            match child with
            | Child (childName) -> childName
            | ChildWithParent (childName, _) -> childName
            
    module Team =
        open Types
        
        let GetTeamName i =
            match i with
            | 0 -> TeamName("Team A")
            | 1 -> TeamName("Team B")
            | 2 -> TeamName("Team C")
            | 3 -> TeamName("Team D")
            | 4 -> TeamName("Team E")
            | 5 -> TeamName("Team F")
            | 6 -> TeamName("Team G")
            | 7 -> TeamName("Team H")
            | 8 -> TeamName("Team I")
            | 9 -> TeamName("Team J")
            | _ -> TeamName("Too many teams")
             
        let ContainsChild childFinder team =
            team |> Seq.exists childFinder

    module Tournament =
        open System
        open Helpers
        open Types
        open Coach

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
            | Child.ChildWithParent(c2,p), Child.Child(c1) -> -1
            | Child.ChildWithParent(c1,p1), Child.ChildWithParent(c2,p2) -> 0
            | _,_ -> 1

        let private notStreamedSelector children =
            children
            |> Helpers.Shuffle
            |> Seq.map fst
            
        let private notStreamedCoachWithChildSelector children =
            children
            |> Helpers.Shuffle
            |> Seq.map fst
            |> Seq.sortWith childComparer
            
        let private streamedSelector children =
            children 
            |> Seq.sortByDescending snd
            |> Seq.map fst
            
//        let private streamedCoachWithChildSelector children =
//            children 
//            |> Seq.sortByDescending snd
//            |> Seq.map fst
//            |> Seq.sortWith childComparer

        let private randomCoachAssigner (shuffledCoachList:Coach[]) teams =
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
                    | (team,matched) when matched -> Seq.append (team::tail) newTeams
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
            |> Seq.map(function | (t,Some c) -> (t,c) | (t,None) -> (t,Coach("NoCoach")))

        let calculateTeams children coachList teamSelectionType numTeams =
            let selector =
                match teamSelectionType with
                | TeamSelectionType.NotStreamed -> notStreamedSelector
                | TeamSelectionType.NotStreamedCoachWithChild -> notStreamedCoachWithChildSelector
                | TeamSelectionType.Streamed | TeamSelectionType.StreamedCoachWithChild -> streamedSelector
                
            let teamAssigner =
                match teamSelectionType with
                | TeamSelectionType.NotStreamed | TeamSelectionType.NotStreamedCoachWithChild -> assignRoundRobin
                | TeamSelectionType.Streamed | TeamSelectionType.StreamedCoachWithChild -> assignSequential
                
            let coachAssigner =
                match teamSelectionType with
                | TeamSelectionType.NotStreamed | TeamSelectionType.Streamed -> randomCoachAssigner
                | TeamSelectionType.NotStreamedCoachWithChild | TeamSelectionType.StreamedCoachWithChild -> withChildCoachAssigner

            selector children
            |> teamAssigner numTeams
            |> coachAssigner (coachList |> Shuffle)
            |> Seq.mapi(fun i (team,coach) -> HomeTeam(coach,(team |> Seq.toList),Team.GetTeamName i))