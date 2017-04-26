module Projections
open CafeApp.Events
open CafeApp.Domain
open System

(*
To answer the queries that we had at the beginning of this section, we need to persist these ReadModels  in a storage and retrieve it for the queries.
The interesting part how are we going to populate this data. We are going to leverage the Event  returned from the  Evolve  function to do it.
An  Event  represents an activity happened in the application. As each event carries its associated data, we can use it to populate our read models.
For example, given the event is  TabOpened , We can use the  Id  and the  TableNumber from the  Tab  data of the  TabOpened  to update the status of a table.
Manipulating ReadModels using Events is called Projecting Read Models. As a good design practice, The projection logic should avoid interacting 
directly with the underlying storage. 
*)

//Abstraction hiding the storage
type TableActions = {
    OpenTab         : Tab -> Async<unit>
    ReceivedOrder   : Guid -> Async<unit>
    CloseTab        : Tab -> Async<unit>
}

type ChefActions = {
    AddFoodsToPrepare   : Guid -> Food list -> Async<unit>
    RemoveFood          : Guid -> Food -> Async<unit>
    Remove              : Guid -> Async<unit>
}

type WaiterActions = {
    AddDrinksToServe : Guid -> Drink list -> Async<unit>
    MarkDrinksServed : Guid -> Drink -> Async<unit>
    AddFoodToServe   : Guid -> Food -> Async<unit>
    MarkFoodsServed  : Guid -> Food -> Async<unit>
    Remove           : Guid -> Async<unit>
}

type CashierActions = {
    AddTabAmount : Guid -> decimal -> Async<unit>
    Remove       : Guid -> Async<unit>
}

type ProjectionActions = {
    Table   : TableActions
    Waiter  : WaiterActions
    Chef    : ChefActions
    Cashier : CashierActions
}

let projectReadModel actions = function
    | TabOpened tab -> 
        //Expose output of action by calling Async.Parallel 
        [ actions.Table.OpenTab tab ] |> Async.Parallel
    | OrderPlaced order ->
        [   
            actions.Table.ReceivedOrder order.Tab.Id;
            actions.Waiter.AddDrinksToServe order.Tab.Id order.Drinks; 
            actions.Chef.AddFoodsToPrepare order.Tab.Id order.Foods  
        ] |> Async.Parallel
    | DrinkServed (drink, tabId) -> 
        [   actions.Waiter.MarkDrinksServed tabId drink ]
        |> Async.Parallel
    | FoodPrepared (food, tabId) -> 
        [   actions.Chef.RemoveFood tabId food ;
            actions.Waiter.AddFoodToServe tabId food
        ] |> Async.Parallel
    | FoodServed (food, tabId) ->
        [ actions.Waiter.MarkFoodsServed tabId food ]
        |> Async.Parallel

    | OrderServed (order, payment) -> 
        [
            actions.Chef.Remove order.Tab.Id;
            actions.Waiter.Remove order.Tab.Id; 
            actions.Cashier.AddTabAmount order.Tab.Id payment.Amount
        ] |> Async.Parallel
    | TabClosed payment -> 
        [
            actions.Table.CloseTab payment.Tab;
            actions.Cashier.Remove payment.Tab.Id
        ] |> Async.Parallel
    


