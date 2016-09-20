// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.
#r "../ponzi/packages/FSharp.Data.2.3.2/lib/net40/FSharp.Data.dll"
#load "Types.fs"
#load "Data.fs"
#load "Functions.fs"

open TeamSelection.Functions

// Define your library scripting code here

let children = TeamSelection.Functions.getChildren

children

let childRatings = TeamSelection.Functions.getChildRatings

childRatings