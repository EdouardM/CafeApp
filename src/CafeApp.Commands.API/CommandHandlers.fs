module CommandHandlers 

open EventStore
open Queries
open CafeApp.Result
open CafeApp.Commands
open CafeApp.Errors
open CafeApp.CommandHandlers

(*
Handling an incoming command involves the following responsibilities. 
1. Define a JSON payload for the command 
2. Parse the JSON payload to a data type 
3. Validate the data and transform it to a domain type 
4. Create the  Command  type 
5. Get the  State  from the  EventStore 
6. Call the  evolve  function 
7. Return the result or the error
As these steps are standard for all the commands, we can define a generic command handler. 
*)

type Commander<'a, 'b> = {
    Validate    : 'a -> Async<Choice<'b, string>>
    ToCommand   : 'b -> Command
}

type ErrorResponse = {
    Message : string
}

let err msg = { Message = msg }

let getTabIdFromCommand = function
    | OpenTab tab               -> tab.Id
    | PlaceOrder order          -> order.Tab.Id
    | ServeDrink (_ , tabId)    -> tabId 
    | PrepareFood (_, tabId)    -> tabId
    | ServeFood (_, tabId)      -> tabId
    | CloseTab  payment         -> payment.Tab.Id  

let handleCommand eventStore commandData commander = 
    async {
        let! validationRes = commander.Validate commandData
        match validationRes with
        | Choice1Of2 validCommandData ->
            let command = commander.ToCommand validCommandData
            let tabId = getTabIdFromCommand command
            let! state = eventStore.GetState tabId
            match decide command state with
            | Ok events     -> 
                let newState = List.fold evolve state events
                return Ok (newState, events)
            | Failure msg  -> 
                return (toErrorString >> err >> Failure) msg
        | Choice2Of2 errorMessage ->
            return (failwith errorMessage)
    }
