namespace TeamSelection

    module Team = 

        open Types
        
        let GetTeamName i =
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
             
        let ContainsChild childFinder team =
            team |> Seq.exists childFinder