module Tests
open System
open Expecto
open CafeApp.Domain
open CafeApp.Events
open CafeApp.CommandHandlers

// Implement => to make the test run
let (=>) events command  = 
  events
  //Compute current state by folding events
  |> List.fold (evolve) (ClosedTab None) 
  |> decide command

let (==) actual expected = Expect.equal actual (Ok expected) "Events should match" 


[<Tests>]
let tests =
  testList "State Transitions" [
    testCase "Can Open a new Tab" <| fun _ ->
      let tab = { Id = new Guid.NewGuid() ; TableNumber = 1}

      [ OpenTab tab ]
      == [ TableOpened tab ]
      
      Expect.equal "F#" "F#" "We should access CafeApp.Core type."
  ]