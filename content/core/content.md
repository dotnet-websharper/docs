---
title: Serving content with sitelets
---

Content describes the response to send back to the client: its HTTP status, headers and body. Content is always worked with asynchronously: all the constructors and combinators described below take and return values of type `Async<Content<'EndPoint>>`. You will find various functions that create different types of content: ordinary text (`Content.Text`), file content (`Content.File`), HTML (`Content.Page`), HTML based on templates (`Content.WithTemplate`), JSON (`Content.Json`), custom content (`Content.Custom`), and HTTP error codes and redirects.

### Content.Text

The simplest response is plain text content, created by passing a string to `Content.Text`.

```fsharp
let simpleResponse =
    Content.Text "This is the response body."
```

### Content.File

You can serve files using `Content.File`.  Optionally, you can set the content type returned for the file response and whether file access is allowed outside of the web root:

```fsharp
type EndPoint = //. . .

let fileResponse: Async<Content<EndPoint>> =
    Content.File("../Main.fs", AllowOutsideRootFolder=true, ContentType="text/plain")
```

### Content.Page

You can return full HTML pages, with managed dependencies using `Content.Page`. Here is a simple example:

```fsharp
open WebSharper.UI.Html
    
let IndexPage : Async<Content<EndPoint>> =
    Content.Page(
        Title = "Welcome!",
        Head = [ link [attr.href "/css/style.css"; attr.rel "stylesheet"] [] ],
        Body = [
            h1 [] [text "Welcome to my site."] 
        ]
    )
```

The optional named arguments `Title`, `Head`, `Body` and `Doctype` set the corresponding elements of the HTML page.
These contents use the `WebSharper.Web.INode` type which provides a `Write : Web.Context * HtmlTextWriter -> unit` method to render the HTML content to the response body.
For easy construction of HTML, you can use `WebSharper.UI.Html` module, which provides a number of functions for creating HTML elements and text nodes.

The `Bundle` argument can be used to specify a bundle name for production-ready mode, that will be used for creating a JavaScript bundle for the page.
Any `Web.Control` instantiations or quoted code (for example with the `client` helper) that is defined within the `Content.Page` expression will be automatically included in the bundle.

### Content.PageFromFile

An implementation of a simple form of templating, `PageFromFile` allows you to use a static HTML file as a template for your page and insert some startup code.

```fsharp
let IndexPage : Async<Content<EndPoint>> =
    Content.PageFromFile(
        "index.html",
        fun () -> InitializeHomePage() // This function is called when the page is loaded 
    )
```

The startup script will be inserted where the template has `<script ws-replace="scripts"></script>`

<a name="json-response"></a>
### Content.Json

If you are creating a web API, then sitelets can automatically generate JSON content for you based on the type of your data. Simply pass your value to `Content.Json`, and WebSharper will serialize it. The format is the same as when parsing requests. [See here for more information about the JSON format.](json)

```fsharp
type BlogArticleResponse =
    {
        id: int
        slug: string
        title: string
    }

let content id =
    Content.Json
        {
            id = id
            slug = "some-blog-article"
            title = "Some blog article!"
        }

type EndPoint =
    | GetBlogArticle of id: int

let sitelet = Sitelet.Infer <| fun context endpoint ->
    match endpoint with
    | GetBlogArticle id -> content id

// Accepted Request:    GET /GetBlogArticle/1423
// Parsed Endpoint:     GetBlogArticle 1423
// Returned Content:    {"id": 1423, "slug": "some-blog-article", "title": "Some blog article!"}
```

### Content.MvcResult

Marks an object to be handled as if from and MVC controller. This is useful if you want to use the MVC framework for rendering pages, but still want to use sitelets for routing.

### Content.Cors

Used to enable CORS (Cross-Origin Resource Sharing) checks for a sitelet.
Wrap the endpoint with `Sitelet.Cors` type, and then the `Content.Cors` function can be used to specify the response, including setting allowed origins, headers, and methods.

