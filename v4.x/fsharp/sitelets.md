# Routing requests and serving content with Sitelets #

Sitelets are WebSharper's primary way to create server-side content. They provide facilities to route requests and generate HTML pages or JSON responses.

Sitelets allow you to:

* Dynamically construct pages and serve arbitrary content.

* Have full control of your URLs by specifying [custom routers](#advanced-sitelets) for linking them to content, or let the URLs be [automatically inferred](#sitelet-infer) from an endpoint type.

* Compose contents into sitelets, which may themselves be [composed into larger sitelets](#sitelet-combinators).

* Have [safe links](#linking) for referencing other content contained within your site.

* Use the type-safe HTML and templating facilities from [UI](ui.md) on the server side.

* Automatically [parse JSON requests and generate JSON responses](Json.md) based on your types.

Below is a minimal example of a complete site serving one HTML page:

```fsharp
namespace SampleWebsite

open WebSharper.Sitelets

module SampleSite =
    open WebSharper
    open WebSharper.UI.Html
    open WebSharper.UI.Server

    type EndPoint =
        | Index

    let IndexContent context : Async<Content<EndPoint>> =
        let time = System.DateTime.Now.ToString()
        Content.Page(
            Title = "Index",
            Body = [h1 [] [text ("Current time: " + time)]]
        )

    [<Website>]
    let MySampleWebsite : Sitelet<EndPoint> =
        Sitelet.Content "/index" EndPoint.Index IndexContent
```

First, a custom endpoint type is defined. It is used for linking requests to content within your sitelet. Here, you only need one endpoint, `EndPoint.Index`, corresponding to your only page.

The content of the index page is defined as a [`Content.Page`](/api/v4.1/WebSharper.UI.Server.Content#Page\`\`1), where the body consists of a server side HTML element.  Here the current time is computed and displayed within an `<h1>` tag.

The `MySampleWebsite` value has type [`Sitelet<EndPoint>`](/api/v4.1/WebSharper.Sitelets.Sitelet\`1). It defines a complete website: the URL scheme, the `EndPoint` value corresponding to each served URL (only one in this case), and the content to serve for each endpoint. It uses the [`Sitelet.Content`](/api/v4.1/WebSharper.Sitelets.Sitelet#Content\`\`1) operator to construct a sitelet for the Index endpoint, associating it with the `/index` URL and serving `IndexContent` as a response.

`MySampleWebsite` is annotated with the attribute [`[<Website>]`](/api/v4.1/WebSharper.Sitelets.WebsiteAttribute) to indicate that this is the sitelet that should be served.

## Routing

WebSharper Sitelets abstract away URLs and request parsing by using an endpoint type that represents the different HTTP endpoints available in a website. For example, a site's URL scheme can be represented by the following endpoint type:

```fsharp
type EndPoint =
    | Index
    | Stats of username: string
    | BlogArticle of id: int * slug: string
```

Based on this, a Sitelet is a value that represents the following mappings:

* Mapping from requests to endpoints. A Sitelet is able to parse a URL such as `/blog/1243/some-article-slug` into the endpoint value `BlogArticle (id = 1243, slug = "some-article-slug")`. More advanced definitions can even parse query parameters, JSON bodies or posted forms.

* Mapping from endpoints to URLs. This allows you to have internal links that are verified by the type system, instead of writing URLs by hand and being at the mercy of a typo or a change in the URL scheme. You can read more on this [in the "Context" section](#context).

* Mapping from endpoints to content. Once a request has been parsed, this determines what content (HTML or other) must be returned to the client.

A number of primitives are available to create and compose Sitelets.

### Trivial Sitelets

Two helpers exist for creating a Sitelet with a trivial router: only handling requests on the root.

* `Application.Text` takes just a `Context<_> -> string` function and creates a Sitelet that serves the result string as a text response.
* `Application.SinglePage`takes a `Context<_> -> Async<Content<_>>` function and creates a Sitelet that serves the returned content.

<a name="sitelet-infer"></a>
### Sitelet.Infer

The easiest way to create a more complex Sitelet is to automatically generate URLs from the shape of your endpoint type using [`Sitelet.Infer`](/api/v4.1/WebSharper.Sitelets.Sitelet#Infer\`\`1), also aliased as [`Application.MultiPage`](/api/v4.1/WebSharper.Application#MultiPage\`\`1). This function parses slash-separated path segments into the corresponding `EndPoint` value, and lets you match this endpoint and return the appropriate content. Here is an example sitelet using `Infer`:

```fsharp
namespace SampleWebsite

open WebSharper.Sitelets

module SampleSite =
    open WebSharper
    open WebSharper.UI.Html
    open WebSharper.UI.Server

    type EndPoint =
        | Index
        | Stats of username: string
        | BlogArticle of id: int * slug: string

    [<Website>]
    let MyWebsite =
        Sitelet.Infer <| fun context endpoint ->
            match endpoint with
            | Index ->
                 // Content of the index page
                 Content.Page(
                     Title = "Welcome!",
                     Body = [h1 [] [text "Index page"]])
            | Stats username ->
                 // Content of the stats page, which depends on the username
                 Content.Page(
                    Body = [text ("Stats for " + username)])
            | BlogArticle (id, slug) ->
                // Content of the article page, which depends on id and slug
                Content.Page(
                    Body = [text (sprintf "Article id %i, slug %s" id slug)])
```

The above sitelets accepts URLs with the following shape:

```xml
Accepted Request:    GET /Index
Parsed Endpoint:     Index
Returned Content:    <!DOCTYPE html>
                     <html>
                         <head><title>Welcome!</title></head>
                         <body>
                             <h1>Index page</h1>
                         </body>
                     </html>

Accepted Request:    GET /Stats/someUser
Parsed Endpoint:     Stats (username = "someUser")
Returned Content:    <!DOCTYPE html>
                     <html>
                         <head></head>
                         <body>
                             Stats for someUser
                         </body>
                     </html>

Accepted Request:    GET /BlogArticle/1423/some-article-slug
Parsed Endpoint:     BlogArticle (id = 1423, slug = "some-article-slug")
Returned Content:    <!DOCTYPE html>
                     <html>
                         <head></head>
                         <body>
                             Article id 1423, slug some-article-slug
                         </body>
                     </html>
```

The following types are accepted by `Sitelet.Infer`:

* Numbers and strings are encoded as a single path segment.

    ```fsharp
    type EndPoint = string

    // Accepted Request:    GET /abc
    // Parsed Endpoint:     "abc"
    // Returned Content:    (determined by Sitelet.Infer)

    type EndPoint = int

    // Accepted Request:    GET /1423
    // Parsed Endpoint:     1423
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* Tuples and records are encoded as consecutive path segments.

    ```fsharp
    type EndPoint = int * string

    // Accepted Request:    GET /1/abc
    // Parsed Endpoint:     (1, "abc")
    // Returned Content:    (determined by Sitelet.Infer)

    type EndPoint = { Number : int; Name : string }

    // Accepted Request:    GET /1/abc
    // Parsed Endpoint:     { Number = 1; Name = "abc" }
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* Union types are encoded as a path segment (or none or multiple) identifying the case, followed by segments for the arguments (see the example above).

    ```fsharp
    type EndPoint = string option

    // Accepted Request:    GET /Some/abc
    // Parsed Endpoint:     Some "abc"
    // Returned Content:    (determined by Sitelet.Infer)
    //
    // Accepted Request:    GET /None
    // Parsed Endpoint:     None
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* Lists and arrays are encoded as a number representing the length, followed by each element. For example:

    ```fsharp
    type EndPoint = string list

    // Accepted Request:    GET /2/abc/def
    // Parsed Endpoint:     ["abc"; "def"]
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* Enumerations are encoded as their underlying type.

    ```fsharp
    type EndPoint = System.IO.FileAccess
    // Accepted Request:    GET /3
    // Parsed Endpoint:     System.IO.FileAccess.ReadWrite
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* `System.DateTime` is serialized with the format `yyyy-MM-dd-HH.mm.ss`. See below to customize this format.

    ```fsharp
    type EndPoint = System.DateTime
    // Accepted Request:    GET /2015-03-24-15.05.32
    // Parsed Endpoint:     System.DateTime(2015,3,24,15,5,32)
    // Returned Content:    (determined by Sitelet.Infer)
    ```

### Customizing Sitelet.Infer

It is possible to annotate your endpoint type with attributes to customize `Sitelet.Infer`'s request inference. Here are the available attributes:

* [`[<Method("GET", "POST", ...)>]`](/api/v4.1/WebSharper.MethodAttribute) on a union case indicates which methods are parsed by this endpoint. Without this attribute, all methods are accepted.

    ```fsharp
    type EndPoint =
        | [<Method "POST">] PostArticle of id: int

    // Accepted Request:    POST /PostArticle/12
    // Parsed Endpoint:     PostArticle 12
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* [`[<EndPoint "/string">]`](/api/v4.1/WebSharper.EndPointAttribute) on a union case indicates the identifying segment.

    ```fsharp
    type EndPoint =
        | [<EndPoint "/blog-article">] BlogArticle of id: int * slug: string

    // Accepted Request:    GET /blog-article/1423/some-article-slug
    // Parsed Endpoint:     BlogArticle(id = 1423, slug = "some-article-slug")
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* `[<Method>]` and `[<EndPoint>]` can be combined in a single `[<EndPoint>]` attribute:

    ```fsharp
    type EndPoint =
        | [<EndPoint "POST /article">] PostArticle of id: int

    // Accepted Request:    POST /article/12
    // Parsed Endpoint:     PostArticle 12
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* A common trick is to use `[<EndPoint "GET /">]` on an argument-less union case to indicate the home page.

    ```fsharp
    type EndPoint =
        | [<EndPoint "/">] Home

    // Accepted Request:    GET /
    // Parsed Endpoint:     Home
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* If several cases have the same `EndPoint`, then parsing tries them in the order in which they are declared until one of them matches:

    ```fsharp
    type EndPoint =
      | [<EndPoint "GET /blog">] AllArticles
      | [<EndPoint "GET /blog">] ArticleById of id: int
      | [<EndPoint "GET /blog">] ArticleBySlug of slug: string

    // Accepted Request:    GET /blog
    // Parsed Endpoint:     AllArticles
    // Returned Content:    (determined by Sitelet.Infer)
    //
    // Accepted Request:    GET /blog/123
    // Parsed Endpoint:     ArticleById 123
    // Returned Content:    (determined by Sitelet.Infer)
    //
    // Accepted Request:    GET /blog/my-article
    // Parsed Endpoint:     ArticleBySlug "my-article"
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* The method of an endpoint can be specified in a field's type, rather than the main endpoint type itself:

    ```fsharp
    type EndPoint =
      | [<EndPoint "GET /">] Home
      | [<EndPoint "/api">] Api of ApiEndPoint

    and ApiEndPoint =
      | [<EndPoint "GET /article">] GetArticle of int
      | [<EndPoint "POST /article">] PostArticle of int

    // Accepted Request:    GET /
    // Parsed Endpoint:     Home
    // Returned Content:    (determined by Sitelet.Infer)
    //
    // Accepted Request:    GET /api/article/123
    // Parsed Endpoint:     Api (GetArticle 123)
    // Returned Content:    (determined by Sitelet.Infer)
    //
    // Accepted Request:    POST /api/article/456
    // Parsed Endpoint:     Api (PostArticle 456)
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* [`[<Query("arg1", "arg2", ...)>]`](/api/v4.1/WebSharper.QueryAttribute) on a union case indicates that the fields with the given names must be parsed as GET query parameters instead of path segments. The value of this field must be either a base type (number, string) or an option of a base type (in which case the parameter is optional).

    ```fsharp
    type EndPoint =
        | [<Query("id", "slug")>] BlogArticle of id: int * slug: string option

    // Accepted Request:    GET /BlogArticle?id=1423&slug=some-article-slug
    // Parsed Endpoint:     BlogArticle(id = 1423, slug = Some "some-article-slug")
    // Returned Content:    (determined by Sitelet.Infer)
    //
    // Accepted Request:    GET /BlogArticle?id=1423
    // Parsed Endpoint:     BlogArticle(id = 1423, slug = None)
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* You can of course mix Query and non-Query parameters.

    ```fsharp
    type EndPoint =
        | [<Query("slug")>] BlogArticle of id: int * slug: string option

    // Accepted Request:    GET /BlogArticle/1423?slug=some-article-slug
    // Parsed Endpoint:     BlogArticle(id = 1423, slug = Some "some-article-slug")
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* Similarly, `[<Query>]` on a record field indicates that this field must be parsed as a GET query parameter.

    ```fsharp
    type EndPoint =
        {
            id : int
            [<Query>] slug : string option
        }

    // Accepted Request:    GET /1423?slug=some-article-slug
    // Parsed Endpoint:     { id = 1423; slug = Some "some-article-slug" }
    // Returned Content:    (determined by Sitelet.Infer)
    ```

<a name="json-request"></a>

* [`[<Json "arg">]`](/api/v4.1/WebSharper.JsonAttribute) on a union case indicates that the field with the given name must be parsed as JSON from the body of the request. If an endpoint type contains several `[<Json>]` fields, a runtime error is thrown.

    [Learn more about JSON parsing.](Json.md)

    ```fsharp
    type EndPoint =
        | [<Method "POST"; Json "data">] PostBlog of id: int * data: BlogData
    and BlogData =
        {
            slug: string
            title: string
        }

    // Accepted Request:    POST /PostBlog/1423
    //
    //                      {"slug": "some-blog-post", "title": "Some blog post!"}
    //
    // Parsed Endpoint:     PostBlog(
    //                          id = 1423,
    //                          data = { slug = "some-blog-post"
    //                                   title = "Some blog post!" })
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* Similarly, `[<Json>]` on a record field indicates that this field must be parsed as JSON from the body of the request.

    ```fsharp
    type EndPoint =
        | [<Method "POST">] PostBlog of BlogPostArgs
    and BlogPostArgs =
        {
            id: int
            [<Json>] data: BlogData
        }
    and BlogData =
        {
            slug: string
            title: string
        }

    // Accepted Request:    POST /PostBlog/1423
    //
    //                      {"slug": "some-blog-post", "title": "Some blog post!"}
    //
    // Parsed Endpoint:     PostBlog { id = 1423,
    //                                 data = { slug = "some-blog-post"
    //                                          title = "Some blog post!" } }
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* [`[<FormData("arg1", "arg2", ...)>]`](/api/v4.1/WebSharper.FormDataAttribute) on a union case indicates that the fields with the given names must be parsed from the body as form data (`application/x-www-form-urlencoded` or `multipart/form-data`) instead of path segments. The value of this field must be either a base type (number, string) or an option of a base type (in which case the parameter is optional).

    ```fsharp
    type EndPoint =
        | [<FormData("id", "slug")>] BlogArticle of id: int * slug: string option

    // Accepted Request:    POST /BlogArticle
    //                      Content-Type: application/x-www-form-urlencoded
    //
    //                      id=1423&slug=some-article-slug
    //
    // Parsed Endpoint:     BlogArticle(id = 1423, slug = Some "some-article-slug")
    // Returned Content:    (determined by Sitelet.Infer)
    //
    // Accepted Request:    POST /BlogArticle
    //                      Content-Type: application/x-www-form-urlencoded
    //
    //                      id=1423
    //
    // Parsed Endpoint:     BlogArticle(id = 1423, slug = None)
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* Similarly, `[<FormData>]` on a record field indicates that this field must be parsed from the body as form data.

    ```fsharp
    type EndPoint =
        {
            id : int
            [<FormData>] slug : string option
        }

    // Accepted Request:    POST /1423
    //                      Content-Type: application/x-www-form-urlencoded
    //
    //                      slug=some-article-slug
    //
    // Parsed Endpoint:     { id = 1423; slug = Some "some-article-slug" }
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* [`[<DateTimeFormat(string)>]`](/api/v4.1/WebSharper.DateTimeFormatAttribute) on a record field or named union case field of type `System.DateTime` indicates the date format to use. Be careful as some characters are not valid in URLs; in particular, the ISO 8601 round-trip format (`"o"` format) cannot be used because it uses the character `:`.

    ```fsharp
    type EndPoint =
        {
            [<DateTimeFormat "yyyy-MM-dd">] dateOnly: System.DateTime
        }

    // Accepted Request:    GET /2015-03-24
    // Parsed Endpoint:     System.DateTime(2015,3,24)
    // Returned Content:    (determined by Sitelet.Infer)

    type EndPoint =
        | [<DateTimeFormat("time", "HH.mm.ss")>] A of time: System.DateTime

    // Accepted Request:    GET /A/15.05.32
    // Parsed Endpoint:     A (System.DateTime(2015,3,24,15,5,32))
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* [`[<Wildcard>]`](/api/v4.1/WebSharper.WildcardAttribute) on a union case indicates that the last argument represents the remainder of the url's path. That argument can be a `list<'T>`, a `'T[]`, or a `string`.

    ```fsharp
    type EndPoint =
        | [<Wildcard>] Articles of pageId: int * tags: list<string>
        | [<Wildcard>] Articles2 of (int * string)[]
        | [<Wildcard>] GetFile of path: string

    // Accepted Request:    GET /Articles/123/fsharp/websharper
    // Parsed Endpoint:     Articles(123, ["fsharp"; "websharper"])
    // Returned Content:    (determined by Sitelet.Infer)
    //
    // Accepted Request:    GET /Articles2/123/fsharp/456/websharper
    // Parsed Endpoint:     Articles2 [(123, "fsharp"); (456, "websharper")]
    // Returned Content:    (determined by Sitelet.Infer)
    //
    // Accepted Request:    GET /GetFile/css/main.css
    // Parsed Endpoint:     GetFile "css/main.css"
    // Returned Content:    (determined by Sitelet.Infer)
    ```

### Catching wrong requests with Sitelet.InferWithErrors

By default, `Sitelet.Infer` ignores requests that it fails to parse, in order to give potential other components (such as [ASP.NET](http://websharper.com/docs/aspnet)) a chance to respond to the request. However, if you want to send a custom response for badly-formatted requests, you can use [`Sitelet.InferWithErrors`](/api/v4.1/WebSharper.Sitelets.Sitelet#InferWithErrors\`\`1) instead. This function wraps the parsed request in the [`ParseRequestResult<'EndPoint>`](/api/v4.1/WebSharper.Sitelets.ParseRequestResult\`1) union. Here are the cases you can match against:

* `ParseRequestResult.Success of 'EndPoint`: The request was successfully parsed.

* `ParseRequestResult.InvalidMethod of 'EndPoint * method: string`: An endpoint was successfully parsed but with the given wrong HTTP method.

* `ParseRequestResult.MissingQueryParameter of 'EndPoint * name: string`: The URL path was successfully parsed but a mandatory query parameter with the given name was missing. The endpoint value contains a default value (`Unchecked.defaultof<_>`) where the query parameter value should be.

* `ParseRequestResult.InvalidJson of 'EndPoint`: The URL was successfully parsed but the JSON body wasn't. The endpoint value contains a default value (`Unchecked.defaultof<_>`) where the JSON-decoded value should be.

* `ParseRequestResult.MissingFormData of 'EndPoint * name: string`: The URL was successfully parsed but a form data parameter with the given name was missing or wrongly formatted. The endpoint value contains a default value ([`Unchecked.defaultof<_>`](/api/v4.1/Microsoft.FSharp.Core.Operators.Unchecked#defaultof\`\`1)) where the form body-decoded value should be.

If multiple of these kinds of errors happen, only the last one is reported.

If the URL path isn't matched, then the request falls through as with `Sitelet.Infer`.

```fsharp
open WebSharper.Sitelets

module SampleSite =
    open WebSharper.Sitelets

    type EndPoint =
    | [<Method "GET"; Query "page">] Articles of page: int

    [<Website>]
    let MySitelet = Sitelet.InferWithCustomErrors <| fun context endpoint ->
        match endpoint with
        | ParseRequestResult.Success (Articles page) ->
            Content.Text ("serving page " + string page)
        | ParseRequestResult.InvalidMethod (_, m) ->
            Content.Text ("Invalid method: " + m)
            |> Content.SetStatus Http.Status.MethodNotAllowed
        | ParseRequestResult.MissingQueryParameter (_, p) ->
            Content.Text ("Missing parameter: " + p)
            |> Content.SetStatus (Http.Status.Custom 400 (Some "Bad Request"))
        | _ ->
            Content.Text "We don't have JSON or FormData, so this shouldn't happen"
            |> Content.SetStatus Http.Status.InternalServerError

// Accepted Request:    GET /Articles?page=123
// Parsed Endpoint:     Articles 123
// Returned Content:    200 Ok
//                      serving page 123
//
// Accepted Request:    POST /Articles?page=123
// Parsed Endpoint:     InvalidMethod(Articles 123, "POST")
// Returned Content:    405 Method Not Allowed
//                      Invalid method: POST
//
// Accepted Request:    GET /Articles
// Parsed Endpoint:     MissingQueryParameter(Articles 0, "page")
// Returned Content:    400 Bad Request
//                      Missing parameter: page
//
// Request:             GET /this-path-doesnt-exist
// Parsed Endpoint:     (none)
// Returned Content:    (not found page provided by the host)
```

<a name="sitelet-combinators"></a>
### Other Constructors and Combinators

The following functions are available to build simple sitelets or compose more complex sitelets out of simple ones:

* [`Sitelet.Empty`](/api/v4.1/WebSharper.Sitelets.Sitelet#Empty\`\`1) creates a Sitelet which does not recognize any URLs.

* [`Sitelet.Content`](/api/v4.1/WebSharper.Sitelets.Sitelet.Content\`\`1), as shown in the first example, builds a sitelet that accepts a single URL and maps it to a given endpoint and content.

    ```fsharp
    Sitelet.Content "/index" Index IndexContent

    // Accepted Request:    GET /index
    // Parsed Endpoint:     Index
    // Returned Content:    (value of IndexContent : Content<EndPoint>)
    ```

* [`Sitelet.Sum`](/api/v4.1/WebSharper.Sitelets.Sitelet.Sum\`\`1) takes a sequence of Sitelets and tries them in order until one of them accepts the URL. It is generally used to combine a list of `Sitelet.Content`s.

  The following sitelet accepts `/index` and `/about`:

    ```fsharp
    Sitelet.Sum [
        Sitelet.Content "/index" Index IndexContent
        Sitelet.Content "/about" About AboutContent
    ]

    // Accepted Request:    GET /index
    // Parsed Endpoint:     Index
    // Returned Content:    (value of IndexContent : Content<EndPoint>)
    //
    // Accepted Request:    GET /about
    // Parsed Endpoint:     About
    // Returned Content:    (value of AboutContent : Content<EndPoint>)
    ```

* [`+`](/api/v4.1/WebSharper.Sitelets.Sitelet\`1#op_LessBarGreater\`\`1) takes two Sitelets and tries them in order. `s1 + s2` is equivalent to `Sitelet.Sum [s1; s2]`.

    ```fsharp
    Sitelet.Content "/index" Index IndexContent
    +
    Sitelet.Content "/about" About AboutContent

    // Same as above.
    ```

For the mathematically enclined, the functions `Sitelet.Empty` and `+` make sitelets a monoid. Note that it is non-commutative: if a URL is accepted by both sitelets, the left one will be chosen to handle the request.

* [`Sitelet.Shift`](/api/v4.1/WebSharper.Sitelets.Sitelet#Shift\`\`1) takes a Sitelet and shifts it by a path segment.

    ```fsharp
    Sitelet.Content "index" Index IndexContent
    |> Sitelet.Shift "folder"

    // Accepted Request:    GET /folder/index
    // Parsed Endpoint:     Index
    // Returned Content:    (value of IndexContent : Content<EndPoint>)
    ```

* [`Sitelet.Folder`](/api/v4.1/WebSharper.Sitelets.Sitelet#Folder\`\`1) takes a sequence of Sitelets and shifts them by a path segment. It is effectively a combination of `Sum` and `Shift`.

    ```fsharp
    Sitelet.Folder "folder" [
        Sitelet.Content "/index" Index IndexContent
        Sitelet.Content "/about" About AboutContent
    ]

    // Accepted Request:    GET /folder/index
    // Parsed Endpoint:     Index
    // Returned Content:    (value of IndexContent : Content<EndPoint>)
    //
    // Accepted Request:    GET /folder/about
    // Parsed Endpoint:     About
    // Returned Content:    (value of AboutContent : Content<EndPoint>)
    ```

* [`Sitelet.Protect`](/api/v4.1/WebSharper.Sitelets.Sitelet#Protect\`\`1) creates protected content, i.e.  content only available for authenticated users:

    ```fsharp
    module Sitelet =
        type Filter<'EndPoint> =
            {
                VerifyUser : string -> bool;
                LoginRedirect : 'EndPoint -> 'EndPoint
            }

    val Protect : Filter<'EndPoint> -> Sitelet<'EndPoint> -> Sitelet<'EndPoint>
    ```

    Given a filter value and a sitelet, `Protect` returns a new sitelet that requires a logged in user that passes the `VerifyUser` predicate, specified by the filter.  If the user is not logged in, or the predicate returns false, the request is redirected to the endpoint specified by the `LoginRedirect` function specified by the filter. [See here how to log users in and out.](#context)

* [`Sitelet.Map`](/api/v4.1/WebSharper.Sitelets.Sitelet#Map\`\`2) converts a Sitelet to a different endpoint type using mapping functions in both directions.

    ```fsharp
    type EndPoint = Article of string

    let s : Sitelet<string> = Sitelet.Infer sContent

    let s2 : Sitelet<EndPoint> = Sitelet.Map Article (fun (Article a) -> a) s
    ```

* [`Sitelet.Embed`](/api/v4.1/WebSharper.Sitelets.Sitelet#Embed\`\`2) similarly converts a Sitelet to a different endpoint type, but with a partial mapping function: the input endpoint type represents only a subset of the result endpoint type.

    ```fsharp
    type EndPoint =
        | Index
        | Article of string

    let index : Sitelet<EndPoint> = Sitelet.Content "/" Index indexContent
    let article : Sitelet<string> = Sitelet.Infer articleContent
    let fullSitelet =
        Sitelet.Sum [
            index
            article |> Sitelet.Embed Article (function Article a -> Some a | _ -> None)
        ]
    ```

* [`Sitelet.EmbedInUnion`](/api/v4.1/WebSharper.Sitelets.Sitelet#EmbedInUnion\`\`2) is a simpler version of `Sitelet.Embed` when the mapping function is a union case constructor.

    ```fsharp
    type EndPoint =
        | Index
        | Article of string

    let index : Sitelet<EndPoint> = Sitelet.Content "/" Index indexContent
    let article : Sitelet<string> = Sitelet.Infer articleContent
    let fullSitelet =
        Sitelet.Sum [
            index
            article |> Sitelet.EmbedInUnion <@ Article @>
        ]
    ```

* [`Sitelet.InferPartial`](/api/v4.1/WebSharper.Sitelets.Sitelet#InferPartial\`\`2) is equivalent to combining `Sitelet.Infer` and `Sitelet.Embed`, except the context passed to the infer function is of the outer endpoint type instead of the inner. For example, in the example for `Sitelet.Embed` above, the function `articleContent` receives a `Context<string>` and can therefore only create links to articles. Whereas with `InferPartial`, it receives a full `Context<EndPoint>` and can create links to `Index`.

    ```fsharp
    type EndPoint =
        | Index
        | Article of string

    let index : Sitelet<EndPoint> = Sitelet.Content "/" Index indexContent
    let article : Sitelet<EndPoint> =
        Sitelet.InferPartial Article (function Article a -> Some a | _ -> None) articleContent
    let fullSitelet = Sitelet.Sum [ index; article ]
    ```

* [`Sitelet.InferPartialInUnion`](/api/v4.1/WebSharper.Sitelets.Sitelet#InferPartialInUnion\`\`2) is a simpler version of `Sitelet.InferPartial` when the mapping function is a union case constructor.

    ```fsharp
    type EndPoint =
        | Index
        | Article of string

    let index : Sitelet<EndPoint> = Sitelet.Content "/" Index indexContent
    let article : Sitelet<EndPoint> = Sitelet.InferPartialInUnion <@ Article @> articleContent
    let fullSitelet = Sitelet.Sum [ index; article ]
    ```

<a name="content"></a>
## Content

Content describes the response to send back to the client: its HTTP status, headers and body. Content is always worked with asynchronously: all the constructors and combinators described below take and return values of type `Async<Content<'EndPoint>>`. You will find various functions that create different types of content: ordinary text (`Content.Text`), file content (`Content.File`), HTML (`Content.Page`), HTML based on templates (`Content.WithTemplate`), JSON (`Content.Json`), custom content (`Content.Custom`), and HTTP error codes and redirects.

### Content.Text

The simplest response is plain text content, created by passing a string to [`Content.Text`](/api/v4.1/WebSharper.Sitelets.Content#Text\`\`1).

```fsharp
let simpleResponse =
    Content.Text "This is the response body."
```

### Content.File

You can serve files using [`Content.File`](/api/v4.1/WebSharper.Sitelets.Content#File\`\`1).  Optionally, you can set the content type returned for the file response and whether file access is allowed outside of the web root:

```fsharp
type EndPoint = //. . .

let fileResponse: Async<Content<EndPoint>> =
    Content.File("../Main.fs", AllowOutsideRootFolder=true, ContentType="text/plain")
```

### Content.Page

You can return full HTML pages, with managed dependencies using [`Content.Page`](/api/v4.1/WebSharper.UI.Next.Server.Content#Page\`\`1). Here is a simple example:

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

The optional named arguments `Title`, `Head`, `Body` and `Doctype` set the corresponding elements of the HTML page. To learn how to create HTML elements for `Head` and `Body`, see [the HTML combinators documentation](HtmlCombinators.md).

### Content.WithTemplate

Very often, most of a page is constant, and only parts of it need to be generated. Templates allow you to use a static HTML file for the main structure, with placeholders for generated content. [See here for more information about templates.](Templates.md)

<a name="json-response"></a>
### Content.Json

If you are creating a web API, then Sitelets can automatically generate JSON content for you based on the type of your data. Simply pass your value to [`Content.Json`](/api/v4.1/WebSharper.Sitelets.Content#Json\`\`1), and WebSharper will serialize it. The format is the same as when parsing requests. [See here for more information about the JSON format.](Json.md)

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

### Content.Custom

[`Content.Custom`](/api/v4.1/WebSharper.Sitelets.Content#Custom\`\`1) can be used to output any type of content. It takes three optional named arguments that corresponds to the aforementioned elements of the response:

* `Status` is the HTTP status code. It can be created using the function [`Http.Status.Custom`](/api/v4.1/WebSharper.Sitelets.Http.Status#Custom), or you can use one of the predefined statuses such as [`Http.Status.Forbidden`](/api/v4.1/WebSharper.Sitelets.Http.Status#Forbidden).

* `Headers` is the HTTP headers. You can create them using the function [`Http.Header.Custom`](/api/v4.1/WebSharper.Sitelets.Http.Header#Custom).

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

In addition to the four standard Content families above, the [`Content`](/api/v4.1/WebSharper.Sitelets.Content) module contains a few helper functions.

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

<a name="context"></a>
## Using the Context

The functions to create sitelets from content, namely `Sitelet.Infer` and `Sitelet.Content`, provide a context of type [`Context<'T>`](/api/v4.1/WebSharper.Sitelets.Context\`1). This context can be used for several purposes; the most important are creating internal links and managing user sessions.

<a name="linking"></a>
### Creating links

Since every accepted URL is uniquely mapped to a strongly typed endpoint value, it is also possible to generate internal links from an endpoint value. For this, you can use the method [`context.Link`](/api/v4.1/WebSharper.Sitelets.Context\`1#Link).

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

Note how `context.Link` is used in order to resolve the URL to the `BlogArticle` endpoint.  Endpoint URLs are always constructed relative to the application root, whether the application is deployed as a standalone website or in a virtual folder.  [`context.ResolveUrl`](/api/v4.1/WebSharper.Sitelets.Context\`1#ResolveUrl) helps to manually construct application-relative URLs to resources that do not map to endpoints.

### Managing User Sessions

`Context<'T>` can be used to access the currently logged in user. The member [`UserSession`](/api/v4.1/WebSharper.Sitelets.Context\`1#UserSession) has the following members:

* [`LoginUser : username: string * ?persistent: bool -> Async<unit>`](/api/v4.1/WebSharper.Web.IUserSession#LoginUser)  
  [`LoginUser : username: string * duration: System.TimeSpan -> Async<unit>`](/api/v4.1/WebSharper.Web.IUserSession#LoginUser)

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

* [`GetLoggedInUser : unit -> Async<string option>`](/api/v4.1/WebSharper.Web.IUserSession#GetLoggedInUser)

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

* [`Logout : unit -> unit`](/api/v4.1/WebSharper.Web.IUserSession#Logout)

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

`WebSharper.Sitelets.Context<'T>` inherits from `WebSharper.Web.Context`, and a number of properties and methods from it are useful. [See the documentation for `WebSharper.Web.Context`](WebContext.md).

<a name="advanced-sitelets"></a>
## Advanced Sitelets

So far, we have constructed sitelets using built-in constructors such as `Sitelet.Infer`. But if you want finer-grained control over the exact URLs that it parses and generates, you can create sitelets by hand.

A sitelet consists of two parts; a router and a handler.
The job of the router is to map endpoints to URLs and to map HTTP requests to endpoints.
The handler is responsible for handling endpoints, by returning content (a synchronous or asynchronous HTTP response).

### Routers

The router component of a sitelet can be constructed in multiple ways. The main options are: 

* Declaratively, using `Router.Infer` which is also used internally by `Sitelets.Infer`. The main advantage of creating a router value separately, is that it also be added a `[<JavaScript>]` attribute, so that the client can generate links from endpoint values too. `WebSharper.UI` also contains functionality for client-side routing, making it possible to handle all or a subset of internal links without browser navigation. So sharing the router abstraction between client and server means that server can generate links that the client will handle and vice versa.
* Manually, by using combinators to build up larger routers from elementary `Router` values or inferred ones. You can use this to further customize routing logic if you want an URL schema that is not fitting default inferred URL shapes, or add additional URLs to handle (e. g. for keeping compatibility with old links).
* Implementing the `IRouter` interface directly or using the `Router.New` helper. This is the most universal way, but has less options for composition.

The following example shows how you can create a router of type `WebSharper.Sitelets.IRouter<EndPoint>` by writing the two mappings manually:

```fsharp
open WebSharper.Sitelets

module WebSite =
    type EndPoint = | Page1 | Page2

    let MyRouter : Router<EndPoint> =
        let route (req: Http.Request) =
            if req.Uri.LocalPath = "/page1" then
                Some Page1
            elif req.Uri.LocalPath = "/page2" then
                Some Page2
            else
                None
        let link endPoint =
            match endPoint with
            | EndPoint.Page1 ->
                Some <| System.Uri("/page1", System.UriKind.Relative)
            | EndPoint.Page2 ->
                Some <| System.Uri("/page2", System.UriKind.Relative)
        Router.New route link
```

A simplified version, `Router.Create` exists to create routers, using only already broken up URL segments:

```fsharp
open WebSharper.Sitelets

module WebSite =
    type EndPoint = | Page1 | Page2

    let MyRouter : Router<EndPoint> =
        let link endPoint =
            match endPoint with
            | Page1 -> [ "page1" ]
            | Page2 -> [ "page2" ]
        let route path =
            match path with
            | [ "page1" ] -> Some Page1
            | [ "page2" ] -> Some Page2
            | _ -> None
        Router.Create link route
```

Specifying routers manually gives you full control of how to parse incoming requests and to map endpoints to corresponding URLs.  It is your responsibility to make sure that the router forms a bijection of URLs and endpoints, so that linking to an endpoint produces a URL that is in turn routed back to the same endpoint.

Constructing routers manually is only required for very special cases. The above router can for example be generated using [`Router.Table`](/api/v4.1/WebSharper.Sitelets.Router#Table\`\`1):

```fsharp
let MyRouter : Router<EndPoint> =
    [
        EndPoint.Page1, "/page1"
        EndPoint.Page2, "/page2"
    ]
    |> Router.Table
```

Even simpler, if you want to create the same URL shapes that would be generated by `Sitelet.Infer`, you can simply use [`Router.Infer()`](/api/v4.1/WebSharper.Sitelets.Router#Infer\`\`1):

```fsharp
let MyRouter : Router<EndPoint> =
    Router.Infer ()
```

### Router primitives

The `WebSharper.Sitelets.RouterOperators` module exposes the following basic `Router` values and construct functions:

* `rRoot`: Recognizes and writes an empty path.
* `r "path"`: Recognizes and writes a specific subpath. You can also write `r "path/subpath"` to parse two or more segments of the URL.
* `rString`, `rChar`: Recognizes a URIComponent as a string or char and writes it as a URIComponent.
* `rTryParse<'T>`: Creates a router for any type that defines a `TryParse` static method.
* `rInt`, `rDouble`, ...: Creates a router for numeric values.
* `rBool`, `rGuid`: Additional primitive types to parse from or write to the URL. 
* `rDateTime`: Parse or write a `DateTime`, takes a format string.

### Router combinators

* `/` (alias `Router.Combine`): Parses or writes using two routers one after the other. For example `rString / rInt` will have type `Router<string * int>`. This operator has overloads for any combination of generic and non-generic routers, as well as a string on either side to add a constant URL fragment. For example `r "article" / r "id" / rInt` can be shortened to `"article/id" / rInt`.
* `+` (alias `Router.Add`): Parses or writes using the first router if successful, otherwise the second.
* `Router.Sum`: Optimized version of combining a sequence of routers with `+`. Parses or writes with the first router in the sequence that can handle the path or value.
* `Router.Map`: A bijection (or just surjection) between representations handled by routers. For example if you have a `type Person = { Name: string; Age: int }`, then you can define a router for it by mapping from a `Router<string * int>` like so
    ```fsharp
    let rPerson : Router<Person> =
        rString / rInt
        |> Router.Map 
            (fun (n, a) -> { Name = n; Age = a })
            (fun p -> p.Name, p.Age)
    ```
    See that `Map` needs two function arguments, to convert data back and forth between representations. All values of the resulting type must be mapped back to underlying type by the second function in a way compatible with the first function to work correctly.
* `Router.MapTo`: Maps a non-generic `Router` to a single valued `Router<'T>`. For example if `Home` is a union case in your `Pages` union type describing pages on your site, you can create a router for it by:
    ```fsharp
    let rHome : Router<Pages> =
        rRoot |> Router.MapTo Home
    ```
    This only needs a single value as argument, but the type used must be comparable, so the writer part of the newly created `Router<'T>` can decide if it is indeed a `Home` value that it needs to write by the underlying router (in our case producing a root URL).
* `Router.Embed`: An injection between representations handled by routers. For example if you have a `Router<Person>` parsing a person's details, and a `Contact of Person` union case in your `Pages` union, you can do:
    ```fsharp
    let rContact : Router<Pages> =
        "contact" / rPerson 
        |> Router.Embed
            Contact
            (function Contact p -> Some p | _ -> None)
    ```
    See that now we have two functions again, but the second is returning an option. The first tells us that once a path is parsed (for example we are recognizing `contact/Bob/32` here), it can wrap it in a `Contact` case (`Contact` here is used as a short version of a union case constructor, a function with signature `Person -> Pages`). And if the newly created router gets a value to write, it can use the second function to map it back optionally to an underlying value.
* `Router.Filter`: restricts a router to parse/write values only that are passing a check. Usage: `rInt |> Router.Filter (fun x -> x >= 0)`, which won't parse and write negative values.
* `Router.Slice`: restricts a router to parse/write values only that can be mapped to a new value. Equivalent to using `Filter` first to restrict the set of values and then `Map` to convert to a type that is a better representation of the restricted values.
* `Router.TryMap`: a combination of `Slice` and `Embed`, a mapping from a subset of source values to a subset of target values. Both the encode and decode functions must return `None` if there is no mapping to a value of the other type.
* `Router.Query`: Modifies a router to parse from and write to a specific query argument instead of main URL segments. Usage: `rInt |> Router.Query "x"`, which will read/write query segments like `?x=42`. You should pass only a router that is always reading/writing a single segment, which inclide primitive routers, `Router.Nullable`, and `Sum`s and `Map`s of these.
* `Router.QueryOption`: Modifies a router to read an optional query value as an F# option. Creates a `Router<option<'T>>`, same restrictions apply as to `Query`.
* `Router.QueryNullable`: Modifies a router to read an optional query value as a `System.Nullable`. Creates a `Router<Nullable<'T>>`, same restrictions apply as to `Query`.
* `Router.Box`: Converts a `Router<'T>` to a `Router<obj>`. When writing, it uses a type check to see if the object is of type `'T` so it can be passed to underlying router.
* `Router.Unbox`:  Converts a `Router<obj>` to a `Router<'T>`. When parsing, it uses a type check to see if the object is of type `'T` so that the parsed value can be represented in `'T`.
* `Router.Array`: Creates an array parser/writer. The URL will contain the length and then the items, so for example `Router.Array rString` can handle `2/x/y`.
* `Router.List`: Creates a list parser/writer. Similar to `Router.Array`, just uses F# lists as data type.
* `Router.Option`: Creates an F# option parser/writer. Writes or reads `None` and `Some/x` segments.
* `Router.Nullable`: Creates a `Nullable` value parser/writer. Writes or reads `null` for null or a value that is handled by the input router. For 
* `Router.Infer`: Creates a router based on type shape. The attributes recognized are the same as `Sitelet.Infer` described in the [Sitelets documentation](sitelets.md).
* `Router.Table`: Creates a router mapping between a list of static endpoint values and paths.
* `Router.Method`: Creates a router that only parses request with the inner router, it the HTTP method methes the given method argument. By default, routers ignore the method.
* `Router.Body` : Creates a router that parses and serializes any value to and from the request body with custom functions. If the will be used on server-side only to parse requests and generate links, the serialize function can return just a null or empty string. For example `Router.Body id id` just gets the request body as a string.
* [`Router.Json`](/api/v4.1/WebSharper.Sitelets.Router#Json\`\`1) creates a router that parses the request body by the JSON format derived from the type argument.
* [`Router.FormData`](/api/v4.1/WebSharper.Sitelets.Router#FormData) creates a router from an underlying router handling query arguments that parses query arguments from the request body of a form post instead of the URL.
* `Router.Delay` can be used to construct routers for recursive data types. Takes a `unit -> Router<'T>` function, and evaluates it firsthe t time the router is used for parsing and writing (never just when combining them).

### Using the router

* `Router.Link` creates a (relative) link using a router.
A useful helper to have in the file defining your router is:
    ```fsharp
        let Link page content =
            a [ attr.href (Router.Link router page) ] [ text content ]
    ```
This works the same on both server and client-side to create basic `<a>` links to pages of your web application.
* `Sitelet.New` creates a Sitelet from a router and handler. Example:
```fsharp
    [<Website>]
    let Main =
        Sitelet.New rPages (fun ctx ep ->
            match ep with 
            | Home -> div [] [ text "This is the home page" ]
            | Contact _ -> client <@ ContactMain() @>
        )
```
Here we return a static page for the root, but call into a client-side generated content in the `Contact` pages, which is parsing the URL again to show the contact details from the URL.
Sitelets are only a server-side type.
* `Router.Ajax` makes a request from an endpoint value on the client and executes it using `jQuery.ajax`. Returns an `async<string>`, which raises an exception internally if the request fails. Example:
```fsharp
    // [<EndPoint "/get-data">] GetData of int

    let GetDataAsyncSafe i =
        async {
            try
                return! Some (Router.Ajax router (GetData i))
            with _ ->
                None
        }
```
