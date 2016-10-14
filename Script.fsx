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
open System.IO
open TeamSelection.Helpers

let pitches = TeamSelection.Functions.generatePitches 6

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

//let numPerTeam = 5
let numTeams = 6//int (System.Math.Floor(float (children.Length / numPerTeam)))
let HHmm (tw:TextWriter) (date:StartTime) = tw.Write("{0:HH:mm}", match date with | StartTime(d) -> d)
let c = 
    (TeamSelection.Functions.generateFixtures  
        pitches
        [AwayTeam(TeamName("Beechwood Team 1"));AwayTeam(TeamName("Beechwood Team 2"));AwayTeam(TeamName("Beechwood Team 3"));AwayTeam(TeamName("Beechwood Team 4"));AwayTeam(TeamName("Beechwood Team 5"));AwayTeam(TeamName("Beechwood Team 6"))] 
        ((calculateTeams childRatingAverage coaches TeamSelectionType.BalancedCoachWithChild numTeams "Belmont") |> Seq.toList)
        15
        (new System.DateTime(2016,10,15,9,0,0))
        60)
        |> List.map (
            function 
            | Fixture(HomeTeam(_,_,TeamName(ht)),AwayTeam(TeamName(at)),Pitch(p),d,st) -> 
                printf "%s %s %s %a\n" ht at p HHmm st
            | RestFixture(HomeTeam(_,_,TeamName(ht)),AwayTeam(TeamName(at)),d,st) ->
                 printf "%s %s None %a\n" ht at HHmm st
            | Fixture(AwayTeam(TeamName(at)),HomeTeam(_,_,TeamName(ht)),Pitch(p),d,st) -> 
                printf "%s %s %s %a\n" ht at p HHmm st
            | RestFixture(AwayTeam(TeamName(at)),HomeTeam(_,_,TeamName(ht)),d,st) ->
                 printf "%s %s None %a\n" ht at HHmm st
            | RestFixture(HomeTeam(_,_,TeamName(ht)),NoTeamAvailable,d,st) -> 
                printf "%s None None %a\n" ht HHmm st 
            | RestFixture(NoTeamAvailable,AwayTeam(TeamName(at)),d,st) -> 
                printf "None %s None %a\n" at HHmm st
            | Fixture(_,_,_,_,_) -> 
                printf "Invalid Fixture\n"
            | RestFixture(_,_,_,_) -> 
                printf "Invalid Rest Fixture\n")

for n in 0..5 do
    printf "%O" (List.permute (fun i -> (i + n) % 6) [1..6])

let pitches = TeamSelection.Functions.generatePitches 6

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

//let numPerTeam = 5
let numTeams = 6//int (System.Math.Floor(float (children.Length / numPerTeam)))


let teams = 
    calculateTeams
        childRatingAverage
        coaches 
        TeamSelectionType.BalancedCoachWithChild 
        numTeams
        "Belmont" 
    |> Seq.toList
//     -> if i % numTeams = numTeams - 1 then Some(c) else None) |> Seq.choose id |> Seq.toList
//    let coach = coachList.[startIndex - 1]
//    HomeTeam(coach, teamMembers, teamName)

//let teams = [| for i in 1 .. numTeams -> calculateTeam shuffledList shuffledCoaches numTeams i |] |> Seq.toList
//teams

