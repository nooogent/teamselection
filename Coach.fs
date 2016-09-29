namespace TeamSelection
    
    module Coach =

        open Types

        let MatchesChild coach child =
            match child with 
            | Child.ChildWithParent(_,Parent(co)) when co = coach -> true 
            | _ -> false