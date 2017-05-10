module Chef

open CafeApp.Domain
open ReadModel
open Projections
open Queries
open System.Collections.Generic
open System 
open Table

let private chefToDos = new Dictionary<Guid, ChefToDo>()

let private addFoodsToPrepare tabId foods =
    async { 
        let res = 
            match getTablebyTableId tabId with
            | Some table -> 
                let tab = { Id = tabId; TableNumber = table.Number }
                let todo : ChefToDo = { Tab = tab; Foods = foods }
                chefToDos.Add(tabId, todo)
            | None -> ()
        return res }

let private removeFood tabId food = 
    async {
        let todo = chefToDos.[tabId]
        let chefToDo = { todo with Foods = todo.Foods |> List.removeFirst(fun f -> f = food) }
        return chefToDos.[tabId] <- chefToDo }

let private remove tabId  =
    async { 
        return chefToDos.Remove tabId |> ignore 
    }

let chefActions = {
    AddFoodsToPrepare = addFoodsToPrepare
    RemoveFood = removeFood
    Remove = remove
}

let getChefToDos () = 
    async { 
        return chefToDos.Values |> Seq.toList
    }