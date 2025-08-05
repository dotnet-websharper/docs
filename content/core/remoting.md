---
title: Communicating with the server
---

WebSharper supports remote procedure calls from the client (JavaScript environment) to the server (ASP.NET Core or other hosting environment).
Remoting is designed to be safe and efficient while requiring as little boilerplate code as possible, just an attribute and a simple asynchronous method call.

Here is an example of a client-side and a server-side function
pair that communicate with RPC:

```fsharp
module Server =
    
    [<Remote>]
    let GetBlogsByAuthor author =
        async { 
            use db = new DbContext()
            let! blogs = db.GetBlogsByAuthorAsync author
            return blogs 
        }
    
[<JavaScript>]
module Client =

    let GetBlogsByAuthor (author: Author) (callback: Blog [] -> unit) =
        async {
            let! blogs = Server.GetBlogsByAuthor author
            return callback blogs
        }
        |> Async.Start
```

The conceptual model assumed by WebSharper is this: the client always
has the control, and calls the server when necessary.

The remoting component also assumes that:

* RPC-callable methods are marked with `[<Remote>]` (or `[<Rpc>]` alias).

* RPC-callable methods are safe to call from the web by an
  unauthenticated client. You can introduce helper functions to do the authentication for protected endpoints.
  
* RPC-callable methods have argument types that are serializable to
  JSON.

* RPC-callable methods have a return type that is serializable to
  JSON, or are of type `Async<'T>` or `Task<'T>` where `'T` is such a type.

See WebSharper [JSON serialization documentation](json) for details on what types are supported.

## The RPC protocol and customization

The goal of WebSharper remoting is to make RPC calls as simple as possible, yet, it's good to know how it works under the hood to customize it when needed.

By default, every remote method will create an HTTP endpoint with the URL `/TypeName/MethodName`.
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

# Remote Call Types

The remoting mechanism supports three different ways of doing a remote
call: asynchronous, message-passing, and synchronous.
Asynchronous calls are the most common, and the recommended way to do remote calls.
Message-passing calls are a convenience feature, when the client does not care about the result of the call.
Synchronous (blocking) calls are discouraged for production-ready code as they can create bad user experience on a possibly slow response.

## Asynchronous Calls

These calls allow for asynchronous, callback-based processing of the
server response.  They utilize `Async<'T>` or `Task<'T>` abstraction to
express multi-step asynchronous workflows.  The implementation uses
nested JavaScript callbacks.

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

The mechanics of individual calls are similar to the message-passing
calls.

## Message-Passing Calls

Message-passing calls do not lock the browser, returning immediately
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

## Synchronous Calls

Synchronous RPC calls block the browser until the server's reply is
available. Their use is not recommended. For the user they look just
like ordinary client-side function calls.

Example:

```fsharp
[<Remote>]
let Increase(x: int) = x + 1
```

With these definitions, a client call to `Increase 0` proceeds as
follows:

* The client serializes `0` to JSON.

* The client sends a RESTful request to the server. The request
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

### Security

Remote methods are exposed as http endpoints, so any security measures have to be integrated into the method body itself.
`WebSharper.Web.Remoting.GetContext().UserSession` exposes some utilities for tracking users.
This uses `Microsoft.AspNetCore.Authentication` on the server and by default cookies in the browser. [See here](webcontext) for more information.

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
        else return false
    }
```

Then, you can protect other remote methods by checking if the user is logged in with a handy helper:
```fsharp
let WithAuthenticatedUser (f: unit -> Async<'T>) =
    async {
        let ctx = WebSharper.Web.Remoting.GetContext()
        if ctx.UserSession.IsAuthenticated then
            return! f()
        else
            return failwith "User is not authenticated"
    }

[<Remote>]
let GetUserData () =
    WithAuthenticatedUser (fun () ->
        FetchUserData ctx.UserSession.UserName
    )
```

<a name="handler"></a>
### Instance-based remoting

You can also use instance methods for client-server communication.
In the server-side code, you define a type with methods that you want to expose to the client.
This type can be a class, a record, or an interface.
The method invoked should be annotated with the `Remote` attribute and
follows the same convention as static methods:

```fsharp
type MyType() =
    [<Remote>]
    member this.MyMethod(...) = //...
```

You don't need to properly create an instance of the type on the client side, instead there is a helper to create a shim, like this:

```fsharp
//open WebSharper.JavaScript
Remote<MyType>.MyMethod(...)
```

To have a handler for this type, you need to register an instance of it on the server side.

```fsharp
WebSharper.Core.Remoting.AddHandler typeof<MyType> (new MyType())
```

Or, on configuring ASP.NET Core, instead of calling `AddHandler`, you can add the handler to the dependency injection graph using `builder.Services.AddWebSharperRemoting<THandler>()`. [See here for more details.](aspnetcore)

Remote annotated methods can be abstract.
The instance that you provide can be of a subclass of the type in the first argument.
This way you can have parts of server-side RPC functionality undefined in a library, and have them implemented in your web project for better separation of concerns.

```fsharp
// Common server and client code:

[<AbstractClass>]
type MyType() =
    [<Remote>]
    abstract MyMethod : ...

// Client consumer:

Remote<MyType>.MyMethod(...)

// Server implementation:

type MyImpl() =
    inherits MyType()
    
    override this.MyMethod(...) = ...

// App startup:

builder.Services.AddWebSharperRemoting<MyType, MyImpl>() // using overload with two type arguments
```

### Client-side customization

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

