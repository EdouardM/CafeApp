module CommandApi

open CommandHandlers
open OpenTab
open Queries

let handleCommandRequest queries eventStore = 
    function
    | OpenTabRequest tab -> 
        queries.Table.GetTableByTableNumber
        |> openTabCommander
        |> handleCommand eventStore tab
    | _ -> (CafeApp.Result.Failure <| err "Invalid Command") |> async.Return 