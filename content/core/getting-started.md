---
title: Getting started with WebSharper
---

Install the WebSharper templates by running the following command in your terminal:

```cmd
dotnet new install WebSharper.Templates
```

This will add the WebSharper templates to your .NET SDK, allowing you to create new WebSharper projects using the `dotnet new` command.
Now you can create a new WebSharper project in Visual Studio or by running:

```cmd
dotnet new websharper-web -lang F# -n MyWebSharperApp
```

The following templates are available:

* `websharper-web`: A client-server application template that includes both server-side and client-side code.
* `websharper-html`: An application template that allows you to generate static HTML sites.
* `websharper-spa`: A single-page application template that you can add routing to handle navigation all on the client side.
* `websharper-svc`: An application template for creating web services.`
* `websharper-lib`: A library template for creating reusable WebSharper libraries.
* `websharper-min`: A minimal application template that provides a clean starting point for WebSharper applications.
* `websharper-ext`: A template for creating JavaScript bindings for WebSharper, allowing you to use external JavaScript libraries from within .NET code.
* `websharper-prx`: A template for creating a WebSharper proxy assembly for an existing .NET library, enabling you to use that library in the client side of a WebSharper project.

## Adding WebSharper to an existing project

If you have an existing .NET project and want to add WebSharper support, you can do so by following these steps:

* Add the WebSharper NuGet package to your project: `dotnet add package WebSharper`
* If you use F#, also add the F# compiler package: `dotnet add package WebSharper.FSharp`, and for C# projects, add the C# compiler package: `dotnet add package WebSharper.CSharp`.
* Create a `wsconfig.json` file in the root of your project. This file is used to configure WebSharper settings for your project. You can start with a basic configuration like this:
```json
{
  "$schema": "https://websharper.com/wsconfig.schema.json",
  "project": "library"
}
```
  See the [WebSharper Compiler Configuration](wsconfig) for more details on available settings.
* If your project is a library, you can start annotating types and methods with the `[<JavaScript>]` attribute to indicate that they should be compiled to JavaScript.
  No JavaScript files will be written, but they will be included as resources in the compiled library for quick use by other projects for debug mode.
  To see the compiled JavaScript right away, you can add the `"jsOutput": "folderName"` setting to your `wsconfig.json`.
* If your project is a web application, take a look at the [ASP.NET Core integration](aspnetcore) to set up WebSharper in your application pipeline.