namespace CafeApp 

[<AutoOpen>]
module Result = 

    type Result<'TSuccess, 'TFailure> = 
        | Ok of 'TSuccess
        | Failure of 'TFailure