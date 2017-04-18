namespace CafeApp 

    module CommandHandlers =
        open CafeApp.Result
        open CafeApp.Errors
        open CafeApp.Domain
        open CafeApp.Events


        let handleOpenTab (tab: Tab) = 
            function
                | ClosedTab _   -> Ok [ TabOpened { Id = tab.Id ; TableNumber = tab.TableNumber } ]
                | _   -> Failure TableAlreadyOpened

        let handlePlaceOrder (order: Order) = 
            function
            | ClosedTab _   -> Failure CannotOrderWithClosedTable

            | OpenedTab tab ->
                match order.Drinks, order.Foods with
                    | [], []        -> Failure CannotPlaceEmptyOrder
                    | _, _          -> Ok [ OrderPlaced order ]
            | PlacedOrder _         
            | OrderInProgress _ 
            | ServedOrder _         -> Failure OrderAlreadyPlaced
    
        let (|NonOrderedDrink|_|) order drink = 
            if not <| List.contains drink order.Drinks then
                Some drink
            else
                None
        let (|ServedDrinkCompletesOrder|_|) order drink = 
            match order.Drinks, order.Foods with
                |  drink::[], [] -> Some drink
                |  _, _          -> None

        let serveDrinkOnPlacedOrder order tableId drink = 
            if order.Tab.Id = tableId then
                    match drink with
                    
                    | NonOrderedDrink order drink -> 
                        Failure (CannotServeNonOrderedDrink drink)
                    //Order completed with served drink
                    | ServedDrinkCompletesOrder order drink -> 
                        Ok [ DrinkServed (drink, tableId); OrderServed order ]
                    
                    //Drink ordered, order not complete
                    | _ ->  Ok [ DrinkServed (drink, tableId)] 
                else
                        Failure (CannotServeNonOrderedDrink drink)


        let handleServeDrink drink tableId = 
            function
            | PlacedOrder order     -> serveDrinkOnPlacedOrder order tableId drink
            | OrderInProgress order -> serveDrinkOnPlacedOrder order.PlacedOrder tableId drink
            | ServedOrder order     -> Failure OrderAlreadyServed
            | ClosedTab _ -> Failure CannotServeClosedTab
            | OpenedTab _ -> Failure CannotServeNonPlacedOrder


        let decide command state =
            match command with
                | OpenTab tab -> handleOpenTab tab state
                | PlaceOrder order -> handlePlaceOrder order state
                | ServeDrink (drink, tableId) -> handleServeDrink drink tableId state

                
        let evolve state event = 
            match state, event with
                | ClosedTab _, TabOpened tab        -> OpenedTab tab
                | OpenedTab tab, OrderPlaced order  -> PlacedOrder order
                | PlacedOrder order, DrinkServed (drink, tabId) -> 
                    //Remove served drink from list of drinks
                    let drinks = order.Drinks |> List.except [ drink ]
                    {
                        PlacedOrder = { order with Drinks = drinks }
                        ServedDrinks = [ drink ]
                        ServedFoods = []
                        PreparedFoods = []
                    } |> OrderInProgress
                | OrderInProgress orderinprogress, OrderServed order -> ServedOrder order 
                | _ -> state
        
            