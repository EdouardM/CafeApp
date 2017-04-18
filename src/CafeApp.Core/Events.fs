namespace CafeApp 

    module Events =
        open Domain
        open System

        type Command = 
            | OpenTab       of Tab
            | PlaceOrder    of Order
            | ServeDrink    of Drink * Guid
        
        type Event = 
            | TabOpened of Tab
            | OrderPlaced of Order 
            | DrinkServed of Drink * Guid
            | OrderServed of Order

        type State = 
            | ClosedTab         of Tab option
            | OpenedTab         of Tab
            | PlacedOrder       of Order
            | OrderInProgress   of InProgressOrder
            | ServedOrder       of Order

        

          