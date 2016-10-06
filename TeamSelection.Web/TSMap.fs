namespace TeamSelection.Web

module TSMap = 

    open TeamSelection.Types

    let mapTeamSelectionType typeId = 
        match typeId with
        | 2.0m -> TeamSelectionType.NotStreamedCoachWithChild
        | 3.0m -> TeamSelectionType.Streamed
        | 4.0m -> TeamSelectionType.StreamedCoachWithChild
        | 5.0m -> TeamSelectionType.Balanced
        | 6.0m -> TeamSelectionType.BalancedCoachWithChild
        | _ -> TeamSelectionType.NotStreamed