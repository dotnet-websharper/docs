---
order: -10
---

WebSharper applications are ASP.NET Core applications, and to develop and build them you pretty much only need a .NET SDK. However, we also recommend you install the WebSharper CLI, your favorite IDE with F\# support, and the Azure SDK and CLI:

---

### .NET SDK

* .**NET 6.0.100 SDK** or higher, available from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download). The SDK
  contains the `dotnet` CLI tool that you can use to install WebSharper project templates, compile and run projects, and prepare deployment packages.

  !!! What .NET can I target?
  * WebSharper 7 (upcoming) will target `net70`.

  * WebSharper 5/6 applications target `net50`/`net60`, respectively.

  * If you need to target .NET Framework 4.x \(`net472`, etc.\), you must use WebSharper 4.x.
  !!!

---

### WebSharper CLI

The WebSharper CLI is a [dotnet tool](https://github.com/dotnet-websharper/dotnet-ws) that helps streamline building applications and manage compiler runtimes.

You can install it using `dotnet`:

```text
dotnet tool install -g dotnet-ws
```

---

### IDE

You need an IDE to work with WebSharper projects effectively. We recommend one of the following:

* **Visual Studio Community** or above, available from [https://visualstudio.microsoft.com](https://visualstudio.microsoft.com).

* **Visual Studio Code**, available from [https://code.visualstudio.com](https://code.visualstudio.com/). For F\# support, you can use:

  * the [Ionide](https://ionide.io/) plugin, installed through the Extensions tab and searching for "Ionide", or via a direct download from [https://marketplace.visualstudio.com/items?itemName=Ionide.Ionide-fsharp](https://marketplace.visualstudio.com/items?itemName=Ionide.Ionide-fsharp).

* You can also use [Rider](https://www.jetbrains.com/rider/), [emacs](https://www.gnu.org/software/emacs/) or vi with F# support, or any other IDE of your choice, but you need to make sure they are properly configured for compiling, running and debugging F#/WebSharper projects.

---

### Cloud tools

* **Azure SDK** - to work with the `az` CLI tool to deploy your WebSharper applications to Azure.
