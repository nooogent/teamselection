namespace TeamSelection
    module Helpers = 
        open System

        // shuffle an array (in-place)
        let Shuffle array = 
            let rand = new System.Random()

            let swap (a: _[]) x y =
                let tmp = a.[x]
                a.[x] <- a.[y]
                a.[y] <- tmp

            Array.iteri (fun i _ -> swap array i (rand.Next(i, Array.length array))) array

            array

