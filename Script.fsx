// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.
#r "../ponzi/packages/FSharp.Data.2.3.2/lib/net40/FSharp.Data.dll"
#load "Helpers.fs"
#load "Types.fs"
#load "Data.fs"
#load "Coach.fs"
#load "Child.fs"
#load "Pitch.fs"
#load "Team.fs"
#load "Tournament.fs"
#load "Functions.fs"

open TeamSelection.Data
open TeamSelection.Types
open TeamSelection.Functions
open TeamSelection.Child
open TeamSelection.Tournament
open System
open TeamSelection.Helpers

let c = 
    TeamSelection.Functions.generateFixtures  
        (TeamSelection.Functions.generatePitches 5)
        [
            HomeTeam(Coach("Bob"),[],TeamName("ATeam1"));
            HomeTeam(Coach("George"),[],TeamName("ATeam2"));
            HomeTeam(Coach("Paul"),[],TeamName("ATeam3"));
            HomeTeam(Coach("Helen"),[],TeamName("ATeam4"));
            HomeTeam(Coach("Sinead"),[],TeamName("ATeam5"));
            HomeTeam(Coach("Liam"),[],TeamName("ATeam6"))
        ] 
        [AwayTeam(TeamName("BTeam1"));AwayTeam(TeamName("BTeam2"));AwayTeam(TeamName("BTeam3"));AwayTeam(TeamName("BTeam4"));AwayTeam(TeamName("BTeam5"));AwayTeam(TeamName("BTeam6"))] 
        9
        DateTime.Now
        60
        |> List.map (
            function 
            | Fixture(HomeTeam(_,_,TeamName(ht)),AwayTeam(TeamName(at)),Pitch(p),d,st) -> 
                printf "%s %s %s\n" ht at p 
            | RestFixture(HomeTeam(_,_,TeamName(ht)),AwayTeam(TeamName(at)),d,st) ->
                 printf "%s %s None\n" ht at
            | RestFixture(HomeTeam(_,_,TeamName(ht)),NoTeamAvailable,d,st) -> 
                printf "%s None None\n" ht 
            | RestFixture(NoTeamAvailable,AwayTeam(TeamName(at)),d,st) -> 
                printf "None %s None\n" at
            | Fixture(_,_,_,_,_) -> 
                printf "Invalid Fixture\n"
            | RestFixture(_,_,_,_) -> 
                printf "Invalid Rest Fixture\n")

for n in 0..5 do
    printf "%O" (List.permute (fun i -> (i + n) % 6) [1..6])

let pitches = TeamSelection.Functions.generatePitches 5

let children = TeamSelection.Functions.getChildren
children

let coaches = TeamSelection.Functions.getCoaches
coaches


let childRatings = TeamSelection.Functions.getChildRatings

childRatings

//let list1 = [1..10]
//let list2 = [1..9]
//
//let joined = list1 |> LeftJoin (=) (fun a b -> (a,b)) (fun a -> (a,0)) list2

let childRatingAverage = TeamSelection.Functions.getChildRatingAverages |> List.sortBy (fun (c,_) -> GetName c) 

let numPerTeam = 5
let numTeams = int (System.Math.Floor(float (children.Length / numPerTeam)))


let teams = 
    calculateTeams
        childRatingAverage
        coaches 
        TeamSelectionType.StreamedCoachWithChild 
        numTeams 
    |> Seq.toList
//     -> if i % numTeams = numTeams - 1 then Some(c) else None) |> Seq.choose id |> Seq.toList
//    let coach = coachList.[startIndex - 1]
//    HomeTeam(coach, teamMembers, teamName)

//let teams = [| for i in 1 .. numTeams -> calculateTeam shuffledList shuffledCoaches numTeams i |] |> Seq.toList
//teams

