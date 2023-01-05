---
order: -20
label: "WebSharper on NuGet"
---
# NuGet packages

WebSharper uses NuGet packages to ship the **core tools** (the F#/C# to JS compilers, build automation, etc.) - often just referred to as WebSharper, **proxies** for the F# standard library and other .NET libraries, and **bindings** (also called "extensions") to JavaScript libraries. This makes it easy and predictable to add or update various WebSharper components in your applications.

---

## Configuring the WebSharper developer feed

WebSharper and its components are available on NuGet.org. In addition, if you would also like to access the latest developer packages, you can find them on the WebSharper GitHub packages feed at [https://nuget.pkg.github.com/dotnet-websharper/index.json](https://nuget.pkg.github.com/dotnet-websharper/index.json):

```fsharp !#6
> dotnet nuget list source
Registered Sources:
  1.  nuget.org [Enabled]
      https://api.nuget.org/v3/index.json
  2.  dotnet-websharper-GitHub [Enabled]
      https://nuget.pkg.github.com/dotnet-websharper/index.json
```

The standard NuGet feed should come configured on your system when installing the .NET SDK. If you want to configure the WebSharper GitHub feed as well, you can do so as follows:

```text
dotnet nuget add source https://nuget.pkg.github.com/dotnet-websharper/index.json --name dotnet-websharper-GitHub --username <GH_USER> --password <PAT>
```

where `GH_USER` is your GitHub username, and `PAT` is your Personal Access Token \(PAT\) for your GitHub account.

!!!
At the time of writing, GitHub requires authentication to access packages on its Packages feed. To set your access up, create a new PAT at [https://github.com/settings/tokens/new](https://github.com/settings/tokens/new) with the `read:packages` scope, noting the expiration you configure.
!!!

If your project uses [Paket](https://fsprojects.github.io/Paket/), you will need to configure the above GitHub feed for Paket as well:

```text
paket config add-credentials https://nuget.pkg.github.com/dotnet-websharper/index.json --username <GH-USER> --password <PAT>
```

---

## Core NuGet packages

Below are some of the WebSharper NuGet packages that you will be working with:

| Package name | What does it provide? |
| :--- | :--- |
| `WebSharper` <br /> [![](https://img.shields.io/nuget/v/websharper?label=&style=for-the-badge)](https://nuget.org/packages/WebSharper) | The core package for any WebSharper project. Includes proxies for the F# standard library, the core JavaScript bindings for DOM, EcmaScript, etc., the sitelets runtime with remoting and static server-side content generation, and the WebSharper Interface Generator (WIG) for creating your own JavaScript bindings. Reference it in any WebSharper project/library, along with `WebSharper.FSharp` or `WebSharper.CSharp`, depending on your source language. |
| `WebSharper.FSharp` <br /> [![](https://img.shields.io/nuget/v/websharper.fsharp?label=&style=for-the-badge)](https://nuget.org/packages/WebSharper.FSharp) | `msbuild` targets/build automation to light up WebSharper features, including the main F# to JavaScript compiler. |
| `WebSharper.CSharp` <br /> [![](https://img.shields.io/nuget/v/websharper.csharp?label=&style=for-the-badge)](https://nuget.org/packages/WebSharper.CSharp) | Same as `WebSharper.FSharp`, except for C#. |
| `WebSharper.AspNetCore` <br /> [![](https://img.shields.io/nuget/v/websharper.aspnetcore?label=&style=for-the-badge)](https://nuget.org/packages/WebSharper.AspNetCore) | Helpers to use WebSharper from an ASP.NET Core project. You only usually need it for your web projects, not libraries (unless the library builds on some ANC functionality.) |
| `WebSharper.Charting` <br /> [![](https://img.shields.io/nuget/v/websharper.charting?label=&style=for-the-badge)](https://nuget.org/packages/WebSharper.Charting) | A [charting library](https://github.com/dotnet-websharper/charting) with a similar API to [FSharp.Charting](https://fslab.org/FSharp.Charting/) and configurable renderers. |
| `WebSharper.Forms` <br /> [![](https://img.shields.io/nuget/v/websharper.forms?label=&style=for-the-badge)](https://nuget.org/packages/WebSharper.Forms) | A reactive [forms library](https://github.com/dotnet-websharper/forms) built on top of `WebSharper.UI`, with support for declarative, composable web forms with retargetable rendering. |
| `WebSharper.UI` <br /> [![](https://img.shields.io/nuget/v/websharper.ui?label=&style=for-the-badge)](https://nuget.org/packages/WebSharper.UI) | A highly performant, reactive [UI library](https://github.com/dotnet-websharper/ui) with type-safe templating. The recommended way to deal with reactive HTML. |
| `WebSharper.Testing` <br /> [![](https://img.shields.io/nuget/v/websharper.testing?label=&style=for-the-badge)](https://nuget.org/packages/WebSharper.Testing) | A client-side testing framework for WebSharper. |
| `WebSharper.Templates` <br /> [![](https://img.shields.io/nuget/v/websharper.templates?label=&style=for-the-badge)](https://nuget.org/packages/WebSharper.Templates) | The WebSharper [project templates]((https://github.com/dotnet-websharper/templates)) for F# and C#. |

---

### Compiler packages

For more advanced use cases, where you need custom JavaScript compilation, you can use the following packages:

| Package name | What does it provide? |
| :--- | :--- |
| `WebSharper.Compiler.Common` <br /> [![](https://img.shields.io/nuget/v/websharper.compiler.common?label=&style=for-the-badge)](https://nuget.org/packages/WebSharper.Compiler.Common) | Core WebSharper compiler utilities shared by the F# and C# to JavaScript compilers, including offline sitelet generation. |
| `WebSharper.Compiler.FSharp` <br /> [![](https://img.shields.io/nuget/v/websharper.compiler.fsharp?label=&style=for-the-badge)](https://nuget.org/packages/WebSharper.Compiler.FSharp) | The F# to JavaScript compiler. Depends on `WebSharper.Compiler.Common` and `FSharp.Compiler.Services`. |
| `WebSharper.Compiler.CSharp` <br /> [![](https://img.shields.io/nuget/v/websharper.compiler.csharp?label=&style=for-the-badge)](https://nuget.org/packages/WebSharper.Compiler.CSharp) | The C# to JavaScript compiler. Depends on `WebSharper.Compiler.Common` and `Microsoft.CodeAnalysis.CSharp` (a.k.a. Roslyn). |
| `WebSharper.Compiler` <br /> [![](https://img.shields.io/nuget/v/websharper.compiler?label=&style=for-the-badge)](https://nuget.org/packages/WebSharper.Compiler) | Includes both `WebSharper.Compiler.FSharp` and `WebSharper.Compiler.CSharp`. |
