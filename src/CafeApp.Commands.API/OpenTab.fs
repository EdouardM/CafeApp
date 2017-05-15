module OpenTab

open FSharp.Data
open CafeApp.Domain
open CafeApp.Commands
open CommandHandlers
open ReadModel
open System

//Let's start by defining the JSON payload for  OpenTab  and parsing that to a type.
[<Literal>]
let OpenTabJson = """{
    "openTab" : {
        "tableNumber": 1
    }
}"""

type OpenTabReq = JsonProvider<OpenTabJson>

let (|OpenTabRequest|_|) payload =
    try
        let req = OpenTabReq.Parse(payload).OpenTab
        { Id = Guid.NewGuid() ; TableNumber = req.TableNumber }
        |> Some
    with 
        ex -> None

let validateOpenTab (getTableByTableNumber: int -> Async<Table option>) tab =  async {
    let! result = getTableByTableNumber tab.TableNumber
    match result with
    | Some table -> return Choice1Of2 tab
    | None       -> return Choice2Of2 "Invalid Table Number" 
}

let openTabCommander (getTableByTableNumber: int -> Async<Table option>) = {
    Validate = validateOpenTab getTableByTableNumber
    ToCommand = OpenTab
}