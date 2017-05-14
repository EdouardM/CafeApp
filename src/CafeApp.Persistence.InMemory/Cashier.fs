module Cashier

open CafeApp.Domain
open ReadModel
open Projections
open Queries
open System.Collections.Generic
open System 

open Table

let cashierToDos = new Dictionary<Guid, Payment>()

let private addTabAmount tabId amount = 
    async {
        let res = 
            match getTablebyTableId tabId with
            | Some tab -> 
                cashierToDos.Add(tabId, { Tab = { Id = tabId ; TableNumber = tab.Number }  ; Amount = amount })
            | None -> 
                ()
        return res
    }
let private removeTabId tabId = 
    cashierToDos.Remove tabId
    |> ignore
    |> async.Return

let cashierActions = {
    AddTabAmount = addTabAmount
    Remove = removeTabId
}

let getCashierToDos () = 
    async { 
        return cashierToDos.Values |> Seq.toList
    }