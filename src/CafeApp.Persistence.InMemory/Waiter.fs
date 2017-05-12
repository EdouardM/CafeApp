module Waiter

open CafeApp.Domain
open ReadModel
open Projections
open Queries
open System.Collections.Generic
open System 

open Table

let private waiterToDos = new Dictionary<Guid, WaiterToDo>()

let private addDrinksToServe tabId drinks = 
    async { 
        let res =
            match getTablebyTableId tabId with
            | Some table -> 
                let tab = { Id = tabId; TableNumber = table.Number } 
                let todo: WaiterToDo = {
                    Tab = tab;
                    Foods = [];
                    Drinks = drinks
                }
                waiterToDos.Add(tabId, todo)
            | None -> () 
        return res }
let private addFoodToServe tabId food = 
    async {
        let res = 
            if waiterToDos.ContainsKey tabId then
                let todo = waiterToDos.[tabId]
                let waiterTodo = {
                    todo with Foods = food::todo.Foods
                }
                waiterToDos.[tabId] <- waiterTodo
            else
                match getTablebyTableId tabId with
                | Some table -> 
                    let tab = { Id = tabId; TableNumber = table.Number } 
                    let todo: WaiterToDo = {
                        Tab = tab;
                        Foods = [ food ];
                        Drinks = []
                    }
                    waiterToDos.Add(tabId, todo)
                | None -> () 
        return res
    }    
let private markDrinkServed tabId drink = 
    async {
        let todo = waiterToDos.[tabId]
        let waiterTodo = {
            todo with Drinks = List.removeFirst (fun d -> d = drink) todo.Drinks
        }
        return ( waiterToDos.[tabId] <- waiterTodo )
    }

let private markFoodServed tabId food = 
    async {
        let todo = waiterToDos.[tabId]
        let waiterTodo = {
            todo with Foods = List.removeFirst (fun f -> f = food) todo.Foods 
        }
        return ( waiterToDos.[tabId] <- waiterTodo )
    }

let private remove tabId = 
    waiterToDos.Remove tabId
    |> ignore
    |> async.Return

let WaiterActions : WaiterActions =
    {
        AddDrinksToServe = addDrinksToServe
        MarkDrinkServed  = markDrinkServed
        AddFoodToServe   = addFoodToServe
        MarkFoodServed   = markFoodServed
        Remove           = remove
    }

let getwaiterToDos = 
    waiterToDos.Values
    |> Seq.toList
    |> async.Return

