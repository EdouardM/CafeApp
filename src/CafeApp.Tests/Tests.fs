module Tests
open System
open Expecto
open FsCheck
open CafeApp.Result
open CafeApp.Errors
open CafeApp.Domain
open CafeApp.Commands
open CafeApp.State
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


//Constant inputs
let tab = { Id = Guid.NewGuid() ; TableNumber = 1}
let lemonade =  Drink { MenuNumber = 2; Price = 1.5m; Name = "Lemonade" }
let salad = Food { MenuNumber = 4; Price = 0.5m; Name = "Salad" }
let cookie = Food { MenuNumber = 6; Price = 0.5m; Name = "Cookie" }

let sandwich = Food { MenuNumber = 30; Price = 5.5m; Name = "Sandwich" }
let coke = Drink { MenuNumber = 1; Price = 2.5m; Name = "Coke" }

let tea = Drink { MenuNumber = 3; Price = 1.2m; Name = "Tea" }

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
      let arb = genDrinksNonEmpty |> Arb.fromGen
      
      Prop.forAll arb ( fun drinks -> 
      let order = { Tab = tab; Drinks = drinks ; Foods = [] }
      [ TabOpened tab ]
      => PlaceOrder order
      == [ OrderPlaced order ] )

    testProperty "Can place only foods order if tab opened" <| fun _ -> 
      let arb = genFoodsNonEmpty |> Arb.fromGen

      Prop.forAll arb (fun foods -> 
      let order = { Tab = tab; Drinks = []  ; Foods = foods }
      [ TabOpened tab ]
      => PlaceOrder order
      == [ OrderPlaced order ] )
  
    testProperty "Can place any order if tab opened" <| fun _ -> 
      let arb = genOrder |> Arb.fromGen
      
      Prop.forAll arb (fun order -> 
      [ TabOpened tab ]
      => PlaceOrder order
      == [  OrderPlaced order] )
    
    testCase "Cannot place empty order" <| fun _ -> 
        let order = { Tab = tab; Drinks = []; Foods = [] }
        [ TabOpened tab ]
        =>  PlaceOrder order
        =! CannotPlaceEmptyOrder
    
    testCase "Cannot place order on closed table" <| fun _ -> 
        let order = { Tab = tab; Drinks = [tea]; Foods = [] }
        []
        =>  PlaceOrder order
        =! CannotOrderWithClosedTable
      
    testCase "Cannot place order multiple times" <| fun _ -> 
        let order = { Tab = tab; Drinks = [ coke ]; Foods = [] }
        
        [TabOpened tab; OrderPlaced order  ]
        =>  PlaceOrder order
        =! OrderAlreadyPlaced
  ]


