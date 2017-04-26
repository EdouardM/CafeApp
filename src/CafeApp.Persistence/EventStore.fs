module EventStore
open CafeApp.State
open CafeApp.CommandHandlers

let getStateFromEvents events = 
    events |> Seq.fold evolve (ClosedTab None)

