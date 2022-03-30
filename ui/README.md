---
order: -52
expanded: true
label: WebSharper.UI
icon: rocket
---

# WebSharper UI

==- Where is it?

* NuGet package: `WebSharper.UI`
* DLL: `WebSharper.UI.dll`
* Namespace: `WebSharper.UI`

===

WebSharper.UI is a library providing a novel, pragmatic and convenient approach to UI reactivity. It includes:

* An [HTML library](#html) usable both from the server side and from the client side, which you can use to build HTML pages either by calling F# functions to create elements, or by instantiating template HTML files.
* A [reactive layer](#reactive) for expressing user inputs and values computed from them as time-varying values. This approach is related to Functional Reactive Programming (FRP). This reactive system integrates with the HTML library to create reactive documents. If you are familiar with Facebook React, then you will find some similarities with this approach: instead of explicitly inserting, modifying and removing DOM nodes, you return a value that represents a DOM tree based on inputs. The main difference is that these inputs are nodes of the reactive system, rather than a single state value associated with the component.
* Client-side [routing](#routing) using the same endpoint type declaration as [WebSharper server-side routing](sitelets.md).

This page is an overview of the capabilities of WebSharper.UI. You can also check [the full reference of all the API types and modules](http://developers.websharper.com/api/WebSharper.UI).

Get the package from NuGet: [WebSharper.UI](https://www.nuget.org/packages/websharper.ui).

### HTML on the client

To insert a Doc into the document on the client side, use the `Doc.Run*` family of functions from the module [`WebSharper.UI.Client`](/api/v4.1/WebSharper.UI.Client). Each of these functions has two variants: one directly taking a DOM [`Element`](/api/v4.1/WebSharper.JavaScript.Dom.Element) or [`Node`](/api/v4.1/WebSharper.JavaScript.Dom.Node), and the other suffixed with `ById` taking the id of an element as a string.

* [`Doc.Run`](/api/v4.1/WebSharper.UI.Doc#Run) and [`Doc.RunById`](/api/v4.1/WebSharper.UI.Doc#RunById) insert a given Doc as the child(ren) of a given DOM element. Note that it replaces the existing children, if any.

    ```fsharp
    open WebSharper.JavaScript
    open WebSharper.UI
    open WebSharper.UI.Client
    open WebSharper.UI.Html

    let Main () =
        div [] [ text "This goes into #main." ]
        |> Doc.RunById "main"
        
        p [] [ text "This goes into the first paragraph with class my-content." ]
        |> Doc.Run (JS.Document.QuerySelector "p.my-content")
    ```

* [`Doc.RunAppend`](/api/v4.1/WebSharper.UI.Doc#RunAppend) and [`Doc.RunAppendById`](/api/v4.1/WebSharper.UI.Doc#RunAppendById) insert a given Doc as the last child(ren) of a given DOM element.

* [`Doc.RunPrepend`](/api/v4.1/WebSharper.UI.Doc#RunPrepend) and [`Doc.RunPrependById`](/api/v4.1/WebSharper.UI.Doc#RunPrependById) insert a given Doc as the first child(ren) of a given DOM element.

* [`Doc.RunAfter`](/api/v4.1/WebSharper.UI.Doc#RunAfter) and [`Doc.RunAfterById`](/api/v4.1/WebSharper.UI.Doc#RunAfterById) insert a given Doc as the next sibling(s) of a given DOM node.

* [`Doc.RunBefore`](/api/v4.1/WebSharper.UI.Doc#RunBefore) and [`Doc.RunBeforeById`](/api/v4.1/WebSharper.UI.Doc#RunBeforeById) insert a given Doc as the previous sibling(s) of a given DOM node.

* [`Doc.RunReplace`](/api/v4.1/WebSharper.UI.Doc#RunReplace) and [`Doc.RunReplaceById`](/api/v4.1/WebSharper.UI.Doc#RunReplaceById) insert a given Doc replacing a given DOM node.

### HTML on the server

On the server side, using [sitelets](sitelets.md), you can create HTML pages from Docs by passing them to the `Body` or `Head` arguments of `Content.Page`.

```fsharp
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Html

let MyPage (ctx: Context<EndPoint>) =
    Content.Page(
        Title = "Welcome!",
        Body = [
            h1 [] [ text "Welcome!" ]
            p [] [ text "This is my home page." ]
        ]
    )
```

By opening `WebSharper.UI.Server`, you can also just pass a full page to `Content.Page`. This is particularly useful together with [templates](#templating).

```fsharp
let MyPage (ctx: Context<EndPoint>) =
    Content.Page(
        html [] [
            head [] [ title [] [ text "Welcome!" ] ]
            body [] [
                h1 [] [ text "Welcome!" ]
                p [] [ text "This is my home page." ]
            ]
        ]
    )
```

To include client-side elements inside a page, use the `client` method, from inside `WebSharper.UI.Html`.

```fsharp
[<JavaScript>]
module Client =

    let MyControl() =
        button [ on.click (fun el ev -> JS.Alert "Hi!") ] [ text "Click me!" ]

module Server =

    let MyPage (ctx: Context<EndPoint>) =
        Content.Page(
            Title = "Welcome!",
            Body = [
                h1 [] [ text "Welcome!" ]
                p [] [ client <@ Client.MyControl() @> ]
            ]
        )
```

# Client-side, reactive programming

WebSharper.UI's reactive layer helps represent user inputs and other time-varying values, and define how they depend on one another.

Vars/Views/etc.

---
