---
order: -5
label: Hello world!
---
# Hello world!

Here is a simple [sitelet](/sitelets/README.md) that returns "Hello world!":

{% tabs %}
{% tab title="Main.fs" %}

```fsharp
module Site

open WebSharper
open WebSharper.Sitelets

[<Website>]
let Main = Application.Text (fun ctx -> "Hello World!")
```

{% endtab %}
{% endtabs %}

[![Hello world!](http://i.imgur.com/fZgqeKjm.png)](http://i.imgur.com/fZgqeKjl.png)

This sitelet has only one entry point: the root of the web application (at `/`, and the port is configurable). Requests to any other URL yield a 404 error. Given the root as a fixed, single endpoint and the textual response type, `Application.Text` is quick and easy way to create a simple, highly specialized sitelet when you need to respond with text.

---

## Steps

1. Install the [WebSharper project templates](/about/templates.md) and create a new WebSharper minimal application:

   ```text
   dotnet new websharper-min -lang f# -n MyApp
   ```

   OR

   Follow the steps in [Creating a new WebSharper project from scratch](/about/from-scratch.md). Subsequent examples will assume you have installed the WebSharper project templates, instead of going from an empty project.

2. Run it:

    ```text
    cd MyApp
    dotnet run
    ```

---

## Going further

---

### Other server responses

You can also construct a multitude of server responses, such as HTML (with or without client-side functionality), JSON data, or serving files; see sitelet [responses](/sitelets/responses.md) for more information.

---

### Switching to larger sitelets

Unless you are planning on returning text only, `Application.Text` is of limited use. You will find `Application.SinglePage` (see the [Single-page application example](/quick/spa.md)) and `Application.MultiPage` (an alias for `Sitelet.Infer`, see the [Full-stack application example](/quick/fullstack.md)) more useful.

{% tabs %}
{% tab title="Main.fs" %}

```fsharp !#6-8
module Site

open WebSharper
open WebSharper.Sitelets

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/about">] About

[<Website>]
let Main = Application.MultiPage (fun ctx -> function
    | Home ->
        Content.Page(
            [
                h1 [] [text "Hello world!"]
            ]
        )
    | About -> ...
)
```

{% endtab %}
{% endtabs %}
