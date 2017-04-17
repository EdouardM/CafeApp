module Generators
open System
open Expecto
open FsCheck
open CafeApp.Result
open CafeApp.Errors
open CafeApp.Domain
open CafeApp.Events
open CafeApp.CommandHandlers

let genItem =
    Arb.generate<Item>
    |> Gen.filter(fun it -> 
        it.Price >= 0m 
        && it.MenuNumber > 0 
        && not <| String.IsNullOrEmpty(it.Name) )

let genDrink = 
    genItem
    |> Gen.map Drink 

let genFood = 
    genItem
    |> Gen.map Food 

let genDrinksNonEmpty = 
    genDrink
    |> Gen.nonEmptyListOf

let genFoodsNonEmpty = 
    genFood
    |> Gen.nonEmptyListOf

let genDrinksAndFoodsNonEmpty = Gen.zip genDrinksNonEmpty genFoodsNonEmpty
