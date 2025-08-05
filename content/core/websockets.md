---
title: Websockets
---

Websockets are a browser feature that allows for full-duplex communication channels over a single TCP connection.
They are particularly useful for real-time applications where you need to send and receive messages continuously without the overhead of HTTP requests.

In WebSharper client side, it is possible to use a raw JavaScript WebSocket object `WebSharper.JavaScript.WebSocket` and set up event handlers to use websockets.
However, there is a nuget package `WebSharper.AspNetCore.WebSockets` that provides a more convenient, F#-style API for working with websockets in WebSharper applications.

## Setting up message types

To use websockets in WebSharper, you need to define the types of messages that will be sent and received.

You can use F# discriminated unions to define these message types. For example:
```fsharp
open WebSharper

[<JavaScript>]
type ClientToServer =
    | Request of string
    | Pong

[<JavaScript>]
type ServerToClient =
    | Response of string
    | Ping
```

These types will be serialized and deserialized automatically by the WebSharper framework when sending and receiving messages.
See the [json documentation](json) for restrictions on the types that can be used.

## Defining server agents

A server agent is a function that has a signature conforming to one of the following types:

```fsharp
type Agent<'S2C, 'C2S> = WebSocketClient<'S2C, 'C2S> -> Async<Message<'C2S> -> unit>

type StatefulAgent<'S2C, 'C2S, 'State> = WebSocketClient<'S2C, 'C2S> -> Async<'State * ('State -> Message<'C2S> -> Async<'State>)>

type CustomAgent<'S2C, 'C2S, 'Custom, 'State> = CustomWebSocketAgent<'S2C, 'C2S, 'Custom> -> Async<'State * ('State -> CustomMessage<'C2S, 'Custom> -> Async<'State>)>
```

The `Agent` type is the simplest form, where there is no state maintained between messages.
This means that each message can be processed independently.
The message handler function is expected to be returned wrapped in an `Async` computation, allowing for asynchronous operations before the websocket server goes live.

For example:

```fsharp
open WebSharper.AspNetCore.WebSocket

let serverAgent() : Agent<ServerToClient, ClientToServer> =
    fun client ->
        async {
            do! Async.Sleep 1000 // Simulate some processing for websocket startup
            return fun msg ->
                match msg with
                | Server.Message (Request name) ->
                    client.Post (Response $"Hello, {name}!")
                | Server.Message Pong ->
                    client.Post Ping
                | Server.Error ex ->
                    printfn "Error: %s" ex.Message
                | Server.Close ->
                    printfn "Connection closed"
        }
```

By setting up the return type on the `serverAgent()`, code service can help you to end up with the right signature.

If the message processing itself requires asynchronous operations, you can use an `Async.Start` on it, but be aware that then the message processing will not be awaited for the next message processing to possibly begin.
If the response contains some id that ties it back to the request, this can still be fine.

But if you need message processing to be sequential, you can use the `StatefulAgent` type, which also allows you to maintain state between messages.

For example:

```fsharp
let statefulServerAgent() : Server.StatefulAgent<ServerToClient, ClientToServer, int> =
    fun client ->
        async {
            do! Async.Sleep 1000 // Simulate some processing for websocket startup
            return (0, fun count msg ->
                async {
                    do! Async.Sleep 1000 // Simulate some processing for websocket response
                    match msg with
                    | Server.Message (Request name) ->
                        do! client.PostAsync (Response $"Hello, {name}! You are visitor number {count}.")
                        return count + 1
                    | Server.Message Pong ->
                        client.Post Ping
                        return count
                    | Server.Error ex ->
                        printfn "Error: %s" ex.Message
                        return count
                    | Server.Close ->
                        printfn "Connection closed"
                        return count
                })
        }
```

Here after initialization, a starting state of `0` is and the message processing function is returned.
The message processing function takes the current state and returns a new state after processing the message, wrapped in an `Async` which allows the response computation to be awaited.
In the background, stateful agent functions will be converted to an F# `MailboxProcessor`, ensuring that messages are processed in a single-threaded manner.

Also note that the `PostAsync` method is used to send a response back to the client, which ensures the whole message is sent before moving on to processing next request.

There is a third type of agent, `CustomAgent`, which provides an abstraction for the agent to reply to events happening on the server too.
A third message type `'Custom` is needed, for example:

```fsharp
[<JavaScript>]
type Custom = 
    | Dismiss of string

let customServerAgent() : Server.CustomAgent<ServerToClient, ClientToServer, Custom, int> =
    fun client ->
        async {
            do! Async.Sleep 1000 // Simulate some processing for websocket startup
            return (0, fun count msg ->
                async {
                    do! Async.Sleep 1000 // Simulate some processing for websocket response
                    match msg with
                    | Server.CustomMessage.Custom (Dismiss name) ->
                        client.Client.Post (Response $"Goodbye, {name}!")
                        return count - 1
                    | Server.CustomMessage.Message (Request name) ->
                        client.Client.Post (Response $"Hello, {name}! There are currently {count} visitors.")
                        async {
                            do! Async.Sleep 10000 // keep a visitor for 10 seconds
                            client.PostCustom (Dismiss name)
                        } |> Async.Start
                        return count + 1
                    | Server.CustomMessage.Message Pong ->
                        client.Client.Post Ping
                        return count
                    | Server.CustomMessage.Error ex ->
                        printfn "Error: %s" ex.Message
                        return count
                    | Server.CustomMessage.Close ->
                        printfn "Connection closed"
                        return count
                })
        }
```

