namespace TeamSelection.Web

module Form =
    open Suave.Form

    type RequestType = RequestType of decimal * string

    let requestTypes = 
        [
            (1.0m, "Random")
            (2.0m, "Random - Coach with Own Child")
            (3.0m, "Streamed")
            (4.0m, "Streamed - Coach with Own Child")
            (5.0m, "Balanced")
            (6.0m, "Balanced - Coach with Own Child")
        ]

    type TSRequest = {
        TypeId : decimal
    }

    let tsRequest : Form<TSRequest> =
        Form ([],[])