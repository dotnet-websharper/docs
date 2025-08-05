---
title: Using imports and script links
---

To use external JavaScript files in your WebSharper project, there are a couple considerations:
* Is your project a library or a web project? 
* Is the JavaScript file modular (ES6) or not?
* If a library, do you want to embed the JavaScript file as a resource or just link to it from a CDN or depend on npm?

First we'll see how to embed/link external JavaScript in library projects.

## Embedding a file

If you have a JavaScript file, first you need to add it as an embedded resource in your project file.

```xml
    <EmbeddedResource Include="sayHi.js" />
```
Then add the following assembly attribute to your code:
```fsharp
open WebSharper

[<assembly: WebResource("sayHi.js", "text/javascript")>]
do()
```
This marks the file to be unpacked by WebSharper into the output folder (usually it will end up in `wwwroot\Scripts\WebSharper`) when a web project is built.

## Using a JavaScript module

If the JavaScript file is a module, you can create bindings for it using the `Import` attribute. This allows you to use the functions and variables defined in that module in your WebSharper code.

```fsharp
[<Import("./sayHi.js")>] // imports the default export, applies to all Inlines/Stubs within class
type SayHi [<Inline "new $import()">] () =
    
    // FooInst is an instance member, we don't have to refer to an import directly, so its effect is ignored
    [<Inline "$this.FooInst()">]
    member this.FooInst() = X<string>

    // Foo is a static member of SayHi class, this inherits the Import attr from the type
    [<Inline "$import.Foo()">]
    static member Foo() = X<string>
 
    // Bar is a separate named export, we redefine the Import attr which hides the one inherited from the type
    [<Import("Bar", "./sayHi.js"); Inline "$import()">]
    static member Bar() = X<string>
```

Here, the `Import` attribute specifies the JavaScript file to import, and the `Inline` attribute is used to define how the function or variable should be called in JavaScript.

Alternatively, you can use the `WebSharper.JavaScript.JS.Import` family of function to import a JavaScript module and use its exports directly in your code.
```fsharp
open JavaScript

[<Inline>]
let importTestJs : obj = JS.Import("testExport", "./test.js")
// translates to: import { testExport } from "./test.js"

[<Inline>]
let importTestJsAll : obj = JS.ImportAll "./test.js" // alternatively JS.Import("*", "./test.js")
// translates to: import * as test from "./test.js"

[<Inline>]
let importTestJsDefault : obj = JS.ImportDefault "./test.js" // alternatively JS.Import("default", "./test.js")
// translates to: import test from "./test.js"
```

The `JS.ImportDynamic` function can be used to import a module dynamically at runtime, which is useful for code-splitting or loading modules conditionally.
```fsharp
[<Inline>]
let importTestJsDyn : obj = JS.ImportDynamic("testExport", "./test.js")
// translates to: import("./test.js")
```

The relative paths are automatically changed by WebSharper when necessary for multi-project solutions, which unpacks each project into its own folder.

`JS.ImportFile` function adds a side-effecting import, that can be used for css and other non-code resources.

## Importing npm packages

To use npm, instead of a relative path, you can use a package name. 

```fsharp
[<Import ("sqrt", "mathjs")>]
let sqrt (x: float) = X<float> 
// translates to: import sqrt from "mathjs"
```

To mark your project as an npm package consumer, you can add the following to your project file:

```xml
  <PropertyGroup>
    <NpmDependencies>
      <NpmPackage Name="mathjs" Version="gt= 14.5.2 lt 15.0.0" ResolutionStrategy="Max" />
    </NpmDependencies>
  </PropertyGroup>
```

Then when the library is consumed by a web project, the npm package can be installed automatically by the `femto` tool. Run once:

```cmd
dotnet tool install femto --global
```

Then run `femto` in your project folder to install the dependencies into `package.json` file. For C# projects, you need to provide project name like `femto MyProject.csproj`

## Linking to a script

For non-modular JavaScript files or css, you can create a script/link in your HTML pages automatically by using the `Require` attribute on a type or method.
It expects a type argument that implements the `WebSharper.Core.Resources.IResource` interface, which provides full control over what to write into the HTML head section. For example:

