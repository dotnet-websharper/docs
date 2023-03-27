---
order: -30
label: Project templates
---

# .NET project templates for WebSharper

!!! Note
You may also want to configure the WebSharper GitHub feed, containing experimental tools and developer builds. To do so, make sure you [have this extra feed configured](/basics/nuget.md/#configuring-the-websharper-developer-feed).
!!!

You can install the latest WebSharper project templates via the `dotnet` CLI - this will also make them available in Visual Studio 2022 and above. As of WebSharper 6, no separate VSIX installer is available, so use the following command to install:

```text
> dotnet new -i WebSharper.Templates
```

!!!
You can also install any given version by adding the version as a suffix, for instance `::6.0.0.140`.
!!!

Different WebSharper project templates are available for F\# and C\#. You can set the language of your preference in your shell with:

```text
> set DOTNET_NEW_PREFERRED_LANG=F#
```

To see what project templates are installed on your system and what's the default language for each, you can use:

```text
> dotnet new --list
...
WebSharper 6 Minimal Application                  websharper-min           [F#], C#          WebSharper/Web
WebSharper 6 Single-Page Application              websharper-spa           [F#], C#          WebSharper/Web
WebSharper 6 Client-Server Application            websharper-web           [F#], C#          WebSharper/Web
WebSharper 6 HTML Application                     websharper-html          [F#], C#          WebSharper/Web
WebSharper 6 JavaScript Bindings                  websharper-ext           [F#]              WebSharper
WebSharper 6 .NET Proxy                           websharper-prx           [F#], C#          WebSharper
WebSharper 6 Library                              websharper-lib           [F#], C#          WebSharper
...
```

---

## How to decide what template to use?

Here are some guidelines on choosing the right project for your app:

---

### Does your application have a server-side?

1) Yes -> use the client-server application template (`websharper-web`) for microservices or full-stack apps. If you have a single page with possibly multiple sub-pages, use the single-page application template (`websharper-spa`).

2) No, I need to build a static web app and host it on GitHub Pages -> use the HTML application (`websharper-html`), or the single-page application template (`websharper-spa`) without a server-side.

---

### What's the difference between an HTML application and an SPA?

An HTML application can contain multiple pages and has no server-side (RPCs or websockets, but it can still make requests to external services). An SPA has a single containing page with possibly multiple sub-pages inside it, and it may have an optional server-side.

---

### Can I have multiple containing pages in an SPA?

Yes, but each page needs to be a separate SPA project and you need to manually assemble them into a single app by copying all master pages and their generated JavaScript code and other artifacts.

---

### I want to use our legacy JavaScript codebase and other, third-party JavaScript libraries in my application.

Create a bindings for them using the JavaScript binding template (`websharper-ext`). You may also want to look around on NuGet or GitHub to see if a binding already exists.

---

### I have .NET code that fails to compile with WebSharper.

Create a .NET proxy project (`websharper-prx`) to describe how to map your .NET code to JavaScript.

---

### I want to package up some of my UI and/or client-side logic to be shared with other WebSharper applications.

Create a WebSharper library (`websharper-lib`).

---

## Quick overview of the available templates

### Minimal Application

`websharper-min` creates a minimal server-side application that you can expand into a full-stack web application. It contains the boilerplate to create a sitelet that responds with a simple text response. Check the [Hello world!](/examples/hello.md) example for more information.

---

### Single-Page Application

`websharper-spa` creates a single-page application (SPA) with a master `index.html` file, into which dynamic client-side functionality is injected. You can expand this template with an optional server-side by adding RPCs to it.

---

### Client-Server Application

`websharper-web` creates a full-stack client-server application. Use this as a base for your full-stack applications.


---

!!! Sorry...
The rest of the page is not yet written, please check back soon!
!!!
