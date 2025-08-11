---
title: Routing requests and serving content with sitelets
---

Sitelets are WebSharper's primary way to create server-side content. They provide a composable abstraction to route requests and generate responses like HTML pages or JSON.

The key concept is the **endpoint**: a value of a user-defined type that represents a specific request route.
Sitelets parse incoming requests into these endpoint values, and then generate the appropriate content based on the endpoint.
Link generation takes an endpoint value and produces a URL, which is the inverse of route parsing.
By encapsulating these concepts, sitelets allow you to define a website's URL scheme, content, and linking in a type-safe manner.

In addition, if the router part is defined separately, it can be used on the client side too, for client-side routing and linking.

Below is a minimal example of a complete site, serving one HTML page:

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

    let MySampleWebsite : Sitelet<EndPoint> =
        Sitelet.Content "/index" EndPoint.Index IndexContent
```

First, a custom endpoint type is defined. It is used for linking requests to content within your sitelet. Here, you only need one endpoint, `EndPoint.Index`, corresponding to your only page.

The content of the index page is defined as a `Content.Page`, where the body consists of a server-side HTML element that computes and displays the current time within an `<h1>` tag.

The `MySampleWebsite` value has type `Sitelet<EndPoint>`. It defines a complete website: the URL scheme, the `EndPoint` value corresponding to each served URL (only one in this case), and the content to serve for each endpoint. It uses the `Sitelet.Content` operator to construct a sitelet for the Index endpoint, associating it with the `/index` URL and serving `IndexContent` as a response.

## Routing

WebSharper sitelets abstract away URLs and request parsing by using an endpoint type that represents the different HTTP endpoints available in a website. For example, a site's URL scheme can be represented by the following endpoint type:

```fsharp
type EndPoint =
    | Index
    | Stats of username: string
    | BlogArticle of id: int * slug: string
```

Based on this, a sitelet is a value that represents the following mappings:

* Mapping from requests to endpoints. A sitelet is able to parse a URL such as `/blog/1243/some-article-slug` into the endpoint value `BlogArticle (id = 1243, slug = "some-article-slug")`. More advanced definitions can even parse query parameters, JSON bodies or posted forms.

* Mapping from endpoints to URLs. This allows you to have internal links that are verified by the type system, instead of writing URLs by hand and being at the mercy of a typo or a change in the URL scheme. You can read more on this [in the Content doc](content#using-the-context).

* Mapping from endpoints to content. Once a request has been parsed, this determines what content (e.g., HTML, JSON, or other formats) must be returned to the client.

A number of primitives are available to create and compose sitelets.

### Trivial sitelets

Two helpers exist for creating a sitelet with a trivial router that only handles requests on the root.

* `Application.Text` takes just a `Context<_> -> string` function and creates a sitelet that serves the result string as a text response.
* `Application.SinglePage` takes a `Context<_> -> Async<Content<_>>` function and creates a sitelet that serves the returned content.

<a name="sitelet-infer"></a>
### Sitelet.Infer

The easiest way to create a more complex sitelet is to automatically generate URLs from the shape of your endpoint type using `Sitelet.Infer`, also aliased as `Application.MultiPage`. This function parses slash-separated path segments into the corresponding `EndPoint` value, and lets you match this endpoint and return the appropriate content. Here is an example sitelet using `Infer`:

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

The above sitelet accepts URLs with the following shape:

```
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

* `[<Method("GET", "POST", ...)>]` on a union case indicates which methods are parsed by this endpoint. Without this attribute, all methods are accepted.

    ```fsharp
    type EndPoint =
        | [<Method "POST">] PostArticle of id: int

    // Accepted Request:    POST /PostArticle/12
    // Parsed Endpoint:     PostArticle 12
    // Returned Content:    (determined by Sitelet.Infer)
    ```

* `[<EndPoint "/string">]` on a union case indicates the identifying segment.

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

