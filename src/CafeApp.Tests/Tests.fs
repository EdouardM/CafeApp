module Tests
open System
open Expecto
open FsCheck
open CafeApp.Result
open CafeApp.Errors
open CafeApp.Domain
open CafeApp.Events
open CafeApp.CommandHandlers

open Generators

// Implement => to make the test run
let (=>) events command  = 
  events
  //Compute current state by folding events
  |> List.fold (evolve) (ClosedTab None) 
  |> decide command

let (==) actual expected = Expect.equal actual (Ok expected) "Events should match" 

let (=!) actual error = Expect.equal actual (Failure error) "Errors should match"

[<Tests>]
let OpenTabTests =
  testList "Open Tab Transition" [
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
[<Tests>]
let PlaceOrderTests = 
  testList "Place Order Transition" [
    testProperty "Can place only drinks order if tab opened" <| fun _ -> 
      let tab = { Id = Guid.NewGuid() ; TableNumber = 12 }
      let arb = genDrinksNonEmpty |> Arb.fromGen
      
      Prop.forAll arb ( fun drinks -> 
      let order = { Tab = tab; Drinks = drinks ; Foods = [] }
      [ TabOpened tab ]
      => PlaceOrder order
      == [ OrderPlaced order ] )

    testProperty "Can place only foods order if tab opened" <| fun _ -> 
      let tab = { Id = Guid.NewGuid() ; TableNumber = 12 }
      let arb = genFoodsNonEmpty |> Arb.fromGen

      Prop.forAll arb (fun foods -> 
      let order = { Tab = tab; Drinks = []  ; Foods = foods }
      [ TabOpened tab ]
      => PlaceOrder order
      == [ OrderPlaced order ] )
  
    testProperty "Can place any order if tab opened" <| fun _ -> 
      let tab = { Id = Guid.NewGuid() ; TableNumber = 12 }
      let arb = genOrder |> Arb.fromGen
      
      Prop.forAll arb (fun order -> 
      [ TabOpened tab ]
      => PlaceOrder order
      == [  OrderPlaced order] )
    
    testCase "Cannot place empty order" <| fun _ -> 
        let tab = { Id = Guid.NewGuid() ; TableNumber = 1}
        let order = { Tab = tab; Drinks = []; Foods = [] }
        [ TabOpened tab ]
        =>  PlaceOrder order
        =! CannotPlaceEmptyOrder
    
    testCase "Cannot place order on closed table" <| fun _ -> 
        let tab = { Id = Guid.NewGuid() ; TableNumber = 1}
        let order = { Tab = tab; Drinks = []; Foods = [] }
        []
        =>  PlaceOrder order
        =! CannotOrderWithClosedTable
      
    testCase "Cannot place order multiple times" <| fun _ -> 
        let tab = { Id = Guid.NewGuid() ; TableNumber = 1}
        let drink = Drink { MenuNumber = 1; Price = 2.5m; Name = "Coke" }
        let order = { Tab = tab; Drinks = [drink]; Foods = [] }
        
        [TabOpened tab; OrderPlaced order  ]
        =>  PlaceOrder order
        =! OrderAlreadyPlaced
      
  ]