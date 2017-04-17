namespace CafeApp 

    module CommandHandlers =
        open CafeApp.Result
        open CafeApp.Errors
        open CafeApp.Domain
        open CafeApp.Events

        let decide command state =
            match command with
                | OpenTab tab -> 
                    match state with
                        | ClosedTab _ -> Ok [ TabOpened { Id = tab.Id ; TableNumber = tab.TableNumber } ]
                        | OpenedTab _ -> Failure TableAlreadyOpened

        let evolve state event = 
            match state, event with
                | ClosedTab _, TabOpened tab    -> OpenedTab tab
                | OpenedTab _, TabOpened evinfo -> 
                    failwith "unexpected event"
        
            