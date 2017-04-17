namespace CafeApp 
    
    module Errors = 
        
        type Error = 
            | TableAlreadyOpened
            | CannotOrderWithClosedTable
            | CannotPlaceEmptyOrder
            | OrderAlreadyPlaced