# Web Context

Both in Sitelets and Rpc functions, WebSharper provides a value of type `WebSharper.Web.IContext` that gives some contextual information about the current request.

## Retrieving the context

### Sitelets

In Sitelets, the context provided by [content-generating functions](Sitelets.md#content) such as `Content.Page` or `Content.Custom` implements `Web.IContext`, so you can use it directly.

### Remote functions

In Remote functions, the context can be retrieved using the function `WebSharper.Web.Remoting.GetContext()`. Be careful to only call it from the thread from which your function was called. A typical Remote has the following structure:

```csharp
using WebSharper;
using WebSharper.Web; // Important: *Async() methods are extensions defined here

[Remote]
public static async Task<bool> Login(string user, string password)
{
    // Retrieve the context before the first await.
    var ctx = WebSharper.Web.Remoting.GetContext();

    if (await VerifyLogin(user, password)) {
        // Once retrieved, use the context at will here.
        await ctx.UserSession.LoginUserAsync(user);
        return true;
    }
    return false;
}
```

<a name="user-sessions"></a>
## User Sessions

The main reason to use the context is to manage user sessions. The member `UserSession` has the following members:

* `Task LoginUserAsync(string username, bool persistent = false)`

    Logs in the user with the given username. This sets a cookie that is uniquely associated with this username. Set `persistent` to `true` if the user session should last beyond the user's current browser session.

* `Task LoginUserAsync(string username, TimeSpan duration)`

    Logs in the user with the given username. This sets a cookie that is uniquely associated with this username. The user session should last for the given duration.

* `Task<string> GetLoggedInUserAsync()`

    Retrieves the currently logged in user's username, or `null` if the user is not logged in.

* `Task Logout()`

    Logs the user out.

The implementation of these functions relies on cookies and thus requires that the browser has enabled cookies.

## Other Context functionality

* `string ApplicationPath` is the virtual application path of the server.

* `string RootFolder` is the physical folder on the server machine from which the application is running.

* `Uri RequestUri` is the URI of the request.

* `string ResolveUrl(string url)` resolves URL paths starting with `~` into absolute paths prefixed with the `ApplicationPath`.

* `Dictionary<string, obj> Environment` is a host-dependent environment.

    * On ASP.NET Core, this contains the following items:
    
        * Under key `"WebSharper.AspNetCore.HttpContext"`, the HTTP context, of type `Microsoft.AspNetCore.Http.HttpContext`.
        
        * Under key `"WebSharper.AspNetCore.Services"`, the dependency injection service provider, of type `Microsoft.Extensions.DependencyInjection.IServiceProvider`.

    * On ASP.NET 4.x, this contains the following item:

        * Under key `"HttpContext"`, the HTTP context, of type `System.Web.HttpContextBase`.

    * On OWIN, this contains the OWIN environment proper. Additionally, if this is OWIN on ASP.NET 4.x (using Microsoft.Owin.Host.SystemWeb), the above `"HttpContext"` key is added to the environment.
