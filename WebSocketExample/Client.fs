namespace WebSocketExample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Templating
open WebSharper.UI.Notation



[<JavaScript>]
module Templates =

    type MainTemplate = Templating.Template<"Main.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>

[<JavaScript>]
module Client =

    open WebSharper.AspNetCore.WebSocket
    open WebSharper.AspNetCore.WebSocket.Client

    let webSocketExample (endpoint: WebSocketEndpoint<Shared.ServerToClient, Shared.ClientToServer>) =
        async {
            let! server = 
                ConnectStateful endpoint <| fun server -> async {
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
                            Console.Error "Connection error"
                            return state
                    }
                }

            let arr = Array.init 10 id
            let pairs = 
                Array.allPairs arr arr
                |> Array.map Shared.ExampleRequest
            for pair in pairs do
                do! Async.Sleep 1000
                server.Post pair
                
        }
    let Main wsEndpoint =
        webSocketExample(wsEndpoint)
        |> Async.StartImmediate
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