[<Tests>]
let ServeDrinkTests = 
  testList "Serve Drink Transition" [
    testCase "Can serve one drink in one placed order" <| fun _ -> 
        let order = { Tab = tab; Drinks = [coke; lemonade]; Foods = [] }
        
        [ TabOpened tab; OrderPlaced order ]
        =>  ServeDrink (coke, tab.Id)
        == [ DrinkServed (coke, tab.Id) ]

    testCase "Cannot serve one not ordered drink in one placed order" <| fun _ -> 
        let order = { Tab = tab; Drinks = [ lemonade ]; Foods = [] }
        
        [ TabOpened tab; OrderPlaced order ]
        =>  ServeDrink (coke, tab.Id)
        =! CannotServeNonOrderedDrink coke

    testCase "Can serve one drink in one in progress order" <| fun _ -> 
        let order = { Tab = tab; Drinks = [ coke; lemonade; tea ]; Foods = [] }
        
        let actual = 
            [ TabOpened tab; OrderPlaced order; DrinkServed (tea, tab.Id); DrinkServed (lemonade, tab.Id) ]
            |> List.fold (evolve) (ClosedTab None) 
        let expected = OrderInProgress { 
            PlacedOrder     = order  
            OrderedDrinks   = [ coke ]
            OrderedFoods    = []
            ServedDrinks    = [ lemonade; tea ]
            ServedFoods     = []
            PreparedFoods   = [] } 

        Expect.equal actual expected "There should be one remaining drink to serve and 2 served."

    testCase "Can serve one drink for order with one drink" <| fun _ -> 
        let order = { Tab = tab; Drinks = [ coke ]; Foods = [] }
      
        [ TabOpened tab; OrderPlaced order]
        =>  ServeDrink (coke, tab.Id)
        == [ DrinkServed (coke, tab.Id); OrderServed order ]

    testCase "Cannot serve one drink more times than ordered" <| fun _ -> 
        let order = { Tab = tab; Drinks = [ coke; lemonade ]; Foods = [] }
        
        [ TabOpened tab; OrderPlaced order; DrinkServed (coke, tab.Id) ]
        =>  ServeDrink (coke, tab.Id)
        =! CanNotServeAlreadyServedDrink coke

    testCase "Cannot serve one not ordered drink during order in progress" <| fun _ -> 
        let order = { Tab = tab; Drinks = [ coke; lemonade ]; Foods = [] }
        
        [ TabOpened tab; OrderPlaced order; DrinkServed (coke, tab.Id) ]
        =>  ServeDrink (tea, tab.Id)
        =! CannotServeNonOrderedDrink tea


    testCase "Cannot serve one drink on a served order" <| fun _ -> 
        let order = { Tab = tab; Drinks = [ coke ]; Foods = [] }
        
        [ TabOpened tab; OrderPlaced order; DrinkServed (coke, tab.Id); OrderServed order ]
        =>  ServeDrink (coke, tab.Id)
        =! OrderAlreadyServed

    testCase "Cannot serve one drink for a non placed order" <| fun _ -> 
        let order = { Tab = tab; Drinks = [coke; lemonade]; Foods = [] }

        [ TabOpened tab ]
        =>  ServeDrink (coke, tab.Id)
        =! CannotServeNonPlacedOrder

    testCase "Cannot serve one drink to a closed tab" <| fun _ -> 
        [ ]
        =>  ServeDrink (coke, Guid.NewGuid())
        =! CannotServeClosedTab
]


[<Tests>]
let PrepareFoodTests = 
  testList "Prepare Food Transition" [
    testCase "Can prepare food in one placed order" <| fun _ -> 
        let order = { Tab = tab; Drinks = [lemonade]; Foods = [salad] }
        
        [ TabOpened tab; OrderPlaced order ]
        =>  PrepareFood (salad, tab.Id)
        == [ FoodPrepared (salad, tab.Id) ]

    testCase "Cannot prepare one not ordered food in one placed order" <| fun _ -> 
        let order = { Tab = tab; Drinks = [ lemonade ]; Foods = [ salad ] }

        [ TabOpened tab; OrderPlaced order ]
        =>  PrepareFood (cookie, tab.Id)
        =! CannotPrepareNonOrderedFood cookie

    testCase "Cannot prepare one food more times than ordered" <| fun _ -> 
        let order = { Tab = tab; Drinks = [ lemonade ]; Foods = [cookie; salad ] }
        
        [ TabOpened tab; OrderPlaced order; FoodPrepared (salad, tab.Id) ]
        =>  PrepareFood (salad, tab.Id)
        =! CanNotPrepareAlreadyPreparedFood salad

    testCase "Can serve one food in one in progress order" <| fun _ -> 
        let order = { Tab = tab; Drinks = [ coke; lemonade ]; Foods = [salad; cookie; sandwich] }
        
        let actual = 
            [   TabOpened tab; OrderPlaced order; DrinkServed (coke, tab.Id); DrinkServed (lemonade, tab.Id);
                FoodPrepared (salad, tab.Id); FoodPrepared (cookie, tab.Id)
            ]
            |> List.fold (evolve) (ClosedTab None) 
        let expected = OrderInProgress { 
            PlacedOrder     = order
            OrderedDrinks   = []
            OrderedFoods    = [ sandwich ]
            ServedDrinks    = [ lemonade; coke ]; 
            ServedFoods     = []
            PreparedFoods   = [ cookie; salad ] } 

        Expect.equal actual expected "There should be one remaining food to prepare and 2 prepared."

    testCase "Cannot prepare one food on a served order" <| fun _ -> 
        let order = { Tab = tab; Drinks = [ coke ]; Foods = [ cookie ] }

        [ TabOpened tab; OrderPlaced order; DrinkServed (coke, tab.Id); OrderServed order ]
        =>  PrepareFood (cookie, tab.Id)
        =! OrderAlreadyServed

    testCase "Cannot prepare one food for a non placed order" <| fun _ -> 
        let order = { Tab = tab; Drinks = [coke; lemonade]; Foods = [ salad ] }

        [ TabOpened tab ]
        =>  PrepareFood (salad, tab.Id)
        =! CannotPrepareFoodForNonPlacedOrder

    testCase "Cannot prepare one food to a closed tab" <| fun _ -> 
        [ ]
        =>  PrepareFood (cookie, Guid.NewGuid())
        =! CannotPrepareFoodForClosedTab
  ]

