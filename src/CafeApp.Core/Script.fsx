open System

#load "Result.fs"
#load "Domain.fs"
#load "Events.fs"
#load "Errors.fs"
#load "./CommandHandlers.fs"
open CafeApp.Result
open CafeApp.Domain
open CafeApp.Events
open CafeApp.Errors
open CafeApp.CommandHandlers

#r "../../packages/Expecto/lib/net40/Expecto.dll"
open Expecto

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
let coke = Drink { MenuNumber = 1; Price = 2.5m; Name = "Coke" }

let tea = Drink { MenuNumber = 3; Price = 1.2m; Name = "Tea" }
let sandwich = Food { MenuNumber = 30; Price = 5.5m; Name = "Sandwich" }


let order = { Tab = tab; Drinks = [ coke; lemonade ]; Foods = [salad; cookie; sandwich] }

let actual = 
    [   TabOpened tab; OrderPlaced order; DrinkServed (coke, tab.Id); DrinkServed (lemonade, tab.Id);
        FoodPrepared (salad, tab.Id); FoodPrepared (cookie, tab.Id)
    ]
    |> List.fold (evolve) (ClosedTab None) 
let expected = OrderInProgress { 
    PlacedOrder = { order with Drinks = []; Foods = [sandwich] } ; 
    ServedDrinks = [lemonade; coke]; 
    ServedFoods  = []
    PreparedFoods = [cookie; salad] } 

actual = expected