```fsharp
type CustomResource() =
    interface Resources.IResource with
        member this.Render ctx =
            fun writer ->
                let scriptWriter = writer RenderLocation.Scripts
                scriptWriter.WriteLine("<script src=\"test.js\"></script>")
```

In all cases when defining a resource, we then can annotate the code that requires it:

```fsharp
[<Require(typeof<TestResource>)>]
type MyWidget() = ...
```

For most cases, you don't need to implement a resource class from scratch but can inherit from the `WebSharper.Core.Resources.BaseResource` type which provides robust functionality to handle both embedded resources and extarnal URLs.

For an embedded resource, you can use:

```fsharp
open WebSharper.Core

type TestResource() =
    inherit Resources.BaseResource("test.js")

[<Require(typeof<TestResource>)>]
type MyWidget() = ...
```

The `Resources.BaseResource` depends on some Reflection for checking that the embedded resource exists, so make sure to define your resource class in the same project into which you embed your script.

Instead of `"test.js"`, you can also use an external URL. Or you can pass it a list of arguments, the first being a base URL, and the rest scripts to be loaded one after another below all using the same base URL.

For external links, there is also a shortcut without defining a class: the `Require` attribute can take additional arguments too, and WebSharper will use those to use the matching constructor on the given resource type with those arguments.
This allows to use `BaseResource` itself with a `Require`:

```
[<Require(typeof<Resources.BaseResource>, "https://example.com/myscript.js")>]
type MyWidget() = ...
```

But even in this case, defining separate resource types is useful when you want to reuse them or define dependencies between resources.
You can put a `Require` attribute on a Resource type to make it depend on another resource type, and WebSharper will ensure that the dependencies are rendered in the correct order.

### In Server-side WebSharper code

Sometimes you might need to depend on a resource without having any client side
code; typically, a CSS file. In this case, you can add the web control
`WebSharper.Web.Require` anywhere in your page. This control does not directly
output any HTML at the location where you put it, but it incurs a dependency on
the resource that you pass to it.

Here is an example UI page with a dependency on `TestResource` from above:

```fsharp
let MyPage() =
    Content.Page(
        div [
            h1 [text "This page includes the script at `test.js`."]
            Doc.WebControl(new Web.Require<TestResource>())
        ]
    )
```

### Hashes on resource links

WebSharper computes a hash of all generated files, and files embedded and referred to from a `WebResource` attribute. These hashes are added as query parameters on script links generated by the sitelets runtime to avoid browsers caching an outdated version.
 
## Overriding Resource URLs

External resources implemented using `BaseResource` can have their URL
overridden on a per-application basis. For example, you can force your
application to use a different version of JQuery than the one used by default
by WebSharper.

This configuration is done in the application configuration file `appsettings.json`, under the `websharper` section.

To override a given resource URL, simply add an appSetting whose key is the
fully qualified name of the resource, and whose value is the URL you want to
use. For example, to tell WebSharper to use a local copy of JQuery located at
the root of your application, you can add the following to your application
configuration file:

In `appsettings.json`:
```json
    "WebSharper.JQuery.Resources.JQuery": "https://code.jquery.com/jquery-3.2.1.min.js"
```

Note that the fully qualified name is in IL format. This means that nested
types and types located inside F# modules are separated by `+` instead of `.`.

If you are a library author and have a resource declared by directly
implementing the `IResource` interface, you can make it configurable by using
the function `ctx.GetSetting`, which retrieves the setting with a given key
from the application configuration file.

### Downloading resources

The `BaseResource` class also supports a toggle to download external URL scripts at compile-time, to serve from local server.
To do this, first set the `"downloadResources"` setting in [wsconfig.json](wsconfig) to `true`.
Also add this runtime setting to `appsettings.json` so that WebSharper runtime will insert the links to downloaded files in your pages instead of online resources:

```json
  "websharper": {
    "UseDownloadedResources": true
  }
```