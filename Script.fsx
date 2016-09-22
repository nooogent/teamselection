// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.
#r "../ponzi/packages/FSharp.Data.2.3.2/lib/net40/FSharp.Data.dll"
#load "Types.fs"
#load "Data.fs"
#load "Functions.fs"
#load "Helpers.fs"

open TeamSelection.Functions
open TeamSelection.Helpers
open TeamSelection.Types
open System

// Define your library scripting code here

let children = TeamSelection.Functions.getChildren
children

let coaches = TeamSelection.Functions.getCoaches
coaches

//
//let childRatings = TeamSelection.Functions.getChildRatings
//
//childRatings

let childRatingAverage = TeamSelection.Functions.getChildRatingAverages |> List.sortBy (fun (child,_) -> match child with | Child(cn) -> cn | ChildWithParent(cn,_) -> cn) 

//childRatingAverage
let getTeamName i =
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

let numPerTeam = 5
let numTeams = int (System.Math.Floor(float (children.Length / numPerTeam)))
let shuffledCoaches = coaches |> Seq.toArray |> Shuffle |> Seq.toList
let shuffledList = childRatingAverage |> List.toArray |> Shuffle |> Seq.toList

let calculateTeams children (coachList:Coach list) numTeams =
    children 
    |> Seq.mapi(fun i c -> (i,c)) 
    |> Seq.groupBy(fun (i,c) -> i % numTeams) 
    |> Seq.map (fun (g,s) -> HomeTeam(coachList.[g],(s |> Seq.map(fun (i,ch) -> ch) |> Seq.toList),getTeamName g))
    |> Seq.toList

let teams = calculateTeams (shuffledList |> Seq.map (fun (c,r) -> c)) shuffledCoaches numTeams   
teams
//     -> if i % numTeams = numTeams - 1 then Some(c) else None) |> Seq.choose id |> Seq.toList
//    let coach = coachList.[startIndex - 1]
//    HomeTeam(coach, teamMembers, teamName)


//let teams = [| for i in 1 .. numTeams -> calculateTeam shuffledList shuffledCoaches numTeams i |] |> Seq.toList
//teams

//let numPerTeam = 2
//let numTeams = childRatingAverage
//let teams = childRatingAverage2 |> List.fold()
