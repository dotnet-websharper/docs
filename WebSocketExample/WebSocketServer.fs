namespace WebSocketExample

open Shared
open WebSharper
open WebSharper.AspNetCore.WebSocket
open WebSharper.AspNetCore.WebSocket.Server
module WebSocketServer =
    let Start() : StatefulAgent<ServerToClient, ClientToServer, int> =
        fun (client: WebSocketClient<ServerToClient,ClientToServer>) -> async {
            let clientIp = client.Connection.Context.Connection.RemoteIpAddress.ToString()
            return 0, fun state msg -> async {
                printfn $"Received ${state} from ${clientIp}"

                match msg with
                | Message data -> 
                    match data with
                    | ExampleRequest(a,b) -> 
                        do! client.PostAsync(ExampleResponse(a + b))
                    return state + 1
                | Error exn ->
                    Printf.eprintfn $""
                    do! client.PostAsync(ErrorResponse ($"Error: {exn.Message}"))
                    return state
                | Close ->
                    printfn $"Closed connection to {clientIp}"
                    return state
            }
        }