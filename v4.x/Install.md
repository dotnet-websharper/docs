# Installing WebSharper for Visual Studio

Developing with WebSharper in Visual Studio currently requires:

* [Visual Studio 2015 or later][vs] with Web Developer Tools installed.
Also Visual F# Tools is needed to use WebSharper for F#.

When your environment is ready, download and install the
WebSharper `.vsix` file for one or both languages from the [WebSharper download page][downloads].
These will install the WebSharper project templates into Visual Studio
(you may have to restart Visual Studio if you have it running while
you install WebSharper), making it easy to get started with new projects.

## Visual Studio templates

Once you installed WebSharper and, if needed, restarted Visual Studio, you should see the main WebSharper templates in the New Project dialog.
A separate `WebSharper` section exists under both `Visual C#` and `Visual F#`.

![Visual Studio templates](https://raw.githubusercontent.com/dotnet-websharper/docs/master/images/VisualStudioTemplates41.png)

<a name="netcore"></a>
# Installing WebSharper Templates for .NET Core/Standard

Install [.NET Core SDK 2.0+](https://www.microsoft.com/net/download/windows) if you don't have it.
Run `dotnet new -i WebSharper.Templates`.

Use `dotnet new` to list available templates.

![.NET Core templates](https://raw.githubusercontent.com/dotnet-websharper/docs/websharper42/images/NetCoreTemplates42.png)

Instantiate templates with `dotnet new templatename`, for example `dotnet new websharper-web`.
Default language is C#, use `dotnet new templatename -lang F#` to create an F# template.

## Updating WebSharper in existing projects

When you create a new WebSharper project from a Visual Studio template,
it will use the version of WebSharper that came bundled with the 
Visual Studio installer you used.

WebSharper extensions, as well as the core WebSharper binaries, are
distributed via Nuget. This means that you can upgrade WebSharper in
or add WebSharper extensions to your existing Visual Studio projects
by using the NuGet package manager, as you would with any other Nuget
package.

The core libraries are contained in the `WebSharper` package.
To have C# compiler support, also install `WebSharper.CSharp`.
To have F# compiler support, also install `WebSharper.FSharp`.

[downloads]: http://websharper.com/downloads
[vs]: http://www.microsoft.com/visualstudio/eng/downloads

## Using NGen.exe for faster compilation on Windows

Run the script `runngen.ps1` in PowerShell with administrator permissions to call `ngen.exe` on the compiler.
It can be found in the `tools` folder of both `WebSharper.CSharp` and `WebSharper.FSharp` packages.
This creates a cached native image that can speedup compiler tool running time.