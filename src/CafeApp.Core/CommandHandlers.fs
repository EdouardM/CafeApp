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
            if List.contains drink order.Drinks then None else Some drink
        
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
            | ServedOrder _         -> Failure OrderAlreadyServed
            | ClosedTab _           -> Failure CannotServeClosedTab
            | OpenedTab _           -> Failure CannotServeNonPlacedOrder


        let (|NonOrderedFood|_|) order food = 
            if List.contains food order.Foods then None else Some food

        let prepareFoodOnPlacedOrder order tableId food =
            if order.Tab.Id = tableId then
                match food with
                | NonOrderedFood order food -> Failure (CannotPrepareNonOrderedFood food)
                | _                         -> Ok [ FoodPrepared (food, tableId) ]
            //Wrong Tab Id > Not Ordered food
            else Failure (CannotPrepareNonOrderedFood food)

        let handlePrepareFood food tableId = 
            function
            | PlacedOrder order     -> prepareFoodOnPlacedOrder order tableId food
            | OrderInProgress ipo   -> prepareFoodOnPlacedOrder ipo.PlacedOrder tableId food
            | ServedOrder _         -> Failure OrderAlreadyServed
            | ClosedTab _           -> Failure CannotPrepareFoodForClosedTab
            | OpenedTab _           -> Failure CannotPrepareFoodForNonPlacedOrder

        let decide command state =
            match command with
                | OpenTab tab -> handleOpenTab tab state
                | PlaceOrder order -> handlePlaceOrder order state
                | ServeDrink (drink, tableId) -> handleServeDrink drink tableId state
                | PrepareFood (food, tableId) -> handlePrepareFood food tableId state


        ///helper to remove first occurence from list
        let rec removeFirst pred lst =
            match lst with
            | h::t when pred h -> t
            | h::t -> h::removeFirst pred t
            | _ -> []
                
        let evolve state event = 
            match state, event with
                | ClosedTab _, TabOpened tab        -> OpenedTab tab
                | OpenedTab tab, OrderPlaced order  -> PlacedOrder order
                | PlacedOrder order, DrinkServed (drink, tabId) -> 
                    //Remove served drink from list of drinks
                    let drinks =  removeFirst (fun d -> d = drink) order.Drinks
                    {
                        PlacedOrder = { order with Drinks = drinks }
                        ServedDrinks = [ drink ]
                        ServedFoods = []
                        PreparedFoods = []
                    } |> OrderInProgress
                | OrderInProgress ipo, DrinkServed (drink, tabId) -> 
                    //Remove served drink from list of drinks
                    let drinks =  removeFirst (fun d -> d = drink) ipo.PlacedOrder.Drinks
                    {
                        ipo with 
                            PlacedOrder = { ipo.PlacedOrder with Drinks = drinks }
                            ServedDrinks = drink::ipo.ServedDrinks
                    } |> OrderInProgress

                | PlacedOrder order, FoodPrepared (food, tabId) -> 
                    //Remove served drink from list of drinks
                    let foods =  removeFirst (fun d -> d = food) order.Foods
                    {
                        PlacedOrder = { order with Foods = foods }
                        ServedDrinks = [ ]
                        ServedFoods = []
                        PreparedFoods = [ food ]
                    } |> OrderInProgress

                | OrderInProgress ipo, FoodPrepared (food, tabId) -> 
                    //Remove served drink from list of drinks
                    let foods =  removeFirst (fun d -> d = food) ipo.PlacedOrder.Foods
                    {
                        ipo with 
                            PlacedOrder = { ipo.PlacedOrder with Foods = foods }
                            PreparedFoods = food::ipo.PreparedFoods
                    } |> OrderInProgress

                | OrderInProgress ipo, OrderServed order -> ServedOrder order 
                | _ -> state
        
            