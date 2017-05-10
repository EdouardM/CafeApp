namespace CafeApp 

    module CommandHandlers =
        open CafeApp.Result
        open CafeApp.Errors
        open CafeApp.Domain
        open CafeApp.Commands
        open CafeApp.State
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
            
        let (|ServedDrinkCompletesPlacedOrder|_|) order drink = 
                match order.Drinks, order.Foods with
                |  d::[], [] when d = drink -> Some drink
                |  _, _                     -> None
            
            
        let (|AlreadyServedDrink|_|) ipo drink = 
            match List.contains drink ipo.ServedDrinks, List.contains drink ipo.OrderedDrinks with
            | true, false -> Some drink
            | _           -> None

        let serveDrinkOnPlacedOrder order tableId drink = 
            if order.Tab.Id = tableId then
                    match drink with
                    | NonOrderedDrink order drink -> 
                        Failure (CannotServeNonOrderedDrink drink)
                    //Order completed with served drink
                    | ServedDrinkCompletesPlacedOrder order drink -> 
                        Ok [ DrinkServed (drink, tableId); OrderServed (order, payment order) ]
                    
                    //Drink ordered, order not complete
                    | _ ->  Ok [ DrinkServed (drink, tableId)] 
            else
                        Failure (CannotServeNonOrderedDrink drink)

        let (|ServedDrinkCompletesInProgressOrder|_|) ipo drink = 
            //All drinks are served, all food being prepared or served
            match ipo.OrderedDrinks , ipo.OrderedFoods with
            | d::[], [] when d  = drink -> 
                //If All drinks are served then OK
                let served = List.sortBy(fun (Drink f) -> f.Name) (d::ipo.ServedDrinks)
                let ordered = List.sortBy(fun (Drink f) -> f.Name) ipo.PlacedOrder.Drinks
                if  ordered = served then  Some drink else None
            | _ -> None

        let serveDrinkOnInProgressOrder ipo tableId drink = 
            match drink with 
            | AlreadyServedDrink ipo _ -> Failure (CanNotServeAlreadyServedDrink drink)
            | ServedDrinkCompletesInProgressOrder ipo drink -> 
                Ok [ DrinkServed (drink, tableId) ; OrderServed (ipo.PlacedOrder, payment ipo.PlacedOrder) ]
            | _ -> serveDrinkOnPlacedOrder ipo.PlacedOrder tableId drink

        let handleServeDrink drink tableId = 
            function
            | PlacedOrder order     -> serveDrinkOnPlacedOrder order tableId drink
            | OrderInProgress ipo   -> serveDrinkOnInProgressOrder ipo tableId drink
            | ServedOrder _         -> Failure OrderAlreadyServed
            | ClosedTab _           -> Failure CannotServeClosedTab
            | OpenedTab _           -> Failure CannotServeNonPlacedOrder

        let (|NonOrderedFood|_|) order food = 
            if List.contains food order.Foods then None else Some food

        let (|AlreadyPreparedFood|_|) ipo food = 
            match List.contains food ipo.PreparedFoods, List.contains food ipo.OrderedFoods with
            | true, false -> Some food
            | _           -> None

        let prepareFoodOnPlacedOrder order tableId food =
            if order.Tab.Id = tableId then
                match food with
                | NonOrderedFood order food -> Failure (CannotPrepareNonOrderedFood food)
                | _                         -> Ok [ FoodPrepared (food, tableId) ]
            //Wrong Tab Id > Not Ordered food
            else Failure (CannotPrepareNonOrderedFood food)

        let prepareFoodOnInProgressOrder ipo tableId food =
            match food with
            | AlreadyPreparedFood ipo _ -> Failure (CanNotPrepareAlreadyPreparedFood food)
            | _ -> prepareFoodOnPlacedOrder ipo.PlacedOrder tableId food

        let handlePrepareFood food tableId = 
            function
            | PlacedOrder order     -> prepareFoodOnPlacedOrder order tableId food
            | OrderInProgress ipo   -> prepareFoodOnInProgressOrder ipo tableId food
            | ServedOrder _         -> Failure OrderAlreadyServed
            | ClosedTab _           -> Failure CannotPrepareFoodForClosedTab
            | OpenedTab _           -> Failure CannotPrepareFoodForNonPlacedOrder


        let (|NotPreparedFood|_|) ipo food = 
                match List.contains food    ipo.PreparedFoods, List.contains food ipo.PlacedOrder.Foods with
                | false, true  -> Some food
                | _             -> None 
            
        let (|AlreadyServedFood|_|) ipo food = 
            match List.contains food ipo.ServedFoods, List.contains food ipo.OrderedFoods with
            | true, false -> Some food
            | _           -> None

        let (|ServedFoodCompletesOrder|_|) (ipo: InProgressOrder) food = 
            match ipo.OrderedDrinks, ipo.OrderedFoods with
                //All drinks are served, all food being prepared or served
                |  [], [] -> 
                    //If All food ordered is served then OK
                    let served = List.sortBy(fun (Food f) -> f.Name) (food::ipo.ServedFoods)
                    let placed = List.sortBy(fun (Food f) -> f.Name) ipo.PlacedOrder.Foods
                    if  placed = served then
                        Some food
                    else None
                |  _, _          -> None

        let serveFoodOnPlacedOrder order tableId food =
            if order.Tab.Id = tableId then
                match food with
                | NonOrderedFood order food -> Failure (CannotServeNonOrderedFood food)
                
                //Cannot go from Placed to Served order => foods must be prepared
                | _                         -> Failure (CannotServeNonPreparedFood food)
            //Wrong Tab Id > Not Ordered food
            else Failure (CannotServeNonOrderedFood food)

        let serveFoodOnInProgressOrder ipo tableId food =
            match food with
            | AlreadyServedFood ipo _           -> Failure (CanNotServeAlreadyServedFood food)
            | NotPreparedFood ipo food          -> Failure (CannotServeNonPreparedFood food)
            | ServedFoodCompletesOrder ipo food -> 
                Ok [ FoodServed (food, tableId); OrderServed (ipo.PlacedOrder, payment ipo.PlacedOrder) ]
            | _                                 -> serveFoodOnPlacedOrder ipo.PlacedOrder tableId food

        let handleServeFood food tableId = 
            function
            | PlacedOrder order -> serveFoodOnPlacedOrder order tableId food
            | OrderInProgress ipo   -> serveFoodOnInProgressOrder ipo tableId food
            | ServedOrder _         -> Failure OrderAlreadyServed
            | ClosedTab _           -> Failure CannotServeClosedTab
            | OpenedTab _           -> Failure CannotServeNonPlacedOrder

        let handleCloseTab payment = 
            function
            | ServedOrder order -> 
                let orderAmount = 
                    List.sumBy(fun (Drink d) -> d.Price) order.Drinks
                    + List.sumBy(fun (Food d) -> d.Price) order.Foods
                if payment.Amount = orderAmount then
                    Ok [ TabClosed payment ]
                else 
                    Failure <| InvalidPayment (payment.Amount, orderAmount)
            | _ -> Failure CannotPayNonServedOrder

        let decide command state =
            match command with
            | OpenTab tab                   -> handleOpenTab tab state
            | PlaceOrder order              -> handlePlaceOrder order state
            | ServeDrink (drink, tableId)   -> handleServeDrink drink tableId state
            | PrepareFood (food, tableId)   -> handlePrepareFood food tableId state
            | ServeFood (food, tableId)     -> handleServeFood food tableId state
            | CloseTab payment              -> handleCloseTab payment state


        let evolve state event = 
            match state, event with
            | ClosedTab _, TabOpened tab        -> OpenedTab tab
            | OpenedTab tab, OrderPlaced order  -> PlacedOrder order
            | PlacedOrder order, DrinkServed (drink, tabId) -> 
                //Remove served drink from list of drinks
                let drinks =  List.removeFirst (fun d -> d = drink) order.Drinks
                {
                    //original order placed
                    PlacedOrder     = order
                    //Copy remaining drinks as still ordered
                    OrderedDrinks   = drinks
                    //All food is still to be prepared / served
                    OrderedFoods    = order.Foods
                    ServedDrinks    = [ drink ]
                    ServedFoods     = []
                    PreparedFoods   = []
                } |> OrderInProgress
            | OrderInProgress ipo, DrinkServed (drink, tabId) -> 
                //Remove served drink from list of drinks
                let drinks =  List.removeFirst (fun d -> d = drink) ipo.OrderedDrinks
                {
                    ipo with 
                        OrderedDrinks = drinks
                        ServedDrinks = drink::ipo.ServedDrinks
                } |> OrderInProgress

            | PlacedOrder order, FoodPrepared (food, tabId) -> 
                //Remove served drink from list of drinks
                let foods =  List.removeFirst (fun d -> d = food) order.Foods
                {
                    PlacedOrder     = order
                    //All food is still to be served
                    OrderedDrinks   = order.Drinks
                    //Copy remaining foods as still ordered
                    OrderedFoods    = foods
                    ServedDrinks    = []
                    ServedFoods     = []
                    PreparedFoods   = [food]
                } |> OrderInProgress

            | OrderInProgress ipo, FoodPrepared (food, tabId) -> 
                //Remove served food from list of ordered Foods
                let foods =  List.removeFirst (fun d -> d = food) ipo.OrderedFoods
                {
                    ipo with 
                        OrderedFoods = foods
                        PreparedFoods = food::ipo.PreparedFoods
                } |> OrderInProgress

            | OrderInProgress ipo, FoodServed (food, tabId) -> 

                { ipo with ServedFoods = food::ipo.ServedFoods } 
                |> OrderInProgress

            | OrderInProgress ipo, OrderServed (order, _ ) -> ServedOrder order 
            | ServedOrder order, TabClosed payment -> ClosedTab (Some payment.Tab.Id)
            | _ -> state
        
            