* `[<Query("arg1", "arg2", ...)>]` on a union case indicates that the fields with the given names must be parsed as GET query parameters instead of path segments. The value of this field must be either a base type (number, string) or an option of a base type (in which case the parameter is optional).

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

* `[<Json "arg">]` on a union case indicates that the field with the given name must be parsed as JSON from the body of the request. If an endpoint type contains several `[<Json>]` fields, a runtime error is thrown.

    [Learn more about JSON parsing.](json)

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

* `[<FormData("arg1", "arg2", ...)>]` on a union case indicates that the fields with the given names must be parsed from the body as form data (`application/x-www-form-urlencoded` or `multipart/form-data`) instead of path segments. The value of this field must be either a base type (number, string) or an option of a base type (in which case the parameter is optional).

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

* `[<DateTimeFormat(string)>]` on a record field or named union case field of type `System.DateTime` indicates the date format to use. Be careful as some characters are not valid in URLs; in particular, the ISO 8601 round-trip format (`"o"` format) cannot be used because it uses the character `:`.

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

* `[<Wildcard>]` on a union case indicates that the last argument represents the remainder of the url's path. That argument can be a `list<'T>`, a `'T[]`, or a `string`.

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

By default, `Sitelet.Infer` ignores requests that it fails to parse, in order to give potential other middleware a chance to respond to the request. However, if you want to send a custom response for badly-formatted requests, you can use `Sitelet.InferWithErrors` instead. This function wraps the parsed request in the `ParseRequestResult<'EndPoint>` union. Here are the cases you can match against:

* `ParseRequestResult.Success of 'EndPoint`: The request was successfully parsed.

* `ParseRequestResult.InvalidMethod of 'EndPoint * method: string`: An endpoint was successfully parsed but with the given wrong HTTP method.

* `ParseRequestResult.MissingQueryParameter of 'EndPoint * name: string`: The URL path was successfully parsed but a mandatory query parameter with the given name was missing. The endpoint value contains a default value (`Unchecked.defaultof<_>`) where the query parameter value should be.

* `ParseRequestResult.InvalidJson of 'EndPoint`: The URL was successfully parsed but the JSON body wasn't. The endpoint value contains a default value (`Unchecked.defaultof<_>`) where the JSON-decoded value should be.

* `ParseRequestResult.MissingFormData of 'EndPoint * name: string`: The URL was successfully parsed but a form data parameter with the given name was missing or wrongly formatted. The endpoint value contains a default value (`Unchecked.defaultof<_>`) where the form body-decoded value should be.

If multiple errors of these kinds occur, only the last one is reported.

If the URL path isn't matched, then the request falls through as with `Sitelet.Infer`.

