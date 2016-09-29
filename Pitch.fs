namespace TeamSelection

    module Pitch =

        open Types

        let GetPitch (i:int) =
            Pitch(sprintf "Pitch %i" i)