[<Tests>]
let ServeFoodTests = 
  testList "Serve Food Transition" [
    
    testCase "Cannot serve one not prepared food in one in progress order" <| fun _ -> 
        let order = { Tab = tab; Drinks = [ lemonade ]; Foods = [ salad ] }

        [ TabOpened tab; OrderPlaced order ]
        =>  PrepareFood (cookie, tab.Id)
        =! CannotPrepareNonOrderedFood cookie

    testCase "Cannot serve already served food" <| fun _ -> 
        let order = { Tab = tab; Drinks = [ lemonade ]; Foods = [cookie; salad ] }
        
        [ TabOpened tab; OrderPlaced order; FoodPrepared (salad, tab.Id); FoodServed (salad, tab.Id) ]
        =>  ServeFood (salad, tab.Id)
        =! CanNotServeAlreadyServedFood salad

    testCase "Can serve one prepared food in one in progress order" <| fun _ -> 
        let order = { Tab = tab; Drinks = [ coke; lemonade ]; Foods = [salad; cookie; sandwich] }
        
        let actual = 
            [   TabOpened tab; OrderPlaced order; DrinkServed (coke, tab.Id); DrinkServed (lemonade, tab.Id);
                FoodPrepared (salad, tab.Id); FoodPrepared (cookie, tab.Id); FoodServed (salad, tab.Id)
            ]
            |> List.fold (evolve) (ClosedTab None) 
        
        let expected = OrderInProgress { 
            PlacedOrder     = order
            OrderedFoods    = [ sandwich ]
            OrderedDrinks   = [] 
            ServedDrinks    = [ lemonade; coke ]; 
            ServedFoods     = [ salad ]
            PreparedFoods   = [ cookie; salad ] } 

        Expect.equal actual expected "There should be one remaining food to prepare and 2 prepared."

    testCase "Cannot prepare one food on a served order" <| fun _ -> 
        let order = { Tab = tab; Drinks = [ coke ]; Foods = [ cookie ] }

        [ TabOpened tab; OrderPlaced order; DrinkServed (coke, tab.Id); OrderServed order ]
        =>  ServeFood (cookie, tab.Id)
        =! OrderAlreadyServed

    testCase "Cannot serve one food for a non placed order" <| fun _ -> 
        let order = { Tab = tab; Drinks = [coke; lemonade]; Foods = [ salad ] }

        [ TabOpened tab ]
        =>  ServeFood (salad, tab.Id)
        =! CannotServeFoodForNonPlacedOrder

    testCase "Cannot prepare one food to a closed tab" <| fun _ -> 
        [ ]
        =>  ServeFood (cookie, Guid.NewGuid())
        =! CannotServeFoodForClosedTab
  ]