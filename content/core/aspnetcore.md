---
title: Setting up WebSharper in ASP.NET Core
---

To integrate WebSharper into your web application, use the `WebSharper.AspNetCore` nuget package. This documentation is for WebSharper version 9.1.3 and above. 

# Configuring options and startup

Configuring an ASP.NET Core application has two distinct stages: first setting up services and then the application pipeline. WebSharper relies on some services to run.

- Call `builder.Services.AddWebSharper()` to set up required singleton service (or `services.AddWebSharper()` in the `ConfigureServices` method of your `Startup` class for older versions of ASP.NET Core). It can take an optional action to set properties of the `WebSharperOptions` object, these are:
    - `DefaultAssembly`: The assembly to load runtime metadata from to run WebSharper sitelets and remoting. Defaults to the entry assembly.
    - `AuthenticationScheme`: The authentication scheme to use for WebSharper's built-in cookie authentication. Defaults to `"WebSharper"`.
    - `Configuration`: An `IConfiguration` object accessible from WebSharper `Context`. Defaults to the `"websharper"` section of the main `IConfiguration` instance available (usually loaded from `appsettings.json` by Kestrel defaults).
    - `ContentRootPath`: The content root path used by WebSharper. Defaults to the `ContentRootPath` from the `IHostingEnvironment` instance.
    - `WebRootPath`: The web root path used by WebSharper. Defaults to the `WebRootPath` from the `IHostingEnvironment` instance.
- Call `builder.Services.AddWebSharperServices()` for the same setup, the difference is that it returns a builder for additional WebSharper service helpers. These are:
    - `.AddRemotingHandler<THandler>()` to register a WebSharper remoting handler for instance-based remoting. This is a typed, service-level alternative to `WebSharper.Core.Remoting.AddHandler`. This method has 3 overloads, you can pass it a single type argument which will be instantiated, two type arguments where the first is the type of the handler and the second is the type of the implementation class, or one type argument and pass it an instance of that type.
    - `.AddMvc()` registers a scoped service to allow embedding WebSharper content into Razor pages.
    - `.AddSiteletRef(siteletRef)` registers a `ref<Sitelet<'T>>` to serve as a fallback if no static sitelet value is configured in the pipeline. This can be used to make a site where the sitelet is expanded dynamically.
    - `.AddRuntimeRef(runtimeRef)` registers a reference to all objects needed for the WebSharper runtime, these are the sitelet value, runtime metadata, dependency graph, json serializer, and remoting server. This can be used to make fully dynamic WebSharper servers.

Additionally, these are the services you can directly configure with `.AddSingleton` or `.AddScoped`, overriding the defaults set by `AddWebSharper`/`AddWebSharperServices`:

- `IOptions<WebSharperOptions>`: another way to access and configure the `WebSharperOptions` object.
- `IWebSharperSiteletService`: provides a sitelet to serve as a fallback if no static instance is configured in the middleware. It is not set by default.
- `IWebSharperMetadataService`: provides WebSharper runtime metadata (for example used for looking up sitelet bundles, remote functions, rendering page initialization code).
- `IWebSharperRemotingServerService`: provides the WebSharper remoting server.
- `IWebSharperJsonProviderService`: provides the WebSharper json serializer.
- `ILogger<WebSharperInitializationService>`: the logger used by WebSharper initialization.
- `ILogger<WebSharperSiteletMiddleware>`: the logger used by sitelets runtime.

Next are the middleware to put in the application pipeline.

WebSharper has two main middleware, one is for serving pages (sitelets), and the other for API endpoints generated for automatic remoting.

## Configuring the pipeline

- Add `app.UseWebSharper()` for a basic setup of both remoting and sitelets if configured. It has a builder parameter that can be used for additional configuration:
  - `Sitetet` passes a sitelet instance to server.
  - `DiscoverSitelet` looks for a class property with the `[<Website>]` attribute to obtain the sitelet instance. This is now deprecated.
  - `UseSitelets(false)` turns off serving a sitelet entirely. `UseSitelets(true)` has no effect, it's the default.
  - `UseRemoting(false)` turns off using remoting entirely. `UseRemoting(true, headers)` can be used to define additional headers to return.
  - `Use` registers an additional action will get the instance of `IWebSharperInitializationService`.
  - `Logger` adds a logger for WebSharper startup, default to the discoverable `ILogger<IWebSharperService>` service. Alternatively you can also pass it an `ILoggerFactory`.
