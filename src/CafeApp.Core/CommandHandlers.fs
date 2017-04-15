namespace CafeApp 

    module CommandHandlers =
        open CafeApp.Domain
        open CafeApp.Events

        let decide command state =
            match command with
                | OpenTab tab -> 
                    match state with
                        | ClosedTab _ -> [ TabOpened { Id = System.Guid.NewGuid(); TableNumber = 1 } ]
                        | OpenedTab _ -> failwith "unexpected command"


        let apply state event = 
            match state, event with
                | ClosedTab _, TabOpened evinfo -> 
                    OpenedTab { Id = evinfo.Id; TableNumber = evinfo.TableNumber }
                | OpenedTab _, TabOpened evinfo -> 
                    failwith "unexpected event"
        
        let evolve state command =
            let events = decide command state
            let newState = List.fold apply state events
            (newState, events)

        