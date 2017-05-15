module InMemory

open NEventStore

open EventStore
open Queries
open Chef
open Waiter
open Cashier
open Table 
open Projections

//For other implementations go visit: 
//https://github.com/NEventStore/NEventStore-Example/blob/master/NEventStore.Example/MainProgram.cs#L76
//For RabenDB storage go to: 
//http://stackoverflow.com/questions/6130790/wiring-up-jolivers-eventstore-using-ravendb-persistence-plugin/8787610#8787610


type InMemoryEventStore () = 
    static member Instance = 
        Wireup.Init()
                .UsingInMemoryPersistence()
                .Build()

let inMemoryEventStore () = 
    let eventStore = InMemoryEventStore.Instance
    {
        GetState = getState eventStore
        SaveEvents = saveEvents eventStore
    }

let toDoQueries = {
    GetChefToDos = getChefToDos
    GetWaiterToDos = getwaiterToDos
    GetCashierToDos = getCashierToDos
}  

let inMemoryQueries = {
    Table = tableQueries
    ToDo  = toDoQueries
}

let inMemoryActions = {
    Table   = tableActions
    Waiter  = waiterActions
    Chef    = chefActions
    Cashier = cashierActions
}

