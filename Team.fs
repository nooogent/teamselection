namespace TeamSelection

    module Team = 

        open Types
        
        let GetTeamName i prefix =
            TeamName(sprintf "%s Team %i" prefix i)
             
        let ContainsChild childFinder team =
            team |> Seq.exists childFinder