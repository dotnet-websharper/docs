## How do I set up WebSocket in WebSharper?
First, we'll have to install the `WebSharper.AspNetCore.WebSocket` nuget package, then in a Client-Server example:
* Set up a shared model, be it a shared project or just a `Shared.fs` file, such as:
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

### Client-side
To instantiate your WebSocket connection, your code will be similar to the server-side `Start` method:
```fsharp
open Shared


open WebSharper.AspNetCore.WebSocket
open WebSharper.AspNetCore.WebSocket.Client

let getWebSocketInstance (endpoint: WebSocketEndpoint<ServerToClient, ClientToServer>) =
    async {
        // here, the WebSocketServer has the same generic params as the WebSocketEndpoint
        return! ConnectStateful endpoint <| fun (server:WebSocketServer<_,_>) -> async {
            return 0, fun state msg -> async {
                match msg with
                | Message data ->
                    match data with
                    | ExampleResponse value -> 
                        Console.Log value
                    | ErrorResponse errStr -> 
                        Console.Error errStr
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

// a function to test it out
let tryWebSocketInstance endpoint =
    async {
        let! server = getWebSocketInstance endpoint

        ExampleRequest(3,2)
        |> server.Post // this should result in a Console.Log with a value of "5" 
    }
```

Now that we have our methods to work with a `WebSocket` connection, we'll just have to plug it into our `Client.Main` method (or wherever you'd use it):

```fsharp
open WebSharper.UI
open  WebSharper.UI.Notation

let Main wsEndpoint =
    tryWebSocketInstance wsEndpoint
    |> Promise.OfAsync
    |> ignore
    
    // ...the rest of our function goes here

    Templates.MainTemplate.MainForm() // example-only
        // .HoleOne(holeContent)
        .Doc()
```
# Needs: proofread/bughunt