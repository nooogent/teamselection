namespace TeamSelection

    module Types = 

        open System

        [<Measure>] type minute

        type Coach = Coach of string
        type ChildName = ChildName of string
        type ClubName = ClubName of string
        type TeamName = TeamName of string
        type Pitch = Pitch of string

        type Rating =
            | One = 1
            | Two = 2
            | Three = 3
            | Four = 4
            | Five = 5
            
        type TeamSelectionType = 
            | NotStreamed
            | NotStreamedCoachWithChild
            | Streamed
            | StreamedCoachWithChild
            | Balanced
            | BalancedCoachWithChild

        type Parent = Parent of Coach

        type Child = 
            | Child of ChildName
            | ChildWithParent of ChildName * Parent

        type ChildRating = ChildRating of Child * Rating * Coach
        
        type Team = 
            | HomeTeam of Coach * Child list * TeamName
            | AwayTeam of TeamName

        type Duration = Duration of int<minute>
        type StartTime = StartTime of System.DateTime

        type Fixture = Fixture of Team * Team * Pitch * Duration * StartTime

        type Tournament = {
            HomeClub: ClubName;
            AwayClub: ClubName;
            HomeTeams: Team list;
            AwayTeams: Team list;
            Pitches: Pitch list;
            Fixtures: Fixture list;
        }