```fsharp
       
type ApiEndPoint =
    | [<EndPoint "GET /item">] GetItem of int
    | [<EndPoint "POST /item"; Json "data">] PostItem of data: Data
    | [<EndPoint "PUT /item"; Json "data">] PutItem of data: Data
    
type MainEndPoint =
    | [<EndPoint "GET /">] Home
    | [<EndPoint "/">] Api of Cors<ApiEndPoint>
    
let Website = Application.MultiPage (fun ctx endpoint ->
    match endpoint with
    | Home -> // ...
    | Api cors ->
        Content.Cors cors
        <| fun allows ->
            { allows with
                Origins = ["*"]
                Headers = ["Content-Type"; "Authorization"] }
        <| function
            | GetItem it -> // ...
            | PostItem it -> // ...
            | PutItem it -> // ...
)
```

### Content.Bundle
    
A helper for marking client-side code to be included in a specific bundle.
This is useful when you want to create helper functions on the server that creates content that has client-side dependencies, but you do it outside of the `Content.Page` scope.
It takes a `WebSharper.Web.INode` sequence and a bundle name, and it will return a `WebSharper.Web.INode` sequence that also marks for the server-side rendering which bundle to use.
This means you can drop the `Bundle ` argument in `Content.Page`.

### Content.BundleScope

A general helper for marking client-side code to be included in a specific bundle that works for any type.
Also `BundleScopes` accepts a list of bundle names, code in scope will go to all bundles specified.
However, for a page to find which bundle to use, you will still need to use the `Bundle ` argument in `Content.Page`.

### Content.Custom

`Content.Custom` can be used to output any type of content. It takes three optional named arguments that corresponds to the aforementioned elements of the response:

* `Status` is the HTTP status code. It can be created using the function `Http.Status.Custom`, or you can use one of the predefined statuses such as `Http.Status.Forbidden`.

* `Headers` is the HTTP headers. You can create them using the function `Http.Header.Custom`.

* `WriteBody` writes the response body.

```fsharp
let content =
    Content.Custom(
        Status = Http.Status.Ok,
        Headers = [Http.Header.Custom "Content-Type" "text/plain"],
        WriteBody = fun stream ->
            use w = new System.IO.StreamWriter(stream)
            w.Write("The contents of the text file.")
    )

type EndPoint =
    | GetSomeTextFile

let sitelet = Sitelet.Content "/someTextFile.txt" GetSomeTextFile content

// Accepted Request:    GET /someTextFile.txt
// Parsed Endpoint:     GetSomeTextFile
// Returned Content:    The contents of the text file.
```

### Helpers

In addition to the four standard Content families above, the `Content` module contains a few helper functions.

* Redirection:

    ```fsharp
    module Content =
        /// Permanently redirect to an endpoint. (HTTP status code 301)
        val RedirectPermanent : 'EndPoint -> Async<Content<'EndPoint>>
        /// Permanently redirect to a URL. (HTTP status code 301)
        val RedirectPermanentToUrl : string -> Async<Content<'EndPoint>>
        /// Temporarily redirect to an endpoint. (HTTP status code 307)
        val RedirectTemporary : 'EndPoint -> Async<Content<'EndPoint>>
        /// Temporarily redirect to a URL. (HTTP status code 307)
        val RedirectTemporaryToUrl : string -> Async<Content<'EndPoint>>
    ```

* Response mapping: if you want to return HTML or JSON content, but further customize the HTTP response, then you can use one of the following:

    ```fsharp
    module Content =
        /// Set the HTTP status of a response.
        val SetStatus : Http.Status -> Async<Content<'T>> -> Async<Content<'T>>
        /// Add headers to a response.
        val WithHeaders : seq<Header> -> Async<Content<'T>> -> Async<Content<'T>>
        /// Add a single header by name and value to a response.
        val WithHeader : string -> string -> Async<Content<'T>> -> Async<Content<'T>>
        /// Add a Content-Type header to a response.
        val WithContentType : string -> Async<Content<'T>> -> Async<Content<'T>>
        /// Replace the headers of a response.
        val SetHeaders : seq<Header> -> Async<Content<'T>> -> Async<Content<'T>>

    // Example use
    let customForbidden =
        Content.Page(
            Title = "No entrance!",
            Body = [text "Oops! You're not supposed to be here."]
        )
        // Set the HTTP status code to 403 Forbidden:
        |> Content.SetStatus Http.Status.Forbidden
        // Add an HTTP header:
        |> Content.WithHeaders [Http.Header.Custom "Content-Language" "en"]
    ```

