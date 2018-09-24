# ASP.NET Core app with WebSharper

To use WebSharper features in an ASP.NET Core web application,
you will need the packages `WebSharper`, `WebSharper.CSharp` and `WebSharper.AspNetCore`.
The easiest way to get started is to use one of the [templates](Install.md#netcore).

WebSharper can be enabled and configured [in the `Configure` method](#configure), and uses a few services configured [in the `ConfigureService` method](#configureServices). Here is a recommended default setup:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSitelet(mySitelet)
                .AddAuthentication("WebSharper")
                .AddCookie("WebSharper")
    }
    
    public void Configure(IApplicationBuilder app)
    {
        app.UseAuthentication()
            .UseWebSharper()
            .UseStaticFiles()
    }
}
```

<a name="configure"></a>
## `Configure` setup

### `UseWebSharper()`

By having `open WebSharper.AspNetCore`, the `UseWebSharper` extension method is available for `IApplicationBuilder`. It installs the middlewares for WebSharper runtime.

For a basic setup, simply call `UseWebSharper()`:

```csharp
public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        app.UseWebSharper();
    }
}
```

You can also configure WebSharper by passing a builder function. For example:

```csharp
public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        app.UseWebSharper(builder =>
        {
            builder.Sitelet(mySitelet)
                .UseRemoting(false);
        });
    }
}
```

The following methods are available for the builder:

* `builder.UseSitelets(bool u = true)` tells WebSharper whether to serve sitelets; the default is true.

    If this is true, then either `builder.Sitelet()` or `services.AddSitelet()` must be called to declare the sitelet to serve; otherwise, a runtime error is thrown.

* `builder.UseRemoting(bool u = true)` tells WebSharper whether to serve remote functions; the default is true.

* `builder.Sitelet(Sitelet<'EndPoint> s)` tells WebSharper to serve the given sitelet. This will be ignored if `UseSitelets(false)` is called. Note that it is advised to pass the sitelet as a service rather than through the builder; see [Sitelets](#sitelets) for more details.

* `builder.Config(IConfiguration c)` tells WebSharper to use this appSettings configuration. By default, WebSharper uses the `websharper` subsection of the host configuration.

* `builder.Logger(ILogger l)` tells WebSharper to use this logger for its internal messages. By default, WebSharper uses an injected `ILogger<WebSharperOptions>`.

* `builder.BinDir(string d)` tells WebSharper to look for assemblies with WebSharper metadata in this directory. By default, it uses the directory where WebSharper.AspNetCore.dll is located.

* `builder.AuthenticationScheme(string s)` tells WebSharper to use the given authentication scheme for `Web.Context.UserSession`. See [Authentication](#authentication) for more details.

### `UseStaticFiles()`

If you have any client-side code, then it will need access to compiled JavaScript files, which the WebSharper middleware does not provide. So you need to add the default static files provider:

```csharp
app.UseWebSharper()
    .UseStaticFiles()
```

Note that `UseStaticFiles()` should be called _after_ `UseWebSharper()`. This way, `WebSharper` sitelets and remoting have a chance to handle requests even if they also happen to match a static file.

### `UseAuthentication()`

If you wish to use `Web.Context.UserSession`, then you need to call `app.UseAuthentication()`, and to [configure its services](#authentication).

```csharp
app.UseAuthentication()
    .UseWebSharper()
    .UseStaticFiles()
```

Note that `UseAuthentication()` should be called _before_ `UseWebSharper()`. This way, `Authentication` can parse session information from the request that `WebSharper` can then use.

<a name="configureServices"></a>
## `ConfigureServices` setup

WebSharper uses ASP.NET Core's dependency injection for a number of services.

<a name="sitelets"></a>
### Sitelets

The recommended way to tell WebSharper which sitelet to serve is to use `services.AddSitelet()`. There are several variants:

* `AddSitelet(Sitelet<'EndPoint> s)` simply uses the given Sitelet value.

    ```csharp
    public type Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSitelet(mySitelet);
        }
    }
    ```

* `AddSitelet<T when T : ISiteletService>()` uses a Sitelet created through dependency injection.

    ```csharp
    public type MyWebsite : ISiteletService
    {
        public override Sitelet<obj> Sitelet { get; private set; }
        
        public MyWebsite()
        {
            InitSitelet();
        }
        
        private void InitSitelet()
        {
            Sitelet = new SiteletBuilder()
                .With("/", ctx => Content.Text("Hello, world!"))
                .Install();
        }
    }

    public type Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSitelet<MyWebsite>();
        }
    }
    ```

    The type `MyWebsite` can now use injected services:

    ```csharp
    public type MyWebsite : ISiteletService
    {
        private ILogger<MyWebsite> logger;

        public override Sitelet<obj> Sitelet { get; private set; }
        
        public MyWebsite(ILogger<MyWebsite> logger)
        {
            this.logger = logger;
            InitSitelet();
        }
        
        private void InitSitelet()
        {
            Sitelet = new SiteletBuilder()
                .With("/", ctx =>
                {
                    logger.LogInformation("Serving a sitelet page!");
                    return Content.Text("Hello, world!"));
                }
                .Install();
        }
    }
    ```

<a name="remoting"></a>
### Remoting

To use simple static method-based remoting, there is nothing to add: as long as there is no `UseRemoting(false)` in your call to `UseWebSharper()`, remote static methods will be served.

WebSharper.AspNetCore additionally provides a way to inject a remoting handler (see Server-side customization [here](#handler)) with dependencies. For this, simply register your handler using `services.AddWebSharperRemoting<_>()`. This call replaces the call to `WebSharper.Core.Remoting.AddHandler` that would otherwise be needed.

```csharp
// Rpc handler:
public class MyRpc
{
    private ILogger<MyRpc> logger;

    public MyRpc(ILogger<MyRpc> logger)
    {
        this.logger = logger;
    }

    [Remote]
    public void MyMethod()
    {
        logger.LogInformation("MyMethod was called");
    }
}

// Registering:
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddWebSharperRemoting<MyRpc>();
    }
}
```

If you have an abstract remoting handler, you can register an implementation using `AddWebSharperRemoting<_, _>()`.

```csharp
// Abstract Rpc handler:
public abstract class MyRpc
{
    [Remote]
    public abstract void MyMethod();
}

// Implementation of the Rpc handler:
public class MyRpcImpl : MyRpc
{
    private ILogger<MyRpc> logger;

    public MyRpc(ILogger<MyRpc> logger)
    {
        this.logger = logger;
    }

    public override void MyMethod()
    {
        logger.LogInformation("MyMethod was called");
    }
}

// Registering:
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddWebSharperRemoting<MyRpc, MyRpcImpl>();
    }
}
```

<a name="authentication"></a>
### Authentication

To use WebSharper's user sessions, add this to your `ConfigureServices` method:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication("WebSharper")
                .AddCookie("WebSharper");
    }
}
```

and also `app.UseAuthentication()` to `Configure`.

If you configured a different authentication scheme in [`UseWebSharper()`](#configure), then you need to use that same scheme name here instead of `"WebSharper"`.
