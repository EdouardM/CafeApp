module Queries

open ReadModel
open CafeApp.Domain
open CafeApp.State
open System

(*

Once we are done with the projection of ReadModels, the next step is exposing it to the outside world as queries

*)

type TabQueries = {
    GetTables : unit -> Async<Table list>
}

type ToDoQueries = {
    GetChefToDos    : unit -> Async<ChefToDo list>
    GetWaiterToDos  : unit -> Async<WaiterToDo list>
    GetCashierToDos : unit -> Async<CashierToDo list>
}

type Queries = {
    Table   : TabQueries
    ToDo    : ToDoQueries
}