### Status codes

* `Content.Ok` returns a 200 OK response.
* `Content.Unauthorized` returns a 401 Unauthorized response.
* `Content.Forbidden` returns a 403 Forbidden response.
* `Content.NotFound` returns a 404 Not Found response.
* `Content.MethodNotAllowed` returns a 405 Method Not Allowed response.
* `Content.ServerError` returns a 500 Internal Server Error response.
* `Content.NotImplemented` returns a 501 Not Implemented response.

<a name="context"></a>
## Using the Context

The functions to create sitelets from content, namely `Sitelet.Infer` and `Sitelet.Content`, provide a context of type `Context<'T>`. This context can be used for several purposes; the most important are creating internal links and managing user sessions.

<a name="linking"></a>
### Creating links

Since every accepted URL is uniquely mapped to a strongly typed endpoint value, it is also possible to generate internal links from an endpoint value. For this, you can use the method `context.Link`.

```fsharp
open WebSharper.UI.Html

type EndPoint = | BlogArticle of id:int * slug:string

let HomePage (context: Context<EndPoint>) =
    Content.Page(
        Title = "Welcome!",
        Body = [
            h1 [] [text "Index page"]
            a [attr.href (context.Link (BlogArticle(1423, "some-article-slug")))] [
                text "Go to some article"
            ]
            br [] []
            a [attr.href (context.ResolveUrl "~/Page2.html")] [
                text "Go to page 2"
            ]
        ]
    )
```

Note how `context.Link` is used in order to resolve the URL to the `BlogArticle` endpoint.  Endpoint URLs are always constructed relative to the application root, whether the application is deployed as a standalone website or in a virtual folder.  `context.ResolveUrl` helps to manually construct application-relative URLs to resources that do not map to endpoints.

### Managing User Sessions

`Context<'T>` can be used to access the currently logged in user. The member `UserSession` has the following members:

* `LoginUser : username: string * ?persistent: bool -> Async<unit>`  
  `LoginUser : username: string * duration: System.TimeSpan -> Async<unit>`

    Logs in the user with the given username. This sets a cookie that is uniquely associated with this username. The second parameter determines the expiration of the login:

    * `LoginUser("username")` creates a cookie that expires with the user's browser session.
    
    * `LoginUser("username", persistent = true)` creates a cookie that lasts indefinitely.
    
    * `LoginUser("username", duration = d)` creates a cookie that expires after the given duration.
    
    Example:
    
    ```fsharp
    let LoggedInPage (context: Context<EndPoint>) (username: string) =
        async {
            // We're assuming here that the login is successful,
            // eg you have verified a password against a database.
            do! context.UserSession.LoginUser(username, 
                    duration = System.TimeSpan.FromDays(30.))
            return! Content.Page(
                Title = "Welcome!",
                Body = [ text (sprintf "Welcome, %s!" username) ]
            )
        }
    ```

* `GetLoggedInUser : unit -> Async<string option>`

    Retrieves the currently logged in user's username, or `None` if the user is not logged in.
    
    Example:
    
    ```fsharp
    let HomePage (context: Context<EndPoint>) =
        async {
            let! username = context.UserSession.GetLoggedInUser()
            return! Content.Page(
                Title = "Welcome!",
                Body = [
                    text (
                        match username with
                        | None -> "Welcome, stranger!"
                        | Some u -> sprintf "Welcome back, %s!" u
                    )
                ]
            )
        }
    ```

* `Logout : unit -> unit`

    Logs the user out.
    
    Example:
    
    ```fsharp
    let Logout (context: Context<EndPoint>) =
        async {
            do! context.UserSession.Logout()
            return! Content.RedirectTemporary Home
        }
    ```

The implementation of these functions relies on cookies and thus requires that the browser has enabled cookies.

### Other Context members

`WebSharper.Sitelets.Context<'T>` inherits from `WebSharper.Web.Context`, and a number of properties and methods from it are useful. [See the documentation for `WebSharper.Web.Context`](webcontext).