<!-- ID:83535 -->

[Sitelets](//developers.websharper.com/docs/sitelets) enable you to create content on the server side by turning incoming HTTP requests to responses (JSON, HTML, etc.) Essentially, they are .NET values that can be combined to create simple microservices, web services, or entire client-server web applications. While sitelets normally require a server side to operate on top of, they can also be used to generate full HTML applications as well (so called "offline sitelets").

You can read about [constructing various types of content](https://forums.websharper.com/topic/83541), and combine those with the examples given here:

 * "Hello World"
 * Single-Page Applications
 * Multi-Page Applications
 * Web Services (TBA)


---

<!-- ID:83537 -->

**Hello World**

The simplest sitelet serves plain text (note `Application.Text`) at the root of the application:

```fsharp
module YourApp

open WebSharper
open WebSharper.Sitelets

[<Website>]
let Main = Application.Text (fun ctx -> "Hello World!")
```
![Output](https://camo.githubusercontent.com/fc38ae1cca18700e5fd0808f40ba18c714428cc7/687474703a2f2f692e696d6775722e636f6d2f665a6771654b6a6d2e706e67)

---

<!-- ID:83538 -->

**Single-Page Applications**

The easiest way to return HTML content is via `Content.Page` ([reference](http://developers.websharper.com/api/WebSharper.Sitelets.Content)). Here we use an SPA (note `Application.SinglePage`) to serve from:

```fsharp
module YourApp

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI.Next.Html
open WebSharper.UI.Next.Server

[<Website>]
let Main =
    Application.SinglePage (fun ctx ->
        Content.Page(
            h1 [text "Hello World!"]
        )
    )
```

![Output](http://i.imgur.com/xYITvCql.png)

---

<!-- ID:83540 -->

**Multi-Page Applications**

Multi-Page Applications (note `Application.MultiPage`) have multiple endpoints: pairs of HTTP verbs and paths, and are represented as an annotated union type we typically call `EndPoint`. These endpoints can be given various annotations that determine how they respond to requests. Links to endpoints in your application can be calculated from the serving context (note `ctx.Link`), so you will never have invalid URLs.

```fsharp
module YourApp

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI.Next
open WebSharper.UI.Next.Html
open WebSharper.UI.Next.Server

type EndPoint =
    | [<EndPoint "GET /">] Home
    | [<EndPoint "GET /about">] About

[<Website>]
let Main =
    Application.MultiPage (fun (ctx: Context<EndPoint>) endpoint ->
        let (=>) label endpoint = aAttr [attr.href (ctx.Link endpoint)] [text label]
        match endpoint with
        | EndPoint.Home ->
            Content.Page(
                Body = [
                    h1 [text "Hello world!"]
                    "About" => EndPoint.About
                ]
            )
        | EndPoint.About ->
            Content.Page(
                Body = [
                    p [text "This is a simple app"]
                    "Home" => EndPoint.Home
                ]
            )
    )
```

![Output](http://i.imgur.com/WMnmzIPl.png)