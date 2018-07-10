# Communicating With the Server

WebSharper supports remote procedure calls from the client
(JavaScript environment) to the server (ASP.NET or other hosting
environment). Remoting is designed to be safe and efficient while
requiring as little boilerplate code as possible.

Here is a simple example of a client-side and a server-side function
pair that communicate with RPC:

```fsharp
module Server =
    
    [<Rpc>]
    let GetBlogsByAuthor author =
        use db = new DbContext()
        let blogs = db.GetBlogsByAuthor author
        async { return blogs }
    
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

* RPC-callable methods are marked with `RemoteAttribute`.

* RPC-callable methods are safe to call from the web by an
  unauthenticated client.
  
* RPC-callable methods have argument types that are serializable to
  JSON.

* RPC-callable methods have a return type that is serializable to
  JSON, or are of type `Async<'T>` where `'T` is such a type.

JSON serializers are automatically derived for the following types,
where `'T1, 'T2, ...` are arbitrary JSON-serializable types:

* `unit`
* `bool`
* `int`
* `int64`
* `double`
* `string`
* `System.DateTime`
* `'T []`
* `'T1 * 'T2 * ... * 'Tn`
* unions (including `option` and `list`)
* records
* `Map<'K, 'V>`
* `Set<'T>`
* classes with a default constructor

For records, unions and classes to be JSON-serializable, all their
fields must also be JSON-serializable.

The remoting mechanism supports three different ways of doing a remote
call: message-passing, synchronous and asynchronous.

### Message-Passing Calls

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

### Asynchronous Calls

These calls allow for asynchronous, callback-based processing of the
server response.  They utilize the `Async<'T>` abstraction from F# to
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

Note that using `Async` on the server side means that your code can
switch threads. Extra care should be taken to acquire references to
thread-local objects such as `System.Web.HttpContext.Current` before
entering the async expression. It is recommended to use
[Web.IContext](WebContext.md).

### Synchronous Calls

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

* The server (in ASP.NET context, the WebSharper handler) parses the
  request and looks up the requested method.

* The server makes sure the method is marked with `RpcAttribute`.

* The server binds to the method, deserializes the arguments from
  JSON, and calls it.

* The server serializes the method's response to JSON and responds to
  the request.

* The client deserializes the response `1` from JSON and returns it.

* The client unblocks the browser.

### Supported types
 
Remote method arguments are serialized from/to JSON.
All types occurring in method parameters and return must be one of the following:

* `System` namespace numeric types, `string`, `bool`, `DateTime`, `TimeSpan`, `Guid`, `Nullable<_>`, `Tuple<...>`. Using `decimal` on the client side needs the `WebSharper.MathJS` package referenced.
* F# union and record types (including built-in types like `option<_>`, `list<_>`, `Result<_>`), `Map<_>`, `Set<_>`.
* Enums.
* Collection types: `System.Array<_>` (one-dimensional arrays), `System.Collections.Generic` types `List<_>`, `Queue<_>`, `Stack<_>`, `LinkedList<_>`. Other collection types have to be converted to a supported one.
* Classes with a default constructor for which the values of all fields are themselves serializable 
or marked with the `System.NonSerialized` attribute. The `System.Serializable` attribute is not required and does not affect behavior.

### Security

Remote methods are exposed as http endpoints, so any security measures have to be integrated into the method body itself.
`WebSharper.Web.Remoting.GetContext().UserSession` exposes some utilities for tracking users.
This uses `System.Web.Security.FormsAuthentication` on the server and cookies in the browser. [See here](WebContext.md) for more information.

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

### Server-side customization

You can also use instance methods for client-server communication. 
The syntax for this is:

```fsharp
//open WebSharper.JavaScript
Remote<MyType>.MyMethod(...)
```

The method invoked should be annotated with the `Remote` attribute and
follows the same convention as static methods:

```fsharp
type MyType() =
    [<Remote>]
    member this.MyMethod(...) = //...
```

In this case, you must provide a single instance of each type you use, preferably when the web application starts (such as in `Global.asax`).

```fsharp
WebSharper.Core.Remoting.AddHandler typeof<MyType> (MyType())
```

Remote annotated methods can be abstract.
The instance that you provide by `AddHandler` can be of a subclass of the type in the first argument.
This way you can have parts of server-side RPC functionality undefined in a library, and have them implemented in your web project for better separation of concerns.

### Client-side customization

If you want to change the URL that remoting calls target by default, set the value at the `WebSharper.Remoting.EndPoint` property at the startup of your client-side code.

You can You can further customize how RPC methods are called from the client-side code by writing a class implementing the 
 customize how RPC methods are called from the client-side code by writing a class implementing the 
`WebSharper.Remoting.IRemotingProvider` interface, then adding a `RemotingProvider` attribute on the remote method
with the custom remoting provider type as argument.

The `IRemotingProvider` provider has 4 methods for implementing synchronous, asynchronous (Task and F# Async), and send calls: `Sync`, `Task`, `Async`, `Send` respectively.
You have to implement only the methods that your RPC methods will be using depending on their signatures.

The `RemotingProvider` attribute can be also used on classes and assemblies, overriding client-side remote handling for
all RPC methods found in their scope.

The default implementation used is `WebSharper.Remoting.AjaxRemotingProvider`.

You can inherit this class for easy customization, it has an `AsyncBase` method that are used by all 3 non-blocking `IRemotingProvider` method implementations. It also has an `Endpoint` property, that looks up the global `WebSharper.Remoting.EndPoint` by default. Example:

```fsharp
[<JavaScript>]
type SafeRemotingProvider() =
    inherit Remoting.AjaxRemotingProvider()

    override this.Endpoint = "https://myserver.com"

    override this.AsyncBase(handle, data) =
        let def = base.AsyncBase(handle, data) 
        async {
            try return! def
            with e ->
                Console.Log("Remoting exception", handle, e)
                return box None
        }
```

Then all `[Remote]` methods that you also annotate with `[RemotingProvider(typeof<SafeRemotingProvider>)]` will send the request to `myserver.com` and also catch any exceptions are happening during the call and return `None` instead.
If all your remote methods are returning `Option`s, this will be a valid value.

## Communication Protocol

The communication protocol used by WebSharper is a custom protocol
built on top of HTTP and JSON. The client sends HTTP POST requests
marked with a special HTTP header `x-websharper-rpc` to the current
URL of the page (`?`), with the bodies of the requests containing the
JSON-serialized method arguments. The server responds with a JSON
reply.

The URL to which the requests are sent can be customized by subclassing
from the `RpcAttribute`.
