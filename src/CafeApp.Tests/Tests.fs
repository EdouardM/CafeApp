module Tests
open System
open Expecto
open CafeApp.Result
open CafeApp.Errors
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

let (=!) actual error = Expect.equal actual (Failure error) "Errors should match"

[<Tests>]
let tests =
  testList "State Transitions" [
    testCase "Can Open a new Tab" <| fun _ ->
      let tab = { Id = Guid.NewGuid() ; TableNumber = 1}
      []
      =>  OpenTab tab
      == [ TabOpened tab ]

    testCase "Cannot open an already opened Tab" <| fun _ -> 
      let tab = { Id = Guid.NewGuid() ; TableNumber = 1}
      [ TabOpened tab ]
      =>  OpenTab tab
      =! TableAlreadyOpened
      
  ]