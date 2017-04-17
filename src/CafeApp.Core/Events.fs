namespace CafeApp 

    module Events =
        open Domain
        open System

        type Command = 
            | OpenTab of Tab
            | PlaceOrder of Order

        type Event = 
            | TabOpened of Tab
            | OrderPlaced of Order 
            

        type State = 
            | ClosedTab     of Tab option
            | OpenedTab     of Tab
            | PlacedOrder   of Order

          