namespace CafeApp

module State = 
    open Domain
    open System
    
    type State = 
        | ClosedTab         of Guid option
        | OpenedTab         of Tab
        | PlacedOrder       of Order
        | OrderInProgress   of InProgressOrder
        | ServedOrder       of Order

        