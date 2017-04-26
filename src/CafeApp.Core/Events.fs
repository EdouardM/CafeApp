namespace CafeApp 

    module Events =
        open Domain
        open System
                
        type Event = 
            | TabOpened     of Tab
            | OrderPlaced   of Order 
            | FoodPrepared  of Food * Guid
            | FoodServed    of Food * Guid
            | DrinkServed   of Drink * Guid
            //Added payment to update Cashier
            | OrderServed   of Order * Payment
            | TabClosed     of Payment

        

          