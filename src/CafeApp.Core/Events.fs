namespace CafeApp 

    module Events =
        open Domain
        open System

        type Command = 
            | OpenTab of Tab
            | PlaceOrder of Order

        type Event = 
            | TabOpened of Tab
            | DrinksOrdered of DrinksOrdered 
            | FoodsOrdered of FoodsOrdered

        and DrinksOrdered = {
            Id : Guid
            Drinks : Drink list
        }

        and FoodsOrdered = {
            Id : Guid
            Foods : Food list
        }

        type State = 
            | ClosedTab     of Tab option
            | OpenedTab     of Tab
            | PlacedOrder   of Order

          