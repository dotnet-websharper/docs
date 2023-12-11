## How do I set up WebSocket in WebSharper?
First, we'll have to install the `WebSharper.AspNetCore.WebSocket` nuget package, then in a Client-Server example:
* Set up a shared "package", be it a shared project or just a `Shared.fs` file, such as:
```fsharp
open WebSharper

[<JavaScript>]
module Shared =

    [<NamedUnionCases>]
    type ClientToServer =
    | ExampleRequest of a:int*b:int

    [<NamedUnionCases "type">]
    type ServerToClient =
    | [<Name "int">] ExampleResponse of int
    | [<Name "string">] ErrorResponse of string
```
### Server-side
Here, we'll need a `StatefulAgent<ServerToClient, ClientToServer, 'State>` value, which will be defined by:
```fsharp
open WebSharper.AspNetCore.WebSocket.Server
open Shared

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
```

Then, we'll also have to set up the `WebSocket` service in our ASP.NET application, which can be done by adding the following line to our `IApplicationBuilder` chain:

```fsharp
[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    
    // Add services to the container.
    builder.Services.AddWebSharper()

    let app = builder.Build()

    app.UseWebSharper(fun ws ->
        ws.UseWebSocket("ws", fun wsocket ->
            wsocket.Use(WebSocketServer.Start()) |> ignore
        )
    )
```

In case of a Sitelet-based project, to be on the safe side we'll have to construct our `WebSocketEndpoint` on the server, and pass it to the sitelet on request, such as:
```fsharp
open WebSharper
open WebSharper.UI.Html
open WebSharper.AspNetCore.WebSocket

open type WebSharper.UI.ClientServer

// the WebSharper.UI.* opens are only there for copy-pastability
let HomePage (ctx:Context<_>) =
    let wsEndpoint = WebSocketEndpoint.Create(ctx.RequestUri.ToString(), "ws")
    Templating.Main ctx EndPoint.Home "Home" [
        h1 [] [text "Say Hi to the server!"]
        div [] [client (Client.Main wsEndpoint)]
    ]
```

# WIP: bughunt