- The following options are also available in the builder, but usually not necessary to override defaults:
  - `SiteletAssembly` specifies which assembly contains the runtime metadata, defaults to the calling assembly having the startup code.
- Add `app.UseWebSharperRemoting` as a shortcut to use remoting only. It also allows to define additional headers to return as a shortcut to using the builder.
- Add `app.UseWebSharperSitelets` as a shortcut to use sitelets only.
- Add `app.UseWebSharperScriptRedirect` to set up script redirection to a localhost server. If the `startVite` parameter is set to `true`, it starts up `vite` in a separate console window for local debugging. The `redirectUrlRoot` argument defines the local url to use, defaults to the `websharper:DebugScriptRedirectUrl` setting from config.

## Serving JavaScript files

If you have any client-side code, then it will need access to compiled JavaScript files, which the WebSharper middleware does not provide. So you need to add the default static files provider `app.UseStaticFiles()`.

Note that `UseStaticFiles` should be called _after_ `UseWebSharper`. This way, WebSharper sitelets and remoting have a chance to handle requests even if they also happen to match a static file.

## Authentication

To use WebSharper's user sessions, add this to your service configuration:

```csharp
builder.Services.AddAuthentication("WebSharper").AddCookie("WebSharper")
```
and also `app.UseAuthentication()` to your application pipeline.

If you configured a different authentication scheme in `UseWebSharper`, then you need to use that same scheme name here instead of `"WebSharper"`.

# Integrations

## Return WebSharper content from an MVC controller

Use the `.ToIActionResult()` extension method available in the `WebSharper.AspNetCore` namespace to convert WebSharper content into a response object that ASP.NET Core can handle. For example:

```csharp
using WebSharper.AspNetCore;
using static WebSharper.UI.Html;

// ...

[HttpGet]
public IActionResult Index()
{
    return WebSharper.Sitelets.Content.Page(
        div("Hello from WebSharper!")
    ).ToIActionResult();
}
```

## Embed WebSharper controls into Razor pages

Remember to add required service with `builder.Services.AddWebSharperServices().AddMvc()`. Then, add a placeholder for WebSharper-generated startup script in your layout page:

```html
@inject WebSharper.AspNetCore.IWebSharperMvcService WebSharperMvcService
...
<head>
  ...
  @WebSharperMvcService.Head()
</head>
```

Then in your Razor PageModel file (code backend), use the `IWebSharperMvcService` service with dependency injection, and then you can create a property that defines a WebSharper content embedded into the page. Here with also creating a named optimized bundle for the page:

```csharp
      public IHtmlContent MyWebSharperControl =>
          _webSharperMvcService.Render(                
              WebSharper.Sitelets.Content.Bundle("index", new MyWebSharperControl()));
```

Use this property within your page, like `@Model.MyWebSharperControl`. Do not create `WebSharper.Web.Control`s directly in your Razor expressions as the WebSharper compiler cannot discover them there for preprocessing.

For a small working example, check out https://github.com/dotnet-websharper/aspnetmvc

## Middleware for Giraffe/Saturn

`WebSharper.AspNetCore` comes with two helpers to create http handlers for Giraffe/Saturn, these are `Sitelets.HttpHandler(sitelet)` and `Remoting.HttpHandler()`.
Use them like this in Giraffe:

```fsharp
let webApp =
    choose [
        GET >=>
            choose [
                routef "/hello/%s" indexHandler
            ]
        Remoting.HttpHandler ()
        GET >=>
            choose [
                routef "/helloagain/%s" indexHandler
            ]
        Sitelets.HttpHandler mySitelet
        setStatusCode 404 >=> text "Not Found" ]
```
