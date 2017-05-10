[<AutoOpen>]
module List

        ///helper to remove first occurence from list
        let rec removeFirst pred lst =
            match lst with
            | h::t when pred h -> t
            | h::t -> h::removeFirst pred t
            | _ -> []