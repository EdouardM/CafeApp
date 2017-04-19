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
            
            //Possible errors when Food Served
            | CannotPrepareNonOrderedFood of Food
            | CannotPrepareFoodForNonPlacedOrder
            | CannotPrepareFoodForClosedTab

            | CannotServeNonPreparedFood of Food
            

            //Possible errors when Food or Drink Served
            | OrderAlreadyServed
            | CannotServeNonPlacedOrder
            | CannotServeClosedTab