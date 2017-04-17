module Generators
open System
open Expecto
open FsCheck

open CafeApp.Result
open CafeApp.Errors
open CafeApp.Domain
open CafeApp.Events
open CafeApp.CommandHandlers


let genTab = 
    Arb.generate<Tab>
    |> Gen.filter(fun tab -> 
        tab.TableNumber > 0) 

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

let genOrder = 
    gen {
        let! tab = genTab
        let! drinks, foods = genDrinksAndFoodsNonEmpty
        let order = {Tab = tab; Drinks = drinks; Foods = foods }
        return order
    }
