---
title: Communicating with the server
---

WebSharper supports remote procedure/function calls (RPCs) from the client (JavaScript) to the server (ASP.NET Core or other hosting environment).
Remoting is designed to be safe and efficient while requiring as little boilerplate as possible, usually a simple `Remote` \ `Rpc` attribute on the server to mark a method to be RPC callable, and an asynchronous workflow on the client to call it.

Here is an example of a client-side and a server-side function
pair that communicates via RPC:

```fsharp
module Server =
    
    [<Remote>]
    let GetArticlesByAuthor author =
        async { 
            use db = new DbContext()
            let! articles = db.GetArticlesByAuthorAsync author
            return blogs
        }
    
[<JavaScript>]
module Client =

    let GetArticlesByAuthor (author: Author) (callback: Blog [] -> unit) =
        async {
            let! articles = Server.GetArticlesByAuthor author
            return callback blogs
        }
        |> Async.Start
```

In the RPC model the client calls the server when necessary. To implement two-way communication, you can use [websockets](websockets).

WebSharper remoting requires that remote methods are marked with the `[<Remote>]` attribute (or its `[<Rpc>]` alias), and assumes that:
  
* They have argument types that are serializable to
  JSON.

* They have a return type that is serializable to
  JSON, or are of type `Async<'T>` or `Task<'T>` where `'T` is such a type.

See the [JSON serialization](json) page for supported types.

By default, remote methods are callable by any unauthenticated client, and you must add authentication into them if needed. See the [authentication](#authentication) section below for more details.

## Remote method types

The remoting mechanism supports three different ways of doing a remote
call: asynchronous, message-passing, and synchronous.

 * Asynchronous calls are the most common and the recommended way to do remote calls.
 * Message-passing calls are a special convenience class, used when the client does not care about the result of the call, and are implemented by remote methods that return `unit`.
 * Synchronous calls block the client execution until the server call returns - these calls are discouraged for production code as they can create a bad user experience.

### Asynchronous calls

As the most often-used RPC variant, these calls allow for asynchronous, callback-based processing of the server response. In the method implementation, they use `Async<'T>` or `Task<'T>` and they lend themselves easily to multi-step asynchronous workflows. The client-side implementation uses
nested JavaScript callbacks under the hood.

For example:

```fsharp
[<Remote>]
let Increment(x: int) =
    async {
        return! x + 1
    }

[<JavaScript>]
let Foo (callback: int -> unit) =
    async {
        let! x = Increment 0
        let! y = Increment x
        let! z = Increment y
        return callback z
    }
    |> Async.Start
```

With these definitions, a call to `Foo f` proceeds as follows:

* The client sends `0` to the server and registers a callback,
  proceeding immediately.

* The server replies with `1` and the browser invokes the callback
  from step 1, binding `x` to `1`.

* The client sends `1` to the server and registers another
  callback. These asynchronous steps repeat according to the workflow,
  until the line `return callback z` is reached, with `z` being bound
  to `3`.

* `f 3` is called.

### Message-passing calls

Message-passing calls do not block the browser, returning immediately
on the client. If an RPC function has the return type of `unit`, calls
to this function are message-passing calls.

```fsharp
[<Remote>]
let Log (msg: string) =
    System.Diagnostics.Debug.Write("MSG: {0}", msg)
```

With these definitions, a call to `Log "foo"` proceeds as follows:

* The client serializes `"foo"` to JSON.

* The client sends a request to the server.

* The client returns `unit` immediately.

* The server parses the request.

* The server binds to and calls the requested method with the
  arguments deserialized from JSON.

### Synchronous calls

Synchronous RPC calls block the browser (the UI thread) until the server reply is
available. Their use is not recommended. For the user they work just
like ordinary client-side function calls, except they call the server.

Example:

```fsharp
[<Remote>]
let Increase(x: int) = x + 1
```

With these definitions, a client call to `Increase 0` proceeds as
follows:

* The client serializes `0` to JSON.

* The client sends a request to the server. The request
  contains information on which method to call, and its
  JSON-serialized arguments (`0`)}

* The client blocks the browser.

* The server (in ASP.NET Core context, the WebSharper handler) parses the
  request and looks up the requested method.

* The server binds to the method, deserializes the arguments from
  JSON, and calls it.

* The server serializes the method's response to JSON and responds to
  the request.

* The client deserializes the response `1` from JSON and returns it.

* The client unblocks the browser.

<a name="authentication"></a>
## Authentication

Remote methods are exposed as HTTP endpoints, so any security measures have to be integrated into the method body itself.

`WebSharper.Web.Remoting.GetContext().UserSession` exposes various utilities for tracking users.
This uses `Microsoft.AspNetCore.Authentication` on the server and by default cookies in the browser. [See here](webcontext) for more information.

For instance, implementing user login might look like this:

```fsharp
//open WebSharper.Web
[<Remote>]
let Login (user: string, password: string) =
    let ctx = WebSharper.Web.Remoting.GetContext()
    async { 
        let! verified = VerifyLogin(user, password)
        if verified then
            do! ctx.UserSession.LoginUser user
            return true
        else
            return false
    }
```

Subsequently, you can protect other remote methods by checking if a user is logged in with a handy helper:
```fsharp
let WithAuthenticatedUser (f: string -> Async<'T>) =
    let ctx = WebSharper.Web.Remoting.GetContext()
    async {
        match! ctx.UserSession.GetLoggedInUser() with
        | Some user ->
            return! f user
        | None ->
            return failwith "User is not authenticated"
    }

[<Remote>]
let GetUserData () =
    WithAuthenticatedUser (fun username ->
        DB.FetchUserData username
    )
```

<a name="handler"></a>
## Instance-based remoting

