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

            | PlacedOrder _ -> Failure OrderAlreadyPlaced
    


        let decide command state =
            match command with
                | OpenTab tab -> handleOpenTab tab state
                | PlaceOrder order -> handlePlaceOrder order state
                
        let evolve state event = 
            match state, event with
                | ClosedTab _, TabOpened tab        -> OpenedTab tab
                | OpenedTab tab, OrderPlaced order  -> PlacedOrder order
                | _ -> state
        
            