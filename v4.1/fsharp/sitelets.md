# Routing requests and serving content with Sitelets #

Sitelets are WebSharper's primary way to create server-side content. They provide facilities to route requests and generate HTML pages or JSON responses.

Sitelets allow you to:

* Dynamically construct pages and serve arbitrary content.

* Have full control of your URLs by specifying [custom routers](#advanced-sitelets) for linking them to content, or let the URLs be [automatically inferred](#sitelet-infer) from an endpoint type.

* Compose contents into sitelets, which may themselves be [composed into larger sitelets](#sitelet-combinators).

* Have [safe links](#linking) for referencing other content contained within your site.

* Use the type-safe HTML and templating facilities from [UI.Next](ui.md) on the server side.

* Automatically [parse JSON requests and generate JSON responses](Json.md) based on your types.

Below is a minimal example of a complete site serving one HTML page:

```fsharp
namespace SampleWebsite

open WebSharper.Sitelets

module SampleSite =
    open WebSharper
    open WebSharper.UI.Next.Html
    open WebSharper.UI.Next.Server

    type EndPoint =
        | Index

    let IndexContent context : Async<Content<EndPoint>> =
        let time = System.DateTime.Now.ToString()
        Content.Page(
            Title = "Index",
            Body = [h1 [text ("Current time: " + time)]]
        )

    [<Website>]
    let MySampleWebsite : Sitelet<EndPoint> =
        Sitelet.Content "/index" EndPoint.Index IndexContent
```

First, a custom endpoint type is defined. It is used for linking requests to content within your sitelet. Here, you only need one endpoint, `EndPoint.Index`, corresponding to your only page.

The content of the index page is defined as a [`Content.Page`](/api/WebSharper.UI.Next.Server.Content#Page``1), where the body consists of a server side HTML element.  Here the current time is computed and displayed within an `<h1>` tag.

The `MySampleWebsite` value has type [`Sitelet<EndPoint>`](/api/WebSharper.Sitelets.Sitelet`1). It defines a complete website: the URL scheme, the `EndPoint` value corresponding to each served URL (only one in this case), and the content to serve for each endpoint. It uses the [`Sitelet.Content`](/api/WebSharper.Sitelets.Sitelet#Content``1) operator to construct a sitelet for the Index endpoint, associating it with the `/index` URL and serving `IndexContent` as a response.

`MySampleWebsite` is annotated with the attribute [`[<Website>]`](/api/WebSharper.Sitelets.WebsiteAttribute) to indicate that this is the sitelet that should be served.

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

<a name="sitelet-infer"></a>
### Sitelet.Infer

The easiest way to create a Sitelet is to automatically generate URLs from the shape of your endpoint type using [`Sitelet.Infer`](/api/WebSharper.Sitelets.Sitelet#Infer``1), also aliased as [`Application.MultiPage`](/api/WebSharper.Application#MultiPage``1). This function parses slash-separated path segments into the corresponding `EndPoint` value, and lets you match this endpoint and return the appropriate content. Here is an example sitelet using `Infer`:

```fsharp
namespace SampleWebsite

open WebSharper.Sitelets

module SampleSite =
    open WebSharper
    open WebSharper.UI.Next.Html
    open WebSharper.UI.Next.Server

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
                     Body = [h1 [text "Index page"]])
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

* Union types are encoded as a path segment identifying the case, followed by segments for the arguments (see the example above).

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

* [`[<Method("GET", "POST", ...)>]`](/api/WebSharper.MethodAttribute) on a union case indicates which methods are parsed by this endpoint. Without this attribute, all methods are accepted.

    ```fsharp
    type EndPoint =
        | [<Method "POST">] PostArticle of id: int

    // Accepted Request:    POST /PostArticle/12
    // Parsed Endpoint:     PostArticle 12
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* [`[<EndPoint "/string">]`](/api/WebSharper.EndPointAttribute) on a union case indicates the identifying segment.

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

* [`[<Query("arg1", "arg2", ...)>]`](/api/WebSharper.QueryAttribute) on a union case indicates that the fields with the given names must be parsed as GET query parameters instead of path segments. The value of this field must be either a base type (number, string) or an option of a base type (in which case the parameter is optional).

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

* [`[<Json "arg">]`](/api/WebSharper.JsonAttribute) on a union case indicates that the field with the given name must be parsed as JSON from the body of the request. If an endpoint type contains several `[<Json>]` fields, a runtime error is thrown.

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

* [`[<FormData("arg1", "arg2", ...)>]`](/api/WebSharper.FormDataAttribute) on a union case indicates that the fields with the given names must be parsed from the body as form data (`application/x-www-form-urlencoded` or `multipart/form-data`) instead of path segments. The value of this field must be either a base type (number, string) or an option of a base type (in which case the parameter is optional).

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

* [`[<DateTimeFormat(string)>]`](/api/WebSharper.DateTimeFormatAttribute) on a record field or named union case field of type `System.DateTime` indicates the date format to use. Be careful as some characters are not valid in URLs; in particular, the ISO 8601 round-trip format (`"o"` format) cannot be used because it uses the character `:`.

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

* [`[<Wildcard>]`](/api/WebSharper.WildcardAttribute) on a union case indicates that the last argument represents the remainder of the url's path. That argument can be a `list<'T>`, a `'T[]`, or a `string`.

    ```fsharp
    type EndPoint =
        | [<Wildcard>] Articles of pageId: int * tags: list<string>
        | [<Wildcard>] Articles2 of (int * string)[]
        | [<Wildcard>] GetFile of path: string

    // Accepted Request:    GET /Articles/123/fsharp/websharper
    // Parsed Endpoint:     Articles(123, ["fsharp"; "websharper"])
    // Returned Content:    (determined by Sitelet.Infer)
    //
    // Accepted Request:    GET /Articles/123/fsharp/456/websharper
    // Parsed Endpoint:     Articles2 [(123, "fsharp"); (456, "websharper")]
    // Returned Content:    (determined by Sitelet.Infer)
    //
    // Accepted Request:    GET /GetFile/css/main.css
    // Parsed Endpoint:     GetFile "css/main.css"
    // Returned Content:    (determined by Sitelet.Infer)
    ```

### Catching wrong requests with Sitelet.InferWithErrors

By default, `Sitelet.Infer` ignores requests that it fails to parse, in order to give potential other components (such as [ASP.NET](http://websharper.com/docs/aspnet)) a chance to respond to the request. However, if you want to send a custom response for badly-formatted requests, you can use [`Sitelet.InferWithErrors`](/api/WebSharper.Sitelets.Sitelet#InferWithErrors``1) instead. This function wraps the parsed request in the [`ActionEncoding.DecodeResult<'EndPoint>`](/api/WebSharper.Sitelets.ActionEncoding.DecodeResult`1) union. Here are the cases you can match against:

* `ActionEncoding.Success of 'EndPoint`: The request was successfully parsed.

* `ActionEncoding.InvalidMethod of 'EndPoint * method: string`: An endpoint was successfully parsed but with the given wrong HTTP method.

* `ActionEncoding.MissingQueryParameter of 'EndPoint * name: string`: The URL path was successfully parsed but a mandatory query parameter with the given name was missing. The endpoint value contains a default value (`Unchecked.defaultof<_>`) where the query parameter value should be.

* `ActionEncoding.InvalidJson of 'EndPoint`: The URL was successfully parsed but the JSON body wasn't. The endpoint value contains a default value (`Unchecked.defaultof<_>`) where the JSON-decoded value should be.

* `ActionEncoding.MissingFormData of 'EndPoint * name: string`: The URL was successfully parsed but a form data parameter with the given name was missing or wrongly formatted. The endpoint value contains a default value ([`Unchecked.defaultof<_>`](/api/Microsoft.FSharp.Core.Operators.Unchecked#defaultof``1)) where the form body-decoded value should be.

If the URL path isn't matched, then the request falls through as with `Sitelet.Infer`.

```fsharp
open WebSharper.Sitelets

module SampleSite =
    open WebSharper.Sitelets.ActionEncoding

    type EndPoint =
    | [<Method "GET"; Query "page">] Articles of page: int

    [<Website>]
    let MySitelet = Sitelet.InferWithCustomErrors <| fun context endpoint ->
        match endpoint with
        | Success (Articles page) ->
            Content.Text ("serving page " + string page)
        | InvalidMethod (_, m) ->
            Content.Text ("Invalid method: " + m)
            |> Content.SetStatus Http.Status.MethodNotAllowed
        | MissingQueryParameter (_, p) ->
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

* [`Sitelet.Empty`](/api/WebSharper.Sitelets.Sitelet#Empty``1) creates a Sitelet which does not recognize any URLs.

* [`Sitelet.Content`](/api/WebSharper.Sitelets.Sitelet.Content``1), as shown in the first example, builds a sitelet that accepts a single URL and maps it to a given endpoint and content.

    ```fsharp
    Sitelet.Content "/index" Index IndexContent

    // Accepted Request:    GET /index
    // Parsed Endpoint:     Index
    // Returned Content:    (value of IndexContent : Content<EndPoint>)
    ```

* [`Sitelet.Sum`](/api/WebSharper.Sitelets.Sitelet.Sum``1) takes a sequence of Sitelets and tries them in order until one of them accepts the URL. It is generally used to combine a list of `Sitelet.Content`s.

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

* [`<|>`](/api/WebSharper.Sitelets.Sitelet`1#op_LessBarGreater``1) takes two Sitelets and tries them in order. `s1 <|> s2` is equivalent to `Sitelet.Sum [s1; s2]`.

    ```fsharp
    Sitelet.Content "/index" Index IndexContent
    <|>
    Sitelet.Content "/about" About AboutContent

    // Same as above.
    ```

For the mathematically enclined, the functions `Sitelet.Empty` and `<|>` make sitelets a monoid. Note that it is non-commutative: if a URL is accepted by both sitelets, the left one will be chosen to handle the request.

* [`Sitelet.Shift`](/api/WebSharper.Sitelets.Sitelet#Shift``1) takes a Sitelet and shifts it by a path segment.

    ```fsharp
    Sitelet.Content "index" Index IndexContent
    |> Sitelet.Shift "folder"

    // Accepted Request:    GET /folder/index
    // Parsed Endpoint:     Index
    // Returned Content:    (value of IndexContent : Content<EndPoint>)
    ```

* [`Sitelet.Folder`](/api/WebSharper.Sitelets.Sitelet#Folder``1) takes a sequence of Sitelets and shifts them by a path segment. It is effectively a combination of `Sum` and `Shift`.

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

* [`Sitelet.Protect`](/api/WebSharper.Sitelets.Sitelet#Protect``1) creates protected content, i.e.  content only available for authenticated users:

    ```fsharp
    module Sitelet =
        type Filter<'EndPoint> =
            {
                VerifyUser : string -> bool;
                LoginRedirect : 'EndPoint -> 'EndPoint
            }

    val Protect : Filter<'EndPoint> -> Sitelet<'EndPoint> -> Sitelet<'EndPoint>
    ```

    Given a filter value and a sitelet, `Protect` returns a new sitelet that requires a logged in user that passes the `VerifyUser` predicate, specified by the filter.  If the user is not logged in, or the predicate returns false, the request is redirected to the action specified by the `LoginRedirect` function specified by the filter. [See here how to log users in and out.](#context)

* [`Sitelet.Map`](/api/WebSharper.Sitelets.Sitelet#Map``2) converts a Sitelet to a different endpoint type using mapping functions in both directions.

    ```fsharp
    type EndPoint = Article of string

    let s : Sitelet<string> = Sitelet.Infer sContent

    let s2 : Sitelet<EndPoint> = Sitelet.Map Article (fun (Article a) -> a) s
    ```

* [`Sitelet.Embed`](/api/WebSharper.Sitelets.Sitelet#Embed``2) similarly converts a Sitelet to a different endpoint type, but with a partial mapping function: the input endpoint type represents only a subset of the result endpoint type.

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

* [`Sitelet.EmbedInUnion`](/api/WebSharper.Sitelets.Sitelet#EmbedInUnion``2) is a simpler version of `Sitelet.Embed` when the mapping function is a union case constructor.

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

* [`Sitelet.InferPartial`](/api/WebSharper.Sitelets.Sitelet#InferPartial``2) is equivalent to combining `Sitelet.Infer` and `Sitelet.Embed`, except the context passed to the infer function is of the outer endpoint type instead of the inner. For example, it the example for `Sitelet.Embed` above, the function `articleContent` receives a `Context<string>` and can therefore only create links to articles. Whereas with `InferPartial`, it receives a full `Context<EndPoint>` and can create links to `Index`.

    ```fsharp
    type EndPoint =
        | Index
        | Article of string

    let index : Sitelet<EndPoint> = Sitelet.Content "/" Index indexContent
    let article : Sitelet<EndPoint> =
        Sitelet.InferPartial Article (function Article a -> Some a | _ -> None) articleContent
    let fullSitelet = Sitelet.Sum [ index; article ]
    ```

* [`Sitelet.InferPartialInUnion`](/api/WebSharper.Sitelets.Sitelet#InferPartialInUnion``2) is a simpler version of `Sitelet.InferPartial` when the mapping function is a union case constructor.

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

The simplest response is plain text content, created by passing a string to [`Content.Text`](/api/WebSharper.Sitelets.Content#Text``1).

```fsharp
let simpleResponse =
    Content.Text "This is the response body."
```

### Content.File

You can serve files using [`Content.File`](/api/WebSharper.Sitelets.Content#File``1).  Optionally, you can set the content type returned for the file response and whether file access is allowed outside of the web root:

```fsharp
type EndPoint = //. . .

let fileResponse: Async<Content<EndPoint>> =
    Content.File("../Main.fs", AllowOutsideRootFolder=true, ContentType="text/plain")
```

### Content.Page

You can return full HTML pages, with managed dependencies using [`Content.Page`](/api/WebSharper.UI.Next.Server.Content#Page``1). Here is a simple example:

```fsharp
open WebSharper.UI.Next.Html
    
let IndexPage : Async<Content<EndPoint>> =
    Content.Page(
        Title = "Welcome!",
        Head = [ linkAttr [attr.href "/css/style.css"; attr.rel "stylesheet"] [] ],
        Body = [
            h1 [text "Welcome to my site."] 
        ]
    )
```

The optional named arguments `Title`, `Head`, `Body` and `Doctype` set the corresponding elements of the HTML page. To learn how to create HTML elements for `Head` and `Body`, see [the HTML combinators documentation](HtmlCombinators.md).

### Content.WithTemplate

Very often, most of a page is constant, and only parts of it need to be generated. Templates allow you to use a static HTML file for the main structure, with placeholders for generated content. [See here for more information about templates.](Templates.md)

<a name="json-response"></a>
### Content.Json

If you are creating a web API, then Sitelets can automatically generate JSON content for you based on the type of your data. Simply pass your value to [`Content.Json`](/api/WebSharper.Sitelets.Content#Json``1), and WebSharper will serialize it. The format is the same as when parsing requests. [See here for more information about the JSON format.](Json.md)

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

[`Content.Custom`](/api/WebSharper.Sitelets.Content#Custom``1) can be used to output any type of content. It takes three optional named arguments that corresponds to the aforementioned elements of the response:

* `Status` is the HTTP status code. It can be created using the function [`Http.Status.Custom`](/api/WebSharper.Sitelets.Http.Status#Custom), or you can use one of the predefined statuses such as [`Http.Status.Forbidden`](/api/WebSharper.Sitelets.Http.Status#Forbidden).

* `Headers` is the HTTP headers. You can create them using the function [`Http.Header.Custom`](/api/WebSharper.Sitelets.Http.Header#Custom).

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

In addition to the four standard Content families above, the [`Content`](/api/WebSharper.Sitelets.Content) module contains a few helper functions.

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
            Body = [Text "Oops! You're not supposed to be here."]
        )
        // Set the HTTP status code to 403 Forbidden:
        |> Content.SetStatus Http.Status.Forbidden
        // Add an HTTP header:
        |> Content.WithHeaders [Http.Header.Custom "Content-Language" "en"]
    ```

<a name="context"></a>
## Using the Context

The functions to create sitelets from content, namely `Sitelet.Infer` and `Sitelet.Content`, provide a context of type [`Context<'T>`](/api/WebSharper.Sitelets.Context`1). This context can be used for several purposes; the most important are creating internal links and managing user sessions.

<a name="linking"></a>
### Creating links

Since every accepted URL is uniquely mapped to a strongly typed action value, it is also possible to generate internal links from an action value. For this, you can use the method [`context.Link`](/api/WebSharper.Sitelets.Context`1#Link).

```fsharp
open WebSharper.UI.Next.Html

type EndPoint = | BlogArticle of id:int * slug:string

let HomePage (context: Context<EndPoint>) =
    Content.Page(
        Title = "Welcome!",
        Body = [
            h1 [text "Index page"]
            aAttr [attr.href (context.Link (BlogArticle(1423, "some-article-slug")))] [
                text "Go to some article"
            ]
            br []
            aAttr [attr.href (context.ResolveUrl "~/Page2.html")] [
                text "Go to page 2"
            ]
        ]
    )
```

Note how `context.Link` is used in order to resolve the URL to the `BlogArticle` action.  Action URLs are always constructed relative to the application root, whether the application is deployed as a standalone website or in a virtual folder.  [`context.ResolveUrl`](/api/WebSharper.Sitelets.Context`1#ResolveUrl) helps to manually construct application-relative URLs to resources that do not map to actions.

### Managing User Sessions

`Context<'T>` can be used to access the currently logged in user. The member [`UserSession`](/api/WebSharper.Sitelets.Context`1#UserSession) has the following members:

* [`LoginUser : username: string * ?persistent: bool -> Async<unit>`](/api/WebSharper.Web.IUserSession#LoginUser)  
  [`LoginUser : username: string * duration: System.TimeSpan -> Async<unit>`](/api/WebSharper.Web.IUserSession#LoginUser)

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

* [`GetLoggedInUser : unit -> Async<string option>`](/api/WebSharper.Web.IUserSession#GetLoggedInUser)

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

* [`Logout : unit -> unit`](/api/WebSharper.Web.IUserSession#Logout)

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

* [`context.ApplicationPath`](/api/WebSharper.Web.Context#ApplicationPath) returns the web root of the application. Most of the time this will be `"/"`, unless you use a feature such as an ASP.NET virtual directory.

* [`context.Request`](/api/WebSharper.Web.Context#Request) returns the `Http.Request` being responded to. This is useful to access elements such as HTTP headers, posted files or cookies.

* [`context.ResolveUrl`](/api/WebSharper.Web.Context#ResolveUrl) resolves links to static pages in your application. A leading `~/` character is translated to the `ApplicationPath` described above.

* [`context.RootFolder`](/api/WebSharper.Web.Context#RootFolder) returns the physical folder on the server machine from which the application is running.

* [`context.Environment`](/api/WebSharper.Web.Context#Environment) returns an `IDictionary<string, obj>` which depends on the host on which WebSharper is running.

    * When running on ASP.NET, `context.Environment.["HttpContext"]` contains the `System.Web.HttpContextBase` for the current request.
    
    * When running on OWIN, `context.Environment` is the OWIN environment.

<a name="advanced-sitelets"></a>
## Advanced Sitelets

So far, we have constructed sitelets using built-in constructors such as `Sitelet.Infer`. But if you want finer-grained control over the exact URLs that it parses and generates, you can create sitelets by hand.

A sitelet consists of two parts; a router and a controller.  The job of the router is to map actions to URLs and to map HTTP requests to actions. The controller is responsible for handling actions, by converting them into content that in turn produces the HTTP response. The overall architecture is analogous to ASP.NET/MVC, and other `Model-View-Controller` -based web frameworks.

### Routers

The router component of a sitelet can be constructed in multiple ways. The main options are: 

* Declaratively, using `Router.Infer` which is also used internally by `Sitelets.Infer`. The main advantage of creating a router value separately, is that it can be also be added a `[JavaScript]` attribute, so that the client can generate links from endpoint values too. `WebSharper.UI` also contains functionality for client-side routing, making it possible to handle all or a subset of internal links without browser navigation. So sharing the router abstraction between client and server means that server can generate links that the client will handle and vice versa.
* Manually, by using combinators to build up larger routers from elementary router values or inferred ones. You can use this to further customize routing logic if you want an URL schema that is not fitting default inferred URL shapes, or add additional URLs to handle (e. g. for keeping compatibility with old links).

The following example shows how you can create a customized router of type `WebSharper.Sitelets.Routing.Router<Action>` by writing the two mappings manually:

```fsharp
open WebSharper.Sitelets
open WebSharper.Sitelets.Routing

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

Specifying routers manually gives you full control of how to parse incoming requests and to map actions to corresponding URLs.  It is your responsibility to make sure that the router forms a bijection of URLs and actions, so that linking to an action produces a URL that is in turn routed back to the same action.

Constructing routers manually is only required for very special cases. The above router can for example be generated using [`Router.Table`](/api/WebSharper.Sitelets.Routing.Router#Table``1):

```fsharp
let MyRouter : Router<EndPoint> =
    [
        EndPoint.Page1, "/page1"
        EndPoint.Page2, "/page2"
    ]
    |> Router.Table
```

Even simpler, if you want to create the same URL shapes that would be generated by `Sitelet.Infer`, you can simply use [`Router.Infer()`](/api/WebSharper.Sitelets.Routing.Router#Infer``1):

```fsharp
let MyRouter : Router<EndPoint> =
    Router.Infer ()
```

### Other Router combinators

Combinators are also defined by the `Router` module inside `WebSharper.Sitelets.Routing` namespace.

* [`Router.Map`](/api/WebSharper.Sitelets.Routing.Router#Map``2), [`Router.Shift`](/api/WebSharper.Sitelets.Routing.Router#Shift``1), [`Router.Sum`](/api/WebSharper.Sitelets.Routing.router#Sum``1), [`Router.Empty`](/api/WebSharper.Sitelets.Routing.Router#Empty``1) are the routing-only combinators of the same functions for Sitelets described above.
* [`Router.InferWithErrors`](/api/WebSharper.Sitelets.Routing.Router#InferWithErrors``1) is used internally by `Sitelet.InferWithErrors`, returns only the router part as a `Router<ActionEncoding.DecodeResult<'T>>`.
* `Router.Sum` takes a sequence of `Router<'T>` values and returns a single `Router<'T>` that is capable of parsing any URL and writing actions that any of the argument router can. Attemps are made in the order of the sequence. This is what `Sitelets.Sum` uses internally.
* [`<|>`](/api/WebSharper.Sitelets.Routing.Router`1#op_LessBarGreater) operator is the same as using `Router.Sum` on two elements.
* [`Router.FromJsonParameter`](/api/WebSharper.Sitelets.Routing.Router#FromJsonParameter``1) creates a router that parses the request body by the JSON format derived from the type argument. Fails to write links.
* [`Router.FromPostParameter`](/api/WebSharper.Sitelets.Routing.Router#FromPostParameter) creates a router that parses a single named argument from the request body of a form post. Fails to write links.

### URL Parsing Helpers

To simplify the parsing of URLs in the Router, some active patterns are provided in the module [`UrlHelpers`](/api/WebSharper.Sitelets.UrlHelpers).

* Method parsing: the active patterns [`GET`, `POST`](/api/WebSharper.Sitelets.UrlHelpers#|DELETE|GET|OPTIONS|POST|PUT|TRACE|SPECIAL|), etc. match a request with the given method, and return the list of parameters (from the query for `GET`, from the body for the others) and the URI.

* [`SPLIT_BY`](/api/WebSharper.Sitelets.UrlHelpers#|SPLIT_BY|) then splits the path of a URI into fragments.

* Finally, individual fragments can be parsed with [`EOL`](/api/WebSharper.Sitelets.UrlHelpers#|EOL|), [`SLASH`](/api/WebSharper.Sitelets.UrlHelpers#|SLASH|) [`INT`](/api/WebSharper.Sitelets.UrlHelpers#|INT|), [`FLOAT`](/api/WebSharper.Sitelets.UrlHelpers#|FLOAT|), [`ALPHA`](/api/WebSharper.Sitelets.UrlHelpers#|ALPHA|), [`ALPHA_NUM`](/api/WebSharper.Sitelets.UrlHelpers#|ALPHA_NUM) and [`REGEX`](/api/WebSharper.Sitelets.UrlHelpers#|REGEX|).

The three sets of patterns above are generally used together to create a whole parser.

```fsharp
    open WebSharper.Sitelets
    open WebSharper.Sitelets.UrlHelpers
    
    type EndPoint =
        | Index
        | Stats of username: string
        | BlogArticle of id: int * slug: string

    let myRouter =
        let route request =
            match request with
            | GET (_, SPLIT_BY '/' []) ->
                Some Index
            | GET (_, SPLIT_BY '/' ["stats"; username]) ->
                Some (Stats username)
            | GET (_, SPLIT_BY '/' ["blog-article"; INT id; slug]) ->
                Some (BlogArticle (id, slug))
            | _ -> None
        let link endPoint =
            match endPoint with
            | Index ->
                System.Uri "/"
            | Stats u ->
                System.Uri ("/stats/" + u)
            | BlogArticle (id, slug) ->
                System.Uri (sprintf "/blog-article/%i/%s" id slug)
            |> Some
        Router.New route link
```

### Controllers

If an incoming request can be mapped to an action by the router, it is passed on to the controller. The job of the controller is to map actions to content. Here is an example of a controller handling actions of the `EndPoint` type defined above.

```fsharp
type EndPoint = | Page1 | Page2

let MyController : Controller<EndPoint> =
    {
        Handle = fun endPoint ->
            match endPoint with
            | EndPoint.Page1  -> MyContent.Page1
            | EndPoint.Page2  -> MyContent.Page2
    }
```

Finally, the router and the controller components are combined into a sitelet:

```fsharp
let MySitelet : Sitelet<EndPoint> =
    {
        Router = MyRouter
        Controller = MyController
    }
```
