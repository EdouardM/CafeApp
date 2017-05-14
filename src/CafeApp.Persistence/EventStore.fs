module EventStore
open CafeApp.State
open CafeApp.Events
open CafeApp.CommandHandlers

open NEventStore
open System

let getStateFromEvents events = 
    events |> Seq.fold evolve (ClosedTab None)

let getTabIdFromState = function
    | ClosedTab None            -> None
    | ClosedTab (Some tabId)    -> Some tabId
    | OpenedTab tab             -> Some tab.Id
    | OrderInProgress ipo       -> Some ipo.PlacedOrder.Tab.Id
    | PlacedOrder order         -> Some order.Tab.Id
    | ServedOrder order         -> Some order.Tab.Id

let saveEvent (storeEvents: IStoreEvents) state event =
    match getTabIdFromState state with
    | Some tabId -> 
        use stream = storeEvents.CreateStream(tabId.ToString())
        stream.Add(EventMessage(Body = event))
        stream.CommitChanges(Guid.NewGuid())
    | None -> ()

let saveEvents (storeEvents: IStoreEvents) state events =
    events
    |> List.iter (saveEvent storeEvents state)
    |> async.Return

let getEvents (storeEvents: IStoreEvents) (tabId: Guid) =
    use stream = storeEvents.OpenStream(tabId.ToString())
    stream.CommittedEvents
    |> Seq.map(fun evmsg -> evmsg.Body)
    |> Seq.cast<Event>

let getState storeEvents tabId = 
    let events = getEvents storeEvents tabId
    getStateFromEvents events
    |> async.Return

type EventStore = {
    GetState : Guid -> Async<State>
    SaveEvents : State -> Event list -> Async<unit>  
}