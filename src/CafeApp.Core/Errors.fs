namespace CafeApp 
    
    module Errors = 
        open CafeApp.Domain        

        type Error = 
            | TableAlreadyOpened
            | CannotOrderWithClosedTable
            | CannotPlaceEmptyOrder
            | OrderAlreadyPlaced
            //Possible errors when Drink Served
            | CannotServeNonOrderedDrink of Drink
            | CanNotServeAlreadyServedDrink of Drink
            
            //Possible errors when Food Prepared
            | CannotPrepareNonOrderedFood of Food
            | CannotPrepareFoodForNonPlacedOrder
            | CannotPrepareFoodForClosedTab
            | CanNotPrepareAlreadyPreparedFood of Food

            //Possible errors when Food Prepared
            | CannotServeNonOrderedFood of Food
            | CannotServeNonPreparedFood of Food
            | CanNotServeAlreadyServedFood of Food

            //Possible errors when Food or Drink Served
            | OrderAlreadyServed
            | CannotServeNonPlacedOrder
            | CannotServeClosedTab

            //Possible errors when closing table
            | InvalidPayment of decimal * decimal
            | CannotPayNonServedOrder

        let toErrorString = 
            function
            | TableAlreadyOpened -> "Tab Already Opened"
            | CannotOrderWithClosedTable -> "Cannot Order as Tab is Closed"
            | OrderAlreadyPlaced -> "Order already placed"
            | CannotServeNonOrderedDrink (Drink item)  ->
                sprintf "Drink %s(%d) is not ordered" item.Name item.MenuNumber
            | CanNotServeAlreadyServedDrink (Drink item)  ->
                sprintf "Drink %s(%d) is already served" item.Name item.MenuNumber
            | CannotServeNonOrderedFood (Food item) ->
                sprintf "Food %s(%d) is not ordered" item.Name item.MenuNumber
            | CannotPrepareNonOrderedFood (Food item) ->
                sprintf "Food %s(%d) is not ordered" item.Name item.MenuNumber
            | CannotServeNonPreparedFood (Food item) ->
                sprintf "Food %s(%d) is not prepared yet" item.Name item.MenuNumber
            | CanNotPrepareAlreadyPreparedFood (Food item) ->
                sprintf "Food %s(%d) is already prepared" item.Name item.MenuNumber
            | CanNotServeAlreadyServedFood (Food item) ->
                sprintf "Food %s(%d) is already served" item.Name item.MenuNumber
            | CannotServeClosedTab -> "Cannot Serve as Tab is Closed"
            | CannotPrepareFoodForClosedTab -> "Cannot Prepare as Tab is Closed"
            | OrderAlreadyServed -> "Order Already Served"
            | InvalidPayment (expected, actual) ->
                sprintf "Invalid Payment. Expected is %f but paid %f" expected actual
            | CannotPayNonServedOrder -> "Can not pay for non served order"
            | CannotPlaceEmptyOrder -> "Can not place empty order"
            | CannotPrepareFoodForNonPlacedOrder ->
                "Can not prepare for non placed order"
            | CannotServeNonPlacedOrder -> "Can not serve for non placed order"
