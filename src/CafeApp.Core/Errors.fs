namespace CafeApp 
    
    module Errors = 
        open CafeApp.Domain        
        
        type Error = 
            | TableAlreadyOpened
            | CannotOrderWithClosedTable
            | CannotPlaceEmptyOrder
            | OrderAlreadyPlaced
            | CannotServeNonOrderedDrink of Drink
            | OrderAlreadyServed
            | CannotServeNonPlacedOrder
            | CannotServeClosedTab