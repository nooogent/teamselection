namespace TeamSelection.Web

module Path =

//    type IntPath = PrintfFormat<(int -> string),unit,string,string,int>
//    
//    let withParam (key,value) path = sprintf "%s?%s=%s" path key value

    let home = "/"
    
    module Team =
        let generate = "/generate"
//        let browse = "/store/browse"
//        let details : IntPath = "/store/details/%d"
//        let browseKey = "genre"