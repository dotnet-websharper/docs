---
title: Setting up WebSharper in ASP.NET Core
---

To integrate WebSharper into your web application, use the `WebSharper.AspNetCore` nuget package. This documentation is for WebSharper version 9.1.3 and above. 

# Configuring startup

Configuring the startup for an ASP.NET Core application has two distinct stages: first setting up services and then the application pipeline. WebSharper relies on some services to run.

- Call `builder.Services.AddWebSharper()` to set up required singleton service (or `services.AddWebSharper()` in the `ConfigureServices` method of your `Startup` class). Any of the following methods also calls this implicitly, so you can use them instead if those are more specific to your needs.
- Call `builder.Services.AddSitelet(mySitelet)` to register a WebSharper sitelet. However, this is no longer the recommended method; instead, set your sitelet in the pipeline configuration as described below.
- Call `builder.Services.AddWebSharperRemoting<THandler>()` to register a WebSharper remoting handler for instance-based remoting. This is a typed alternative to `WebSharper.Core.Remoting.AddHandler`. This method has 3 overloads, you can pass it a single type argument which will be instantiated, two type arguments where the first is the type of the handler and the second is the type of the implementation class, or one type argument and pass it an instance of that type.
- Call `builder.Services.AddWebSharperContent()` to register a scoped service to allow embedding WebSharper content into Razor pages.

Next are the middleware to put in the application pipeline.

WebSharper has two main middleware, one is for serving pages (sitelets), and the other for API endpoints generated for automatic remoting.

- Add `app.UseWebSharper()` for a basic setup of both remoting and sitelets if configured. It has a builder parameter that can be used for additional configuration:
  - `Sitetet` passes a sitelet instance to server.
  - `DiscoverSitelet` looks for a class property with the `[<Website>]` attribute to obtain the sitelet instance. This is now deprecated.
  - `UseSitelets(false)` turns off serving a sitelet entirely. `UseSitelets(true)` has no effect, it's the default.
  - `UseRemoting(false)` turns off using remoting entirely. `UseRemoting(true, headers)` can be used to define additional headers to return.
  - `Use` registers an additional action to be executed after the `WebSharperOptions` object is constructed.
  - `Logger` adds a logger for WebSharper startup, default to the discoverable `ILogger<IWebSharperService>` service. Alternatively you can also pass it an `ILoggerFactory`.
- The following options are also available in the builder, but usually not necessary to override defaults:
  - `SiteletAssembly` specifies which assembly contains the runtime metadata, defaults to the calling assembly having the startup code.
  - `Metadata` allows passing WebSharper metadata, defaults to the deserialized runtime metadata.
  - `Config` allows passing runtime configuration, defaults to the `"websharper"` section of `"appsettings.json"`.
  - `ContentRootPath` specifies the application root for the WebSharper context, defaults to `hostingEnvironment.ContentRootPath`.
  - `WebRootPath` specifies the web root for the WebSharper context, defaults to `hostingEnvironment.WebRootPath`.
  - `AuthenticationScheme` Defines the name of the authentication scheme to use `WebSharper.Web.Context.UserSession`, defaults to `"WebSharper"`.
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

Remember to add required service with `builder.Services.AddWebSharperContent()`. Then, add a placeholder for WebSharper-generated startup script in your layout page:

```html
@inject WebSharper.AspNetCore.IWebSharperContentService WebSharperContentService
...
<head>
  ...
  @WebSharperContentService.Head()
</head>
```

Then in your Razor PageModel file (code backend), use the `IWebSharperContentService` service with dependency injection, and then you can create a property that defines a WebSharper content embedded into the page. Here with also creating a named optimized bundle for the page:

```csharp
      public IHtmlContent MyWebSharperControl =>
          _webSharperContentService.Render(                
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
