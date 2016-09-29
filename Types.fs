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
            | Balanced
            | BalancedCoachWithChild