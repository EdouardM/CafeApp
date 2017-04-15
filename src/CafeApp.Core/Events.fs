namespace CafeApp 

    module Events =
        open Domain
        open System

        type Command = 
            | OpenTab of Tab

        type Event = 
            | TabOpened of Tab

        type State = 
            | ClosedTab of Tab option
            | OpenedTab of Tab

          