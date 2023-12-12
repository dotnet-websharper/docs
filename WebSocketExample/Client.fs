namespace WebSocketExample

open WebSharper
open WebSharper.JavaScript



[<JavaScript>]
module Templates =
    open WebSharper.UI.Templating

    type MainTemplate = Template<"Main.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>

[<JavaScript>]
module Client =

    open WebSharper.UI
    open WebSharper.UI.Notation
    open WebSharper.AspNetCore.WebSocket
    open WebSharper.AspNetCore.WebSocket.Client

    let getWebSocketInstance (endpoint: WebSocketEndpoint<Shared.ServerToClient, Shared.ClientToServer>) =
        async {
            return! ConnectStateful endpoint <| fun server -> async {
                    return 0, fun state msg -> async {
                        match msg with
                        | Message data ->
                            match data with
                            | Shared.ExampleResponse value -> Console.Log value
                            | Shared.ErrorResponse errStr -> Console.Error errStr
                            return state + 1
                        | Close -> 
                            Console.Log "Connection closed"
                            return state
                        | Open -> 
                            Console.Log "Connection opened"
                            return state
                        | Error -> 
                            let errMsg = "Connection error"
                            Console.Error errMsg
                            failwith errMsg
                            return state
                    }
            }
        }
    let tryWebSocketInstance endpoint =
        async {
            let! server = getWebSocketInstance endpoint

            Shared.ExampleRequest(3,2)
            |> server.Post // this should result in a Console.Log with a value of "5" 
        }
    let Main wsEndpoint =
        tryWebSocketInstance wsEndpoint
        |> Promise.OfAsync
        |> ignore

        let rvReversed = Var.Create ""
        Templates.MainTemplate.MainForm()
            .OnSend(fun e ->
                async {
                    let! res = Server.DoSomething e.Vars.TextToReverse.Value
                    rvReversed := res
                }
                |> Async.StartImmediate
            )
            .Reversed(rvReversed.View)
            .Doc()