```fsharp
open WebSharper.Sitelets

module SampleSite =
    open WebSharper.Sitelets

    type EndPoint =
    | [<Method "GET"; Query "page">] Articles of page: int

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

* `Sitelet.Empty` creates a sitelet which does not recognize any URLs.

* `Sitelet.Content`, as shown in the first example, builds a sitelet that accepts a single URL and maps it to a given endpoint and content.

    ```fsharp
    Sitelet.Content "/index" Index IndexContent

    // Accepted Request:    GET /index
    // Parsed Endpoint:     Index
    // Returned Content:    (value of IndexContent : Content<EndPoint>)
    ```

* `Sitelet.Sum` takes a sequence of sitelets and tries them in order until one of them accepts the URL. It is generally used to combine a list of `Sitelet.Content`s.

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

* `+` takes two sitelets and tries them in order. `s1 + s2` is equivalent to `Sitelet.Sum [s1; s2]`.

    ```fsharp
    Sitelet.Content "/index" Index IndexContent
    +
    Sitelet.Content "/about" About AboutContent

    // Same as above.
    ```

For the mathematically inclined, the functions `Sitelet.Empty` and `+` make sitelets a monoid. Note that it is non-commutative: if a URL is accepted by both sitelets, the left one will be chosen to handle the request.

* `Sitelet.Shift` takes a sitelet and shifts it by a path segment.

    ```fsharp
    Sitelet.Content "index" Index IndexContent
    |> Sitelet.Shift "folder"

    // Accepted Request:    GET /folder/index
    // Parsed Endpoint:     Index
    // Returned Content:    (value of IndexContent : Content<EndPoint>)
    ```

* `Sitelet.Folder` takes a sequence of sitelets and shifts them by a path segment. It is effectively a combination of `Sum` and `Shift`.

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

* `Sitelet.Protect` creates protected content, i.e.  content only available for authenticated users:

    ```fsharp
    module Sitelet =
        type Filter<'EndPoint> =
            {
                VerifyUser : string -> bool;
                LoginRedirect : 'EndPoint -> 'EndPoint
            }

    val Protect : Filter<'EndPoint> -> Sitelet<'EndPoint> -> Sitelet<'EndPoint>
    ```

    Given a filter value and a sitelet, `Protect` returns a new sitelet that requires a logged in user that passes the `VerifyUser` predicate, specified by the filter.  If the user is not logged in, or the predicate returns false, the request is redirected to the endpoint specified by the `LoginRedirect` function specified by the filter. [See here how to log users in and out.](content#using-the-context)

* `Sitelet.Map` converts a sitelet to a different endpoint type using mapping functions in both directions.

    ```fsharp
    type EndPoint = Article of string

    let s : Sitelet<string> = Sitelet.Infer sContent

    let s2 : Sitelet<EndPoint> = Sitelet.Map Article (fun (Article a) -> a) s
    ```

* `Sitelet.Embed` similarly converts a sitelet to a different endpoint type, but with a partial mapping function: the input endpoint type represents only a subset of the result endpoint type.

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

* `Sitelet.EmbedInUnion` is a simpler version of `Sitelet.Embed` when the mapping function is a union case constructor.

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

* `Sitelet.InferPartial` is equivalent to combining `Sitelet.Infer` and `Sitelet.Embed`, except the context passed to the infer function is of the outer endpoint type instead of the inner. For example, in the example for `Sitelet.Embed` above, the function `articleContent` receives a `Context<string>` and can therefore only create links to articles. Whereas with `InferPartial`, it receives a full `Context<EndPoint>` and can create links to `Index`.

    ```fsharp
    type EndPoint =
        | Index
        | Article of string

    let index : Sitelet<EndPoint> = Sitelet.Content "/" Index indexContent
    let article : Sitelet<EndPoint> =
        Sitelet.InferPartial Article (function Article a -> Some a | _ -> None) articleContent
    let fullSitelet = Sitelet.Sum [ index; article ]
    ```

* `Sitelet.InferPartialInUnion` is a simpler version of `Sitelet.InferPartial` when the mapping function is a union case constructor.

    ```fsharp
    type EndPoint =
        | Index
        | Article of string

    let index : Sitelet<EndPoint> = Sitelet.Content "/" Index indexContent
    let article : Sitelet<EndPoint> = Sitelet.InferPartialInUnion <@ Article @> articleContent
    let fullSitelet = Sitelet.Sum [ index; article ]
    ```

* `Sitelet.CorsContent` is a helper for creating a sitelet that serves content from a single endpoint with CORS (Cross-Origin Resource Sharing) checking enabled. It takes a URL, an endpoint, and content, and returns a sitelet that serves the content with the appropriate CORS headers.

```fsharp
Sitelet.CorsContent "/index" (Cors.Of Index) 
    (fun allows -> { allows with
        Origins = ["*"]
        Headers = ["Content-Type"; "Authorization"] })
    (fun ctx -> IndexContent)

// Accepted Request:    GET /index
// Parsed Endpoint:     { DefaultAllows = ...; EndPoint = Some Index }
// Returned Content:    (value of IndexContent : Content<EndPoint>)
// With Headers:        Access-Control-Allow-Origin: *
                        Access-Control-Allow-Methods: GET
                        Access-Control-Allow-Headers: Content-Type, Authorization
```