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
             
    module Tournament =
        open Types
        open Helpers

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

        let private withChildCoachAssigner (shuffledCoachList:Coach[]) teams = 
            let findCoachForTeamsWithParentedChildren team coachList =
                team
                |> Seq.map(fun c -> match c with | Child.ChildWithParent(_,Parent(co)) -> Some(co) | _ -> None)
                |> Seq.choose id
                |> Seq.tryFind(fun co -> Seq.contains co coachList)

            let removeCoach coachToRemove coachList = 
                coachList
                |> Seq.filter (fun co ->  co <> Option.get coachToRemove)

            teams
            |> Seq.mapFold(fun cl t -> 
                (t,(findCoachForTeamsWithParentedChildren t cl)),
                (removeCoach (findCoachForTeamsWithParentedChildren t cl) cl)) (shuffledCoachList |> Array.toSeq)
            |> fst 
            |> Seq.map (fun tc ->
                tc
                |> Seq.choose(Option.isNone (snd tc))
                |> Seq.fold(fun ccll tctc -> 
                    ((fst tctc),(Seq.head ccll)), 
                    (Seq.tail ccll) snd))
            
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