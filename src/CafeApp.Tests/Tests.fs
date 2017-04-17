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

    testProperty "Can place only drinks order if tab opened" <| fun _ -> 
      let tab = { Id = Guid.NewGuid() ; TableNumber = 12 }
      let arb = genDrinksNonEmpty |> Arb.fromGen
      
      Prop.forAll arb ( fun drinks -> 
      [ TabOpened tab ]
      => PlaceOrder { Tab = tab; Drinks = drinks ; Foods = [] }
      == [ DrinksOrdered { Id = tab.Id; Drinks = drinks } ] )

    testProperty "Can place only foods order if tab opened" <| fun _ -> 
      let tab = { Id = Guid.NewGuid() ; TableNumber = 12 }
      let arb = genFoodsNonEmpty |> Arb.fromGen

      Prop.forAll arb (fun foods -> 
      [ TabOpened tab ]
      => PlaceOrder { Tab = tab; Drinks = [] ; Foods = foods }
      == [ FoodsOrdered { Id = tab.Id; Foods = foods  } ])
  
    testProperty "Can place any order if tab opened" <| fun _ -> 
      let tab = { Id = Guid.NewGuid() ; TableNumber = 12 }
      let arb = genDrinksAndFoodsNonEmpty |> Arb.fromGen
      
      Prop.forAll arb (fun (drinks, foods) -> 
      [ TabOpened tab ]
      => PlaceOrder { Tab = tab; Drinks = drinks ; Foods = foods }
      == [  FoodsOrdered { Id = tab.Id; Foods = foods  } ; 
            DrinksOrdered { Id = tab.Id; Drinks = drinks }
      ])
    
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
        
        [TabOpened tab; DrinksOrdered { Id = tab.Id; Drinks = [drink] } ]
        =>  PlaceOrder order
        =! OrderAlreadyPlaced
      
  ]