Here, a goodbye message is sent to the client when a custom event is prompted by a server event, in this example 10 seconds after a `Request` message from the client.
Note that this allows weaving in the handling of events that are prompted by the server, for example on some longer running operations, you can send progress updates to the client instead of a single message, without tying up the message processor for other replies.

Some extra redirection is needed to use the `CustomAgent` type, as it uses a `CustomWebSocketAgent` type that has a `Client` property to access the client agent.
Also the message cases to process are of type `Server.CustomMessage`.

## The client object

On the `WebSocketClient` type (used from server-side code), there are several methods and properties available:
    
* `Connection`: an `WebSocketConnection` instance that provides access to some of the functionality of the underlying `System.Net.WebSockets.WebSocket` instance, and also exposes events
* `JsonProvider`: the caching JSON de/serializer instance used for messaging
* `Context`: the WebSharper context that can be used to access the current request url, user session, etc.
* `PostAsync`: post to client asynchronously
* `Post`: post to client and do not await full sending of the message
* `OnMessage`: an event that is triggered when a message is received from the client
* `OnOpen`: an event that is triggered when the websocket connection is opened
* `OnClose`: an event that is triggered when the websocket connection is closed

## Starting the server

Once you have defined your agent, you can start the websocket server with your WebSharper application in your startup code:

```fsharp
app
    .UseWebSockets()
    .UseWebSharper(fun ws ->
        ws
            .Sitelet(mySitelet)
            .UseWebSocket("ws", fun wsws -> wsws.Use(serverAgent()) |> ignore
        )
        |> ignore
    )
```

The part "ws" defines the path where the websocket server will be available.
Don't forget to add the `UseWebSockets()` middleware before using the WebSharper middleware.

## Defining client agents

A client agent is a function that has a signature conforming to one of the following types:

```fsharp
type Agent<'S2C, 'C2S> = WebSocketServer<'S2C, 'C2S> -> Async<Message<'S2C> -> unit>

type StatefulAgent<'S2C, 'C2S, 'State> = WebSocketServer<'S2C, 'C2S> -> Async<'State * ('State -> Message<'S2C> -> Async<'State>)>
```

We can see that it closely resembles the server agent, but the first parameter is a `WebSocketServer` instead of a `WebSocketClient`. There is no `CustomAgent` type for client agents currently.

However, from the client it is usually more convenient to start up the websocket connection without first defining an agent separately.
The functions `Client.Connect` and `Client.ConnectStateful` do this for agents and stateful agents respectively.

For example, to connect to a server agent defined above, you can do:

```fsharp
let startClientAgent endpoint =
    async {
        let! server =
            Client.Connect endpoint <| fun server -> async {
                return fun msg ->
                    match msg with
                    | Client.Message (Response text) ->
                        JavaScript.Console.Log("Server responded:", text)
                    | Client.Message Pong ->
                        server.Post Ping
                    | Client.Close ->
                        JavaScript.Console.Log("Connection closed")
                    | Client.Open ->
                        JavaScript.Console.Log("Connection opened")
                    | Client.Error ->
                        JavaScript.Console.Log("Connection error")
            }
        server.Post Ping // Start the ping-pong cycle
        // connect the server to your application logic to post Request messages
    }
    |> Async.Start
```

An example of a stateful client agent would be:

```fsharp
let statefulClientAgent() =
    async {
        let! server =
            Client.ConnectStateful endpoint <| fun server -> async {
                return (System.DateTime.Now,
                    fun timestamp msg ->
                        async {
                            match msg with
                            | Client.Message (Response text) ->
                                JavaScript.Console.Log("Server responded:", text)
                                return timestamp
                            | Client.Message Pong ->
                                server.Post Ping
                                let now = System.DateTime.Now
                                JavaScript.Console.Log("Server ping:", now - timestamp)
                                return now
                            | Client.Close ->
                                JavaScript.Console.Log("Connection closed")
                                return timestamp
                            | Client.Open ->
                                JavaScript.Console.Log("Connection opened")
                                return timestamp
                            | Client.Error ->
                                JavaScript.Console.Log("Connection error")
                                return timestamp
                        }
                )
            }
        server.Post Ping // Start the ping-pong cycle
        // connect the server to your application logic to post Request messages
    }
    |> Async.Start
```

## The server object

On the `WebSocketServer` type (used from client-side code), there only two members available:

* `Connection`: the underlying JavaScript `Websocket` instance
* `Post`: post a message to the server

## Setting up the client endpoint

We need endpoint to connect to, which a value of type `WebSocketEndpoint<ServerToClient, ClientToServer>`.
Easiest is to just construct it with a relative path:

```fsharp
let endpoint = WebSocketEndpoint<Server.S2CMessage, Server.C2SMessage>("/ws")
```

But it's also a serializable type, so it's possible to pass it from the server for maximum consistency.
For example:
```fsharp
// in the server Sitelet handler
let endpoint = WebSocketEndpoint.Create(ctx.RequestUri.ToString(), "/ws")
```
Then pass it to the client code for example as an argument like `client (Client.Main endpoint)`.
