---
title: Using WebContext
---

Both in sitelets and remote functions, WebSharper provides a value of type `WebSharper.Web.Context` that gives some contextual information about the current request.

## Retrieving the context

### Sitelets

In sitelets, the functions creating sitelet instances like `Sitelet.New`, `Sitelet.Content`, or `Sitelet.Infer`
all take a function argument of type `Web.Context<'T> -> 'T -> Async<Web.Content>` (`Sitelet.Content` is dropping the `'T` value as it's for a single known value). 
This means that when you implement a sitelet, you can retrieve the context by using the function argument directly. For example:

```fsharp
let Main =
    Sitelet.Infer (fun ctx endpoint ->
        match endpoint with
        | EndPoint.Home -> HomePage ctx
        | EndPoint.About -> AboutPage ctx
    )
```

### Remote functions

In [remote functions](remoting), the context can be retrieved using the function `WebSharper.Web.Remoting.GetContext()`. Be careful to only call it from the thread from which your function was called. A typical remote function has the following structure:

```fsharp
open WebSharper
open WebSharper.Web

[<Remote>]
let MyRpcFunction () =
    async {
        // Retrieve the AsyncLocal context
        let context = Remoting.GetContext()
        // Once retrieved, use the context at will here.
        return System.IO.File.ReadAllText(context.RootFolder + "/someContent.txt")
    }
```

<a name="user-sessions"></a>
## User Sessions

The main reason to use the context is to manage user sessions. The member `UserSession` has the following members:

* `LoginUser : username: string * ?persistent: bool -> Async<unit>`

    Logs in the user with the given username. This sets a cookie that is uniquely associated with this username. Set `persistent` to `true` if the user session should last beyond the user's current browser session.

* `LoginUser : username: string * duration: TimeSpan -> Async<unit>`

    Logs in the user with the given username. This sets a cookie that is uniquely associated with this username. The user session should last for the given duration.

* `GetLoggedInUser : unit -> Async<string option>`

    Retrieves the currently logged in user's username, or `None` if the user is not logged in.

* `Logout : unit -> unit`

    Logs the user out.

The implementation of these functions relies on cookies and thus requires that the browser has enabled cookies.

## Other Context functionality

* `ApplicationPath : string` is the virtual application path of the server.

* `RootFolder : string` is the physical folder on the server machine from which the application is running.

* `RequestUri : Uri` is the URI of the request.

* `ResolveUrl : string -> string` resolves URL paths starting with `~` into absolute paths prefixed with the `ApplicationPath`.

* `Environment : Dictionary<string, obj>` is a host-dependent environment.

    * On ASP.NET Core, this contains the following items:
    
        * Under key `"WebSharper.AspNetCore.HttpContext"`, the HTTP context, of type `Microsoft.AspNetCore.Http.HttpContext`.

* `Json : WebSharper.Core.Json.Provider` is an instance of the JSON provider, which can be used to serialize and deserialize JSON data.

* `Metadata : WebSharper.Core.Metadata.Info` is the runtime metadata for the current application, which can be used to inspect the types available in the client code in the WebSharper application.

## Sitelet Context

For sitelets, the object is typed with the type of the endpoint, so you get an instance of `WebSharper.Web.Context<Endpoint>``. This provides a method for generating safe links.

* `Link : Endpoint -> string` generates a link to the given endpoint, ensuring that the link is safe and correctly formatted.

        