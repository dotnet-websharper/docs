# ASP.NET Core app with WebSharper

To use WebSharper features in an ASP.NET Core web application,
you will need the packages `WebSharper`, `WebSharper.CSharp` and `WebSharper.AspNetCore`.
Easiest way to get started is to use one of the [templates](../Install.md#netcore).

## IApplicationBuilder extension methods

By having `using WebSharper.AspNetCore;`, the `UseWebSharper` extension method is available for `IApplicationBuilder`
to install the middlewares for WebSharper runtime.

Basic setup in `Configure` method for serving a [Sitelet](sitelets.md) application:

```csharp
    app.UseWebSharper(env, Website.Main);
```

There is no automatic discovery for Sitelet definition as in ASP.NET core.

`UseWebSharper` also has some additional optional parameters:

### Configuration

The `config` parameter accepts an `IConfiguration` object for WebSharper to use for looking up runtime settings.
A standard way to set it up is:

```csharp
    var config =
        ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json")
            .Build();

    app.UseWebSharper(env, Website.Main, config.GetSection("websharper"));
````

### Binaries directory

To override the directory which WebSharper loads dlls from, pass it as the `binDir` parameter.
Default is the directory of the executing assembly calling `UseWebSharper`.

## Using Authentication

To use WebSharper's user sessions, add this to your `ConfigureServices` method:

```csharp
    services
        .AddAuthentication("WebSharper")
        .AddCookie("WebSharper");
```

and also `app.UseAuthentication()` to `Configure`.

## Using only Sitelets or Remoting

The `UseWebSharperSitelets` and `UseWebSharperRemoting` only installs one part of WebSharper's
server-side functionality (`UseWebSharper` is a combination of both).

The parameters are similar to `UseWebSharper`.

## Additional options

`UseWebSharper` has an overload taking a `WebSharperOptions` object.
Use the `WebSharperOptions.Create` static method to get an instance then it allows
changing additional settings, currently only one more.

### AuthenticationScheme

Default is `"WebSharper"`, set it to override the authentication scheme name you are using.
