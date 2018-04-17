# Runtime settings

WebSharper's server-side runtime can be customized by a number of settings.

The way to provide these settings depends on the type of application you are running.

## .NET Core

ASP.NET Core applications use `Microsoft.Extensions.Configuration`. By default, the configuration is looked up in a file called `appsettings.json` located at the root of the project, under the `"websharper"` key:

```json
{
  "websharper": {
    "Setting1": "Value1",
    "Setting2": "Value2"
  }
}
```

This can be customized in your `Setup.cs` or `Setup.fs` file.

## .NET Framework

.NET Framework applications use `System.Configuration`. The file used is:

* In an ASP.NET application, such as created by the "Client-Server Application" Visual Studio template: `Web.config`.

* In a standalone executable, such as created by the "Suave-hosted Site" or "Owin-hosted Site" Visual Studio templates: `App.config` (which gets copied during build to the output folder as `YourApplicationName.exe.config`).

This file is XML, and settings are provided as follows:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <appSettings>
    <add key="Setting1" value="Value1" />
    <add key="Setting2" value="Value2" />
  </appSettings>
</configuration>
```

# Available Settings

## Resource URL overrides

Runtime settings can be used to override the URL of declared resources. The key is the fully qualified name of the type declaring the resource, and the value is the URL to use.

For example, in WebSharper.JQuery, the dependency on jQuery's JavaScript file is defined by the type `WebSharper.JQuery.Resources.JQuery`. Therefore, to override the URL to jQuery, you must set:

In `appsettings.json`:
```json
    "WebSharper.JQuery.Resources.JQuery": "https://code.jquery.com/jquery-3.2.1.min.js"
```

In `Web.config` / `App.config`:
```xml
    <add key="WebSharper.JQuery.Resources.JQuery" value="https://code.jquery.com/jquery-3.2.1.min.js" />
```

Note that the fully qualified name is in IL format. This means that nested types and types located inside F# modules are separated by `+` instead of `.`.

[Learn more about WebSharper and Resources.](http://developers.websharper.com/docs/v4.x/fs/resources)

## CDN Settings

You can configure WebSharper to load its core libraries from CDN, rather than locally extracted files in your application, by setting **`WebSharper.StdlibUseCdn`** to `true`.

The default URL for this CDN is `//cdn.websharper.com/{assembly}/{version}/{filename}`. You can change it by setting **`WebSharper.StdlibCdnFormat`**. And finally, you can configure the CDN URL for the resources of a specific assembly (from the standard WebSharper library or not) by setting `WebSharper.CdnFormat.{assemblyname}`.

## UseDownloadedResources

The WebSharper compiler can download external resources so that your project only depends on local files; see [the compiler settings documentation](Configuration.md#downloadResources) for more detail.

To tell the runtime to output references (`<script>` and `<link>` tags) to these downloaded resources, you must set the **`UseDownloadedResources`** runtime setting to `true`.
