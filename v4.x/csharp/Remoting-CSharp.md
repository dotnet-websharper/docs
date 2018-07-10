# Communicating With the Server

## Remoting

The simplest way to implement client-server communication with WebSharper is by remote method calls.
Add the `[Remote]` attibute on a static method and you will be able to call it from
JavaScript-translated code, all serialization will be handled automatically by the WebSharper runtime.
Customization of the remoting mechanism are possible for both the server and client side.

Remote calls can fall into three categories, depending on their return type. 

### Send message

Remote methods returning `void` are sending data to the server without blocking on the client.
No callback is registered so there is no way to confirm success or error state.

### Synchronous request

Synchronous remote calls are possible, but not recommended, as they are blocking JavaScript execution on the client.
They give a deprecated warning with most current JavaScript engines.

### Asynchronous request

Remote methods with return types `Task` or `Task<T>` (or `FSharpAsync`) allow waiting on the result asynchronously.

Here is an example of an RPC call which runs a database query asynchronously on the server:

```csharp
public static class Server
{
    [Remote]
    public static async Task<string[]> GetTitlesByAuthor(string author)
    {
        {
            using (var ctx = new MyDataContext())
            {
                return await (
                    from a in ctx.Articles
                    where a.Author.Name = author
                    select a.Title).ToArrayAsync();
            }
        }
    }
}

[JavaScript]
public class SomeClientSideClass
{

	public async Task DisplayTitles(string author)
	{
		var titles = await Server.GetTitlesByAuthor(author);
		// ...
	}
}
```

Here `await` on the client-side code means that execution of the rest of the async
method will be resumed by a callback after the server has responded.
Task based remoting also propagates errors produced by the underlying XMLHttpRequest
as exceptions, such as the server responding with an error code.

## Features

### Supported types
 
Remote method arguments are serialized from/to JSON.
All types occurring in method parameters and return must be one of the following:

* `System` namespace numeric types, `string`, `bool`, `DateTime`, `TimeSpan`, `Guid`, `Nullable<T>`, `Tuple<...>`. Using `decimal` on the client side needs the `WebSharper.MathJS` package referenced.
* Enums.
* Collection types: `Array<T>` (one-dimensional arrays), `List<T>`, `Queue<T>`, `Stack<T>`, `LinkedList<T>`. Other collection types have to be converted to a supported one.
* Classes with a default constructor for which the values of all fields are themselves serializable 
or marked with the `NonSerialized` attribute. The `Serializable` attribute is not required and does not affect behavior.
* F# union and record types (including those defined in `FSharp.Core`), `FSharpSet<>`, `FSharpMap<_>`.

### Security

Remote methods are exposed as http endpoints, so any security measures have to be integrated into the method body itself.
`WebSharper.Web.Remoting.GetContext().UserSession` exposes some utilities for tracking users.
This uses `System.Web.Security.FormsAuthentication` on the server and cookies in the browser. [See here](WebContext.md) for more information.

```csharp
//using WebSharper.Web;
[Remote]
public static async Task<bool> Login(string user, string password)
{
    var ctx = WebSharper.Web.Remoting.GetContext();
    if (await VerifyLogin(user, password)) {
        await ctx.UserSession.LoginUserAsync(user);
        return true;
    }
    return false;
}
```

### Server-side customization

You can also use instance methods for client-server communication. 
The syntax for this is:

```csharp
//using static WebSharper.JavaScript.Pervasives;
Remote<MyType>.MyMethod(...)
```

The method invoked should be annotated with the `Remote` attribute and
follows the same convention as static methods:

```csharp
public class MyType
{
    [<Remote>]
    public a this.MyMethod(...) = //...
}
```

In this case, you must provide a single instance of each type you use, preferably when the web application starts (such as in `Global.asax`).

```csharp
WebSharper.Core.Remoting.AddHandler(typeof(MyType), new MyType());
```

Remote annotated methods can be abstract or virtual.
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

### Communication Protocol

The communication protocol used by WebSharper is a custom protocol
built on top of HTTP and JSON. The client sends HTTP POST requests
marked with a special HTTP header `x-websharper-rpc` to the current
URL of the page (`?`), with the bodies of the requests containing the
JSON-serialized method arguments. The server responds with a JSON
reply.

The URL to which the requests are sent can be customized modifying the value of `WebSharper.Remoting.EndPoint`
static property on client-side, the default implementation of `IRemotingProvider` uses this string value.
