---
label: Single-page applications
order: -10
expanded: true
---

# Single-page applications

Single-page applications (SPAs), as their name suggests, are contained in a single HTML page. Most typically, this single HTML page will be your own `index.html` file. SPAs can be fully client-side, or more often, call server-side functions for fetching and saving data, and other chores.

Below is an example of using `index.html` with an `id="main"` placeholder, and injecting a client-side recommended book listing "widget" into it, one that receives book titles via an initial RPC call. Although you could use [F# code for HTML](/ui/html), the book listing itself is defined as an [HTML template](/ui/templating) named `ShowMeBooks` inside `index.html`, with further nested templates for the individual books. This has the added benefit that changes to either template can be dynamically loaded and do not require recompilation (and thus, are nearly instantly applied.)

!!!contrast Reduce or eliminate HTML from your F# code
Using [WebSharper.UI templating](/ui/templating.md), your code will only compile if it uses the template correctly. This type safety is paramount and **greatly enhances productivity**, and coupled with the ability to load template changes dynamically (which is also available in server-side use, via an additional argument; for instance, `ServerLoad.WhenChanged`) it **supercharges and streamlines your developer experience**. Practically speaking, once you have an initial HTML skeleton design, you can fully write your application logic against it, compile it with static type-safety guarantees, and then take iterations with sub-second processing time to enhance/perfect the presentation layer.
!!!

{% tabs %}
{% tab title="Client.fs" %}

```fsharp #19,32-40
namespace SPA

open WebSharper
open WebSharper.UI.Client
open WebSharper.UI.Templating

module Server =
    [<Rpc>]
    let FetchAllBooks() =
        async {
            let book = "Expert F#"
            return [book; book+" 2.0"; book+" 3.0"; book+" 4.0"]
        }

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type Templates = Template<"wwwroot/index.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>

    module Render =
        let Book (title: string) =
            Templates.Book()
                .Title(title)
                .Order(fun e -> JavaScript.JS.Alert $"Adding {title}...")
                .Doc()
        let Books (books: UI.Doc list) =
            Templates.ListOfBooks().Books(books).Doc()

    [<SPAEntryPoint>]
    let Main () =
        Templates.ShowMeBooks()
            .Name("world")
            .Books([
                async {
                    let! books = Server.FetchAllBooks()
                    return Render.Books (List.map Render.Book books)
                } |> Doc.Async
            ])
            .Doc()
        |> Doc.RunById "main"
```

{% endtab %}

{% tab title="wwwroot/index.html" %}

```html #15-25
<!DOCTYPE html>
<html lang="en">
<head>
    <title>SPA</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link rel="stylesheet" type="text/css" href="Content/SPA.css" />
    <style>
        /* Don't show the not-yet-loaded templates */
        [ws-template], [ws-children-template] { display: none; }
    </style>
    <script type="text/javascript" src="Content/SPA.head.js"></script>
</head>
<body>
    <div id="main" ws-children-template="ShowMeBooks">
        <h1>Hello ${Name}!</h1>
        <p>Here are some book recommendations for you:</p>
        <div ws-hole="Books">
            <ul ws-template="ListOfBooks" ws-hole="Books">
                <li ws-template="Book">
                    <button ws-onclick="Order">Add to cart</button>&nbsp;${Title}
                </li>
            </ul>
        </div>
    </div>
    <script type="text/javascript" src="Content/SPA.min.js"></script>
</body>
</html>
```

{% endtab %}
{% endtabs %}

---

## Steps

1. Create a new WebSharper SPA:

    ```text
    dotnet new websharper-spa -lang f# -name SPA
    ```

2. Replace `Client.fs` and `wwwroot/index.html` with the code above.

3. Run it:

    ```text
    cd SPA
    dotnet run
    ```

4. See it in action:

   ![](spa-running.png)
---

## Going further

---

### Adding a data model

The SPA above doesn't have a client-side data model, for instance, it doesn't keep track of the books added to the cart, nor does it have the capability to refresh the list of recommended books. To add these more realistic features, you can use [WebSharper.MVU] for a variant of the Model-View-Update (MVU) pattern along with [lenses](/ui/lenses), WebSharper.UI's [composite data models](/ui/listmodels) (as shown in the `websharper-spa` project template), or simple [reactive variables](/ui/vars) and their [views](/ui/views).

---

### Serving your SPA from a sitelet endpoint

You can also serve/return an SPA from a [sitelet](/sitelets) endpoint. To convert the above SPA into a sitelet response, you need to make a few minor adjustments to it (removing the `Doc.RunById` call from `Client.Main`, adding a `ws-hole="Main"` attribute at the end of your `<div id="main" ..>` node, and replacing the hardwired `SPA.min.js` include with `<div ws-replace="scripts"></div>` to let the sitelet runtime manage your page dependencies). You can then expose the adjusted SPA code from a sitelet:

```fsharp #13
open WebSharper
open WebSharper.UI
open WebSharper.UI.Server
open WebSharper.UI.Templating
open WebSharper.Sitelets

type IndexPage = Template<"wwwroot/index.html", serverLoad=ServerLoad.WhenChanged>

[<Website>]
let Main = Application.SinglePage (fun ctx ->
    Content.Page(
        IndexPage()
            .Main(Web.Control (ClientSide <@ Client.Main () @>))
            .Doc()
    )
)
```

You can also use `Application.MultiPage` (aka `Sitelet.Infer`) and a discriminated union (DU) endpoint type depending on your [server-side routing](/sitelets/routing/) needs.

---

### Client-side vs. server-side routing

And last, true SPAs often model multiple "logical" pages inside the single HTML page they are contained in. See [WebSharper.MVU] for basic support for partitioning your SPA into such pages, or simply use [client-side routing](/ui/routing) and a discriminated union endpoint type to represent them. You can also find more valuable insights in the F# Advent 2017 blog article titled ["Serving SPAs"](https://intellifactory.com/user/granicz/20171229-serving-spas), especially around splitting your endpoint type into a client-side and server-side part, giving you fine control over where page requests get handled, essentially, transitioning you into full-fledge full-stack application development.

[WebSharper.MVU]: https://github.com/dotnet-websharper/mvu