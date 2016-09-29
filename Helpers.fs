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

        let ShuffleList list1 = 
            list1 |> List.toArray |> Shuffle |> Array.toList

        let ShuffleSeq seq1 = 
            seq1 |> Seq.toArray |> Shuffle |> Array.toSeq

        let LeftJoin joiner getter getDefault list2 list1 =

            let rec matchItems joiner getter l2 l1i = 
                [
                    match l2 with
                    | [] -> ()
                    | head::tail ->
                        if (joiner l1i head) then
                            yield getter l1i head
                        yield! matchItems joiner getter tail l1i
                ]
 
            [
                for i1 in list1 do
                    yield! 
                        match (matchItems joiner getter list2 i1) with 
                        | [] -> [ getDefault i1 ] 
                        | list -> list
            ]

        let Round (digits:int) (number:float) =
            Math.Round(number, digits)

