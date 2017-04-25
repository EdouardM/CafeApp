namespace CafeApp

module State = 
    open Domain
    
    type State = 
        | ClosedTab         of Tab option
        | OpenedTab         of Tab
        | PlacedOrder       of Order
        | OrderInProgress   of InProgressOrder
        | ServedOrder       of Order

        