You can also implement remote functions as instance methods.
To do so, in your server-side, define a type with methods that you want to expose to the client.
This type can be a class, a record, or an interface.
By naming this type and placing it in a shared project, you can reference the server API from the client without a direct dependency on the server implementation, giving you more flexibility to organize your application code. This also allows to cut down on compilation time if you only make client-side changes.
In either case, the remote methods should be annotated with the `Remote` / `Rpc` attribute and
follow the same convention as ordinary `Remote`/`Rpc` functions.

### Using a class

```fsharp
type MyApi() =
    [<Remote>]
    member this.MyMethod(...) = //...
```

You don't need to properly create an instance of the type on the client side, instead there is a helper to create a shim, like this:

```fsharp
//open WebSharper.JavaScript
Remote<MyApi>.MyMethod(...)
```

To have a handler for this type, you need to register an instance of it in your server side.

```fsharp
WebSharper.Core.Remoting.AddHandler typeof<MyApi> (new MyApi())
```

### Using a record type

Alternatively, you can use records to name the client-server API/RPC contract - either in your server-side code or in a project shared between your tiers:

```fsharp
type MyApi = {
    [<Remote>]
    GetAllTransactions : unit -> Async<DTO.Transaction array>
}
```

and implement it in your server-side:

```fsharp
let server : MyApi = {
    GetAllTransactions = fun () ->
        ...
    ...
}
```

You will need to register it to expose its endpoints:

```fsharp
WebSharper.Core.Remoting.AddHandler typeof<MyApi> server
```

Then in your client-side code, you can access it via the record type and pass it to any function that requires access to the server-side API:

```fsharp
[<JavaScript>]
module Grid =
    let ClientTransactionsGrid(server: MyApi) =
        async {
            let! transactions = server.GetAllTransactions()
            ...
        }

[<JavaScript>]
let TransactionsGrid() =
    let server = Remote<MyApi>
    Grid.ClientTransactionsGrid(server)
```

### Delaying implementation via abstract classes

Or, on configuring ASP.NET Core, instead of calling `AddHandler`, you can add the handler to the dependency injection graph using `builder.Services.AddWebSharperRemoting<THandler>()`. [See here for more details.](aspnetcore)

Remote annotated methods can be abstract.
The implementation that you provide can be of a subclass of the type in the first argument.
This way you can have parts of server-side RPC functionality undefined in a library, and have them implemented in your web project for better separation of concerns.

```fsharp
// Common server and client code:

[<AbstractClass>]
type MyApi() =
    [<Remote>]
    abstract MyMethod : ...

// Client consumer:

Remote<MyApi>.MyMethod(...)

// Server implementation:

type MyApiImpl() =
    inherits MyApi()
    
    override this.MyMethod(...) = ...

// App startup:

builder.Services.AddWebSharperRemoting<MyApi, MyApiImpl>() // using overload with two type arguments
```

## Client-side customization

The WebSharper compiler generates a JavaScript function for each remote method, which is used to call the server-side method.
In non-bundled mode, this function is inside the file created for the type, named `TypeName.js`.
This code handles the serialization of arguments, sending the request to the server, and deserializing the response.
It also creates an instance of a helper object to handle the actual call to the server. This object is `WebSharper.Remoting.AjaxRemotingProvider` by default but it is configurable with a `RemotingProvider` attribute on the remote method
with the custom remoting provider type as argument. This remoting type must implement the `WebSharper.Remoting.IRemotingProvider` interface.

The `IRemotingProvider` provider has 4 methods for implementing synchronous, asynchronous (Task and F# Async), and send calls: `Sync`, `Task`, `Async`, `Send` respectively.
You have to implement only the methods that your RPC methods will be using depending on their signatures.

The `RemotingProvider` attribute can be also used on classes and assemblies, overriding client-side remote handling for
all RPC methods found in their scope.

If you only want to change the URL that remoting calls target by default, you can set the value at the `WebSharper.Remoting.EndPoint` static property at the startup of your client-side code.

You can inherit from `AjaxRemotingProvider` to create a custom remoting provider that uses AJAX requests to communicate with the server but adds your extra functionality.

An example:

```fsharp
[<JavaScript>]
type SafeRemotingProvider() =
    inherit Remoting.AjaxRemotingProvider()

    override this.EndPoint = "https://myserver.com"

    override this.AsyncBase(handle, data) =
        let def = base.AsyncBase(handle, data) 
        async {
            try return! def
            with e ->
                Console.Log("Remoting exception", handle, e)
                return box None
        }
```

Then all `[Remote]` methods that you also annotate with `[RemotingProvider(typeof<SafeRemotingProvider>)]` will send the request to `myserver.com` and also catch any exceptions are happening during the call and return `None` instead (same as a `null`).
If all your such remote methods are returning `Option`s, this will be a valid value.

## Server-side customization

With standard RPCs, every remote method will create an HTTP endpoint with the URL `/TypeName/MethodName`.
You can customize the URL with a parameter on the `Remote` attribute, like `[<Remote("custom-url")>]`.

If you do not customize, make sure your type names are unique, so you can have multiple methods with the same name in different types, and do not use overloaded methods.

The server accepts any HTTP method, but the client will use `POST` for sending requests.
The request body will contain the JSON-serialized arguments of the method, and the response will be a JSON-serialized result of the method call.

CORS and CSRF protection are supported by default.
`WebSharper.Web.Remoting` module provides the following functions to configure CORS and CSRF:

* `SetAllowedOrigins` to set full list of allowed origins.
* `AddAllowedOrigin` to add a single allowed origin.
* `RemoveAllowedOrigin` to remove a single allowed origin.
* `DisableCsrfProtection` to disable CSRF protection (it is enabled by default).
* `EnableCsrfProtection` to re-enable CSRF protection if turned off first.
