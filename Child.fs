namespace TeamSelection
    
    module Child =

        open Types

        let GetName child =
            match child with
            | Child (childName) -> childName
            | ChildWithParent (childName, _) -> childName

        let Equals child1 child2 =
            GetName child1 = GetName child2