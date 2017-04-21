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
            | CannotServeNonPreparedFood of Food
            | CannotServeFoodForNonPlacedOrder
            | CannotServeFoodForClosedTab
            | CanNotServeAlreadyServedFood of Food


            //Possible errors when Food or Drink Served
            | OrderAlreadyServed
            | CannotServeNonPlacedOrder
            | CannotServeClosedTab