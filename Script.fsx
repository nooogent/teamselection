// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.
#r "../ponzi/packages/FSharp.Data.2.3.2/lib/net40/FSharp.Data.dll"
#load "Helpers.fs"
#load "Types.fs"
#load "Data.fs"
#load "Coach.fs"
#load "Child.fs"
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

// Define your library scripting code here

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

