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

```csharp
using System;
using System.Threading.Tasks;
using WebSharper.Sitelets;
using static WebSharper.UI.CSharp.Html;

namespace MyWebsite
{
    /// Endpoint for the single URL "/".
    [EndPoint("/")]
    public class Index { }

    public class SampleSite
    {
        /// The content returned when receiving an Index request.
        public static Task<Content> IndexContent(Context ctx)
        {
            var time = DateTime.Now.ToString();
            return Content.Page(
                Title: "Index",
                Body: h1("Current time: ", time)
            );
        }

        /// Defines the website's responses based on the parsed Endpoint.
        [Website]
        public static Sitelet<Index> MySampleWebsite =>            
            Sitelet.Content("/index", new Index(), IndexContent)
    }
}
```

First, custom endpoint types are defined. They are used for linking requests to content within your sitelet. Here, you only need one endpoint, `Index`, corresponding to your only page.

The content of the index page is defined as a `Content.Page`, where the body consists of a server side HTML element.  Here the current time is computed and displayed within an `<h1>` tag.

The `MySampleWebsite` value has type `Sitelet<object>`. It defines a complete website: the URL scheme, the `EndPoint` value corresponding to each served URL (only one in this case), and the content to serve for each endpoint. It uses the `SiteletBuilder` class to construct a sitelet for the Index endpoint, associating it with the `/index` URL and serving `IndexContent` as a response.

`MySampleWebsite` is annotated with the attribute `[Website]` to indicate that this is the sitelet that should be served.

## Routing

WebSharper Sitelets abstract away URLs and request parsing by using an endpoint type that represents the different HTTP endpoints available in a website. For example, a site's URL scheme can be represented by the following endpoint types:

```csharp
[EndPoint("/")]
public class Index { }

[EndPoint("/stats/{username}")]
public class Stats
{
    public string username;
}

[EndPoint("/blog/{id}/{slug}")]
public class BlogArticle
{
    public int id;
    public string slug;
}
```

Based on this, a Sitelet is a value that represents the following mappings:

* Mapping from requests to endpoints. A Sitelet is able to parse a URL such as `/blog/1243/some-article-slug` into the endpoint value `new BlogArticle {id = 1243, slug = "some-article-slug"}`. More advanced definitions can even parse query parameters, JSON bodies or posted forms.

* Mapping from endpoints to URLs. This allows you to have internal links that are verified by the type system, instead of writing URLs by hand and being at the mercy of a typo or a change in the URL scheme. You can read more on this [in the "Context" section](#context).

* Mapping from endpoints to content. Once a request has been parsed, this determines what content (HTML or other) must be returned to the client.

### Trivial Sitelets

Two helpers exist for creating a Sitelet with a trivial router: only handling requests on the root.

* `Application.Text` takes a `Func<Context, string>` function and creates a Sitelet that serves the result string as a text response.
* `Application.SinglePage`takes a `Func<Context, Task<Content>>` function and creates a Sitelet that serves the returned content.

### SiteletBuilder

A handy way to create a Sitelet is by using the `SiteletBuilder` class. It functions conceptually similar to `StringBuilder`, you can chain methods to assemble a final value:
* First, create an instance of `SiteletBuilder()`;
* Then, add your mappings using `.With<T>(...)` method calls;
* Finally, use `.Install()` to return your constructed `Sitelet`.

For example, using the endpoint types defined [in the above section](#sitelet-routing), you can create the following Sitelet:

```csharp
[Website]
public static Sitelet<object> MySampleWebsite =>
    new SiteletBuilder()
        .With<Index>((ctx, endpoint) =>
            Content.Page(
                Title: "Welcome!",
                Body: h1("Index page")
            )
        )
        .With<Stats>((ctx, endpoint) =>
            Content.Page(
                Body: doc("Stats for ", endpoint.username)
            )
        )
        .With<BlogArticle>((ctx, endpoint) =>
            Content.Page(
                Body: doc($"Article id {endpoint.id}, slug {endpoint.slug}")
            )
        )
        .Install();
```

The above sitelets accepts URLs with the following shape:

```xml
Accepted Request:    GET /Index
Parsed Endpoint:     new Index()
Returned Content:    <!DOCTYPE html>
                     <html>
                         <head><title>Welcome!</title></head>
                         <body>
                             <h1>Index page</h1>
                         </body>
                     </html>

Accepted Request:    GET /Stats/someUser
Parsed Endpoint:     new Stats { username = "someUser" }
Returned Content:    <!DOCTYPE html>
                     <html>
                         <head></head>
                         <body>
                             Stats for someUser
                         </body>
                     </html>

Accepted Request:    GET /BlogArticle/1423/some-article-slug
Parsed Endpoint:     new BlogArticle { id = 1423, slug = "some-article-slug" }
Returned Content:    <!DOCTYPE html>
                     <html>
                         <head></head>
                         <body>
                             Article id 1423, slug some-article-slug
                         </body>
                     </html>
```

It is also possible to create an endpoint for a specific URL, without associating an endpoint type to it:

```csharp
new SiteletBuilder()
    .With("/static-url", ctx =>
        Content.Text("Replying to /static-url")
    )

// Accepted Request:    GET /static-url
// Returned Content:    Replying to /static-url
```

### Defining EndPoints

The following types can be used as endpoints:

* Numbers and strings are encoded as a single path segment.

```csharp
SiteletBuilder().With<string>(/* ... */)

// Accepted Request:    GET /abc
// Parsed Endpoint:     "abc"
// Returned Content:    (determined by .With())

SiteletBuilder().With<int>(/* ... */)

// Accepted Request:    GET /1423
// Parsed Endpoint:     1423
// Returned Content:    (determined by .With())
```

* Arrays are encoded as a number representing the length, followed by each element.

```csharp

SiteletBuilder().With<string[]>(/* ... */)

// Accepted Request:    GET /2/abc/def
// Parsed Endpoint:     new string[] { "abc", "def" }
// Returned Content:    (determined by .With())
```

* `System.Tuple`s, `ValueTuple`s and objects are encoded with their fields as consecutive path segments.

```csharp
SiteletBuilder().With<(int, string)>(/* ... */)

// Accepted Request:    GET /1/abc
// Parsed Endpoint:     new Tuple<int, string>(1, "abc")
// Returned Content:    (determined by .With())

class T
{
    int Number;
    string Name;
}

// Accepted Request:    GET /1/abc
// Parsed Endpoint:     new T { Number = 1, Name = "abc" }
// Returned Content:    (determined by .With())
```

* Objects with an `[EndPoint]` attribute are prefixed with the given path segment.

```csharp
[EndPoint("/test/{number}/{name}")]
class T
{
    int number;
    string name;
}

SiteletBuilder().With<T>(/* ... */)

// Accepted Request:    GET /test/1/abc
// Parsed Endpoint:     new EndPoint { number = 1, name = "abc" }
// Returned Content:    (determined by .With())
```

* Enumerations are encoded as their underlying type.

```csharp
SiteletBuilder().With<System.IO.FileAccess>(/* ... */)

// Accepted Request:    GET /3
// Parsed Endpoint:     System.IO.FileAccess.ReadWrite
// Returned Content:    (determined by .With())
```

* `System.DateTime` is serialized with the format `yyyy-MM-dd-HH.mm.ss` by default. Use `[DateTimeFormat(string)]` on a field to customize it. Be careful as some characters are not valid in URLs; in particular, the ISO 8601 round-trip format (`"o"` format) cannot be used because it uses the character `:`.

```csharp
SiteletBuilder().With<DateTime>(/* ... */)

// Accepted Request:    GET /2015-03-24-15.05.32
// Parsed Endpoint:     System.DateTime(2015,3,24,15,5,32)
// Returned Content:    (determined by .With())

class T
{
    [DateTimeFormat("yyy-MM-dd")]
    DateTime date;
}

SiteletBuilder().With<T>(/* ... */)

// Accepted Request:    GET /2015-03-24
// Parsed Endpoint:     System.DateTime(2015,3,24)
// Returned Content:    (determined by .With())
```

* The attribute `[Method("GET", "POST", ...)]` on a class indicates which methods are accepted by this endpoint. Without this attribute, all methods are accepted.

```csharp
[Method("POST")]
class PostArticle
{
    int id;
}

SiteletBuilder().With<PostArticle>(/* ... */)

// Accepted Request:    POST /article/12
// Parsed Endpoint:     new PostArticle { id = 12 }
// Returned Content:    (determined by .With())
```

* If an endpoint accepts only one method, then a more concise way to specify it is directly in the `[EndPoint]` attribute:

```csharp
[EndPoint("POST /article/{id}")]
class PostArticle
{
    int id;
}

SiteletBuilder().With<PostArticle>(/* ... */)

// Accepted Request:    POST /article/12
// Parsed Endpoint:     new PostArticle { id = 12 }
// Returned Content:    (determined by .With())
```

* A common trick is to use `[EndPoint("GET /")]` on a field-less class to indicate the home page.

```csharp
[EndPoint("/")]
class Home { }

SiteletBuilder().With<Home>(/* ... */)

// Accepted Request:    GET /
// Parsed Endpoint:     new Home()
// Returned Content:    (determined by .With())
```

* If several classes have the same `[EndPoint]`, then parsing tries them in the order in which they are passed to `.With()` until one of them matches:

```csharp
[EndPoint("GET /blog")]
class AllArticles { }

[EndPoint("GET /blog/{id}")]
class ArticleById
{
    int id;
}

[EndPoint("GET /blog/{slug}")]
class ArticleBySlug
{
    string slug;
}

SiteletBuilder()
    .With<AllArticles>(/* ... */)
    .With<ArticleById>(/* ... */)
    .With<ArticleBySlug>(/* ... */)

// Accepted Request:    GET /blog
// Parsed Endpoint:     new AllArticles()
// Returned Content:    (determined by .With())
//
// Accepted Request:    GET /blog/123
// Parsed Endpoint:     new ArticleById { id = 123 }
// Returned Content:    (determined by .With())
//
// Accepted Request:    GET /blog/my-article
// Parsed Endpoint:     new ArticleBySlug { slug = "my-article" }
// Returned Content:    (determined by .With())
```

* `[Query]` on a field indicates that this field must be parsed as a GET query parameter instead of a path segment. The value of this field must be either a base type (number, string) or an `Nullable` of a base type (in which case the parameter is optional).

```csharp
[EndPoint]
class Article
{
    [Query]
    int id;
    [Query]
    string slug;
}

SiteletBuilder().With<Article>(/* ... */)

// Accepted Request:    GET /article?id=1423&slug=some-article-slug
// Parsed Endpoint:     new Article { id = 1423, slug = "some-article-slug" }
// Returned Content:    (determined by .With())
//
// Accepted Request:    GET /article?id=1423
// Parsed Endpoint:     new Article { id = 1423, slug = null }
// Returned Content:    (determined by .With())
```

* You can of course mix Query and non-Query parameters.

```csharp
[EndPoint("{id}")]
class Article
{
    int id;
    [Query]
    string slug;
}

SiteletBuilder().With<Article>(/* ... */)

// Accepted Request:    GET /article/1423?slug=some-article-slug
// Parsed Endpoint:     new Article { id = 1423, slug = Some "some-article-slug" }
// Returned Content:    (determined by .With())
```

<a name="json-request"></a>

* `[Json]` on a field indicates that it must be parsed as JSON from the body of the request. If an endpoint type contains several `[Json]` fields, a runtime error is thrown.

    [Learn more about JSON parsing.](Json.md)

```csharp
[EndPoint("POST /article/{id}")]
class PostArticle
{
    int id;
    [Json]
    PostArticleData data;
}

[Serializable]
class PostArticleData
{
    string slug;
    string title;
}

SiteletBuilder().With<PostArticle>(/* ... */)

// Accepted Request:    POST /article/1423
//
//                      {"slug": "some-blog-post", "title": "Some blog post!"}
//
// Parsed Endpoint:     new PostArticle {
//                          id = 1423,
//                          data = new PostArticleData {
//                              slug = "some-blog-post",
//                              title = "Some blog post!" } }
// Returned Content:    (determined by .With())
```

* `[Wildcard]` on a field indicates that it represents the remainder of the url's path. That field can be a `T[]` or a `string`. If an endpoint type contains several `[Wildcard]` fields, a runtime error is thrown.

```csharp
[EndPoint("/articles/{id}")]
class Articles
{
    int pageId;
    [Wildcard]
    string[] tags;
}

[EndPoint("/articles")]
class Articles2
{
    [Wildcard]
    (int, string) tags;
}

[EndPoint("/file")]
class File
{
    [Wildcard]
    string file;
}

SiteletBuilder()
    .With<Articles>(/* ... */)
    .With<Articles2>(/* ... */)
    .With<File>(/* ... */)

// Accepted Request:    GET /articles/123/csharp/websharper
// Parsed Endpoint:     new Articles {
//                          pageId = 123,
//                          tags = new[] { "csharp", "websharper" } }
// Returned Content:    (determined by .With())
//
// Accepted Request:    GET /articles/123/csharp/456/websharper
// Parsed Endpoint:     new Articles2 { tags = new[] {
//                          (123, "csharp"), (456, "websharper") } }
// Returned Content:    (determined by .With())
//
// Accepted Request:    GET /file/css/main.css
// Parsed Endpoint:     new File { file = "css/main.css" }
// Returned Content:    (determined by .With())
```

<a name="sitelet-combinators"></a>
### Other Constructors and Combinators

The following functions are available to build simple sitelets or compose more complex sitelets out of simple ones:

* `Sitelet.Empty<T>()` creates a Sitelet which does not recognize any URLs.

* `Sitelet.Content`, builds a sitelet that accepts a single URL and maps it to a given endpoint and content.

```csharp
Sitelet.Content("/index", new Index(), IndexContent)

// Accepted Request:    GET /index
// Parsed Endpoint:     Index
// Returned Content:    (value of IndexContent : Content<EndPoint>)
```

* `Sitelet.Infer` is used by `SiteletBuilder.With` internally. Using `Sitelet.Infer<EndPoint>(CreateContent)` is equal to `new SiteletBuilder().` 

* `Sitelet.Sum` takes any number of Sitelets (given as parameters, or as an `IEnumerable<Sitelet<T>>`) and tries them in order until one of them accepts the URL. It is generally used to combine a list of `Sitelet.Content`s.

  The following sitelet accepts `/index` and `/about`:

```csharp
Sitelet.Sum(
    Sitelet.Content("/index", new Index(), IndexContent),
    Sitelet.Content("/about", new About(), AboutContent)
)

// Accepted Request:    GET /index
// Parsed Endpoint:     Index
// Returned Content:    (value of IndexContent : Content<EndPoint>)
//
// Accepted Request:    GET /about
// Parsed Endpoint:     About
// Returned Content:    (value of AboutContent : Content<EndPoint>)
```

* `+` operator can be used on two Sitelets to try them in order. `s1 + s2` is equivalent to `Sitelet.Sum(s1, s2)`.

```csharp
Sitelet.Content("/index", new Index(), IndexContent)
| Sitelet.Content("/about", new About(), AboutContent)

// Same as above.
```

For the mathematically enclined, the functions `Sitelet.Empty` and `+` make sitelets a monoid. Note that it is non-commutative: if a URL is accepted by both sitelets, the left one will be chosen to handle the request.

* `.Shift` takes a Sitelet and shifts it by a path segment.

```csharp
Sitelet.Content("/index", new Index(), IndexContent).Shift("folder")

// Accepted Request:    GET /folder/index
// Parsed Endpoint:     Index
// Returned Content:    (value of IndexContent : Content<EndPoint>)
```

* `Sitelet.Folder` takes a sequence of Sitelets and shifts them by a path segment. It is effectively a combination of `Sum` and `Shift`.

```csharp
Sitelet.Folder("folder",
    Sitelet.Content("/index", new Index(), IndexContent),
    Sitelet.Content("/about", new About(), AboutContent)
)

// Accepted Request:    GET /folder/index
// Parsed Endpoint:     Index
// Returned Content:    (value of IndexContent : Content<EndPoint>)
//
// Accepted Request:    GET /folder/about
// Parsed Endpoint:     About
// Returned Content:    (value of AboutContent : Content<EndPoint>)
```

* `.Protect` creates protected content, i.e. content only available for authenticated users:

```csharp
Sitelet.Content("/about", new About(), AboutContent)
    .Protect(userName => VerifyUser(userName), LoginRedirect)
```

Given a predicate on the user name and a `Func<EndPoint, EndPoint>`, `Protect` returns a new sitelet that requires a logged in user that passes the givem predicate. If the user is not logged in, or the predicate returns false, the request is redirected to the action specified by the `LoginRedirect` function. [See here how to log users in and out.](#context)

* `.Map` converts a Sitelet to a different endpoint type using mapping functions in both directions.

```csharp
[EndPoint("/article/{Title}")]
public class Article {
    public string Title;
}

Sitelet.Infer<string>(ArticleContent).Map(t => new Article() { Title = t }, a => a.Title)
```

The mapping functions can also be partial, so one or both of them can return `null` on some inputs. The only important thing is that the two functions are the inverse of each other on valid values, so `decode(encode(x)) = x` for all values of `x`. Also, `null` should never be a valid endpoint value for this to work.

```csharp
[EndPoint("/article/{Title}")]
public class Article : Home {
    public string Title;
}

Sitelet.Infer<string>(ArticleContent).Map(t => new Article() { Title = t }, Home p => p is Article ? (a as Article).Title : null)
```

## Content

Content describes the response to send back to the client: HTTP status, headers and body. Content is always worked with asynchronously: all the constructors and combinators described below take and return values of type `Task<Content>`.

### Creating Content

There are several functions that create different types of content, including ordinary text (`Content.Text`), file (`Content.File`), HTML page (`Content.Page`), JSON (`Content.Json`), any custom content (`Content.Custom`), and HTTP error codes and redirects.

#### Content.Text

The simplest response is plain text content, created by passing a string to `Content.Text`.

```csharp
new SiteletBuilder()
    .With<T>((ctx, endpoint) =>
        Content.Text("This is the response body.")
    )
```

#### Content.File

You can serve files using `Content.File`.  Optionally, you can set the content type returned for the file response and whether file access is allowed outside of the web root:

```csharp
new SiteletBuilder()
    .With<T>((ctx, endpoint) =>
        Content.File("../Main.fs",
            AllowOutsideRootFolder: true,
            ContentType: "text/plain")
    )
```

#### Content.Page

You can return full HTML pages, with managed dependencies using `Content.Page`. Here is a simple example:

```csharp
using static WebSharper.UI.CSharp.Html;

new SiteletBuilder()
    .With<T>((ctx, endpoint) =>
        Content.Page(
            Title: "Welcome!",
            Head: link(attr.href("/css/style.css"), attr.rel("stylesheet")),
            Body: doc(
                h1("Welcome to my site."),
                p("It's great, isn't it?")
            )
        )
    )
```

The optional named arguments `Title`, `Head`, `Body` and `Doctype` set the corresponding elements of the HTML page. To learn how to create HTML elements for `Head` and `Body`, see [the HTML combinators documentation](HtmlCombinators.md).

<a name="json-response"></a>
#### Content.Json

If you are creating a web API, then Sitelets can automatically generate JSON content for you based on the type of your data. Simply pass your value to `Content.Json`, and WebSharper will serialize it. The format is the same as when parsing requests. [See here for more information about the JSON format.](Json.md)

```csharp
[EndPoint("/article/{id}")]
class GetArticle
{
    int id;
}

[Serializable]
class GetArticleResponse
{
    int id;
    string slug;
    string title;
}

new SiteletBuilder()
    .With<GetArticle>((ctx, endpoint) =>
        Content.Json(
            new GetArticleResponse {
                id = endpoint.id,
                slug = "some-blog-article",
                title = "Some blog article!"
            }
        )
    )
    .Install()

// Accepted Request:    GET /article/1423
// Parsed Endpoint:     new GetArticle { id = 1423 }
// Returned Content:    {"id": 1423, "slug": "some-blog-article", "title": "Some blog article!"}
```

#### Content.Custom

`Content.Custom` can be used to output any type of content. It takes three optional named arguments that corresponds to the aforementioned elements of the response:

* `Status` is the HTTP status code. It can be created using the function `Http.Status.Custom`, or you can use one of the predefined statuses such as `Http.Status.Forbidden`.

* `Headers` is the HTTP headers. You can create them using the function `Http.Header.Custom`.

* `WriteBody` writes the response body.

```csharp
new SiteletBuilder()
    .With("/someTextFile.txt", ctx =>
        Content.Custom(
            Status: Http.Status.Ok,
            Headers: new[] { Http.Header.Custom("Content-Type", "text/plain") },
            WriteBody: stream =>
            {
                using (var w = new System.IO.StreamWriter(stream))
                {
                    w.Write("The contents of the text file.");
                }
            }
        )
    )

// Accepted Request:    GET /someTextFile.txt
// Returned Content:    The contents of the text file.
```

### Helpers

In addition to the four standard Content families above, the `Content` module contains a few helper functions.

* Redirection:

```csharp
static class Content {
    /// Permanently redirect to an endpoint. (HTTP status code 301)
    static Task<Content> RedirectPermanent(object endpoint);

    /// Permanently redirect to a URL. (HTTP status code 301)
    static Task<Content> RedirectPermanentToUrl(string url);

    /// Temporarily redirect to an endpoint. (HTTP status code 307)
    static Task<Content> RedirectTemporary(object endpoint);

    /// Temporarily redirect to a URL. (HTTP status code 307)
    static Task<Content> RedirectTemporaryToUrl(string url);
}
```

* Response mapping: if you want to return HTML or JSON content, but further customize the HTTP response, then you can use one of the following:

```csharp
static class Content {
    /// Set the HTTP status of a response.
    static Task<Content> SetStatus(this Task<Content> content, Http.Status status);

    /// Add headers to a response.
    static Task<Content> WithHeaders(this Task<Content> content, IEnumerable<Header> headers);

    /// Replace the headers of a response.
    static Task<Content> SetHeaders(this Task<Content> content, IEnumerable<Header> headers);
}

// Example use
new SiteletBuilder()
    .With("/", ctx =>
        Content.Page(
            Title: "No entrance!",
            Body: text("Oops! You're not supposed to be here."))
            .SetStatus(Http.Status.Forbidden)
            .WithHeaders(new[] { Http.Header.Custom("Content-Language", "en") })
    )
```

<a name="context"></a>
## Using the Context

The method `SiteletBuilder.With()` provides a context of type `Context`. This context can be used for several purposes; the most important are creating internal links and managing user sessions.

<a name="linking"></a>
### Creating links

Since every accepted URL is uniquely mapped to an action value, it is also possible to generate internal links from an action value. For this, you can use the function `context.Link`.

```csharp
[EndPoint("/article/{id}/{slug}")]
class Article
{
    public int id;
    public string slug;
}

new SiteletBuilder()
    .With<Article>((context, endpoint) =>
        Content.Page(
            Title: "Welcome!",
            Body: doc(
                h1("Index page"),
                a(attr.href(context.Link(new Article { id = 1423, slug = "some-article-slug" })),
                    "Go to some article"),
                br(),
                a(attr.href(context.ResolveUrl("~/Page2.html")), "Go to page 2")
            )
        )
    )
```

Note how `context.Link` is used in order to resolve the URL to the `Article` endpoint.  Endpoint URLs are always constructed relative to the application root, whether the application is deployed as a standalone website or in a virtual folder.  `context.ResolveUrl` helps to manually construct application-relative URLs to resources that do not map to sitelet endpoints.

### Managing User Sessions

`Context<'T>` can be used to access the currently logged in user. The member `UserSession` has the following members:

* `Task LoginUser(string username, bool persistent = false)`  
  `Task LoginUser(string username, System.TimeSpan duration)`

    Logs in the user with the given username. This sets a cookie that is uniquely associated with this username. The second parameter determines the expiration of the login:

    * `LoginUser("username")` creates a cookie that expires with the user's browser session.
    
    * `LoginUser("username", persistent: true)` creates a cookie that lasts indefinitely.
    
    * `LoginUser("username", duration: d)` creates a cookie that expires after the given duration.
    
    Example:
    
    ```csharp
    public async Task<Content<EndPoint>> LoggedInPage(Context<EndPoint> context, string username) 
    {
        // We're assuming here that the login is successful,
        // eg you have verified a password against a database.
        await context.UserSession.LoginUser(username, 
                duration: TimeSpan.FromDays(30.));
        return Content.Page(
            Title: "Welcome!",
            Body: text($"Welcome, {username}!")
        );
    } 
    ```

* `Task<string> GetLoggedInUser()`

    Retrieves the currently logged in user's username, or `null` if the user is not logged in.
    
    Example:
    
    ```csharp
    public async Task<Content<EndPoint>> HomePage(Context<EndPoint> context) 
    {
        var username = await context.UserSession.GetLoggedInUser();
        return Content.Page(
            Title: "Welcome!",
            Body:
                text (
                    username is null
                    ? "Welcome, stranger!"
                    : $"Welcome back, {username}!"
                )
        );
    }
    ```

* `Task Logout()`

    Logs the user out.
    
    Example:
    
    ```csharp
    public async Content<EndPoint> Logout(Context<EndPoint> context)
    {
        await context.UserSession.Logout();
        return Content.RedirectTemporary(new Home());
    }
    ```

The implementation of these functions relies on cookies and thus requires that the browser has enabled cookies.

### Other Context members

* `context.ApplicationPath` returns the web root of the application. Most of the time this will be `"/"`, unless you use a feature such as an ASP.NET virtual directory.

* `context.Request` returns the `Http.Request` being responded to. This is useful to access elements such as HTTP headers, posted files or cookies.

* `context.ResolveUrl` resolves links to static pages in your application. A leading `~/` character is translated to the `ApplicationPath` described above.

* `context.RootFolder` returns the physical folder on the server machine from which the application is running.

* `context.Environment` returns an `IDictionary<string, object>` which depends on the host on which WebSharper is running.

    * When running on ASP.NET, `context.Environment["HttpContext"]` contains the `System.Web.HttpContextBase` for the current request.
    
    * When running on OWIN, `context.Environment` is the OWIN environment.

### Routers

The router component of a sitelet can be constructed in multiple ways. The main options are: 

* Declaratively, using `InferRouter.Router.Infer` which is also used internally by `Sitelets.Infer`. The main advantage of creating a router value separately, is that it can be also be added a `[JavaScript]` attribute, so that the client can generate links from endpoint values too. `WebSharper.UI` also contains functionality for client-side routing, making it possible to handle all or a subset of internal links without browser navigation. So sharing the router abstraction between client and server means that server can generate links that the client will handle and vice versa.
* Manually, by using combinators to build up larger routers from elementary `Router` values or inferred ones. You can use this to further customize routing logic if you want an URL schema that is not fitting default inferred URL shapes, or add additional URLs to handle (e. g. for keeping compatibility with old links).
* Implementing the `IRouter` interface. This is the most universal way, but has less options for composition.

The following example shows how you can create a router of type `WebSharper.Sitelets.IRouter<EndPoint>` by writing the two mappings manually:

```csharp
using WebSharper.Sitelets;

public enum EndPoint { Page1, Page2 }

public class MyRouter : IRouter<EndPoint>
{
    public EndPoint Route(Http.Request req) 
    {
        switch (req.Uri.LocalPath)
        {
            case "/page1": return EndPoint.Page1;
            case "/page2": return EndPoint.Page2;
            default: return null;
        }
    }

    public Uri Link(EndPoint endpoint) 
    {
        switch (endpoint)
        {
            case EndPoint.Page1: return new Uri("/page1", System.UriKind.Relative);
            case EndPoint.Page2: return new Uri("/page2", System.UriKind.Relative);
            default: return null;
        }
    }
}
```

Specifying routers manually gives you full control of how to parse incoming requests and to map endpoints to corresponding URLs.  It is your responsibility to make sure that the router forms a bijection of URLs and endpoints, so that linking to an endpoint produces a URL that is in turn routed back to the same endpoint.

Constructing routers manually is only required for very special cases. The above router can for example be generated using [`Router.Table`](/api/v4.1/WebSharper.Sitelets.Router#Table\`\`1):

```csharp
var MyRouter : Router<EndPoint> =
    Router.Table(
        Tuple.Create(EndPoint.Page1, "/page1"),
        Tuple.Create(EndPoint.Page2, "/page2")
    )
```

Even simpler, if you want to create the same URL shapes that would be generated by `Sitelet.Infer`, you can simply use `InferRouter.Router.Infer()`:

```csharp
var MyRouter : Router<EndPoint> =
    InferRouter.Router.Infer ()
```

### Router primitives

The `WebSharper.Sitelets.RouterOperators` module exposes the following basic `Router` values and construct functions: (following examples are assuming that you have `using static WebSharper.Sitelets.RouterOperators;`)

* `rRoot`: Recognizes and writes an empty path.
* `r "path"`: Recognizes and writes a specific subpath. You can also write `r "path/subpath"` to parse two or more segments of the URL.
* `rString`, `rChar`: Recognizes a URIComponent as a string or char and writes it as a URIComponent.
* `rTryParse<T>`: Creates a router for any type that defines a `TryParse` static method.
* `rInt`, `rDouble`, ...: Creates a router for numeric values.
* `rBool`, `rGuid`: Additional primitive types to parse from or write to the URL. 
* `rDateTime`: Parse or write a `DateTime`, takes a format string.

### Router combinators

* `Router.Combine`: Parses or writes using two routers one after the other. For example `Router.Combine(rString, rInt)` will have type `Router<Tuple<string, int>>`.
* `/`: Same as above, but when one side is a non-generic `Router` or a string which adds a constant URL fragment. For example `r("article") / r("id") / rInt` can be shortened to `"article/id" / rInt`.
* `+` (alias `Router.Add`): Parses or writes using the first router if successful, otherwise the second.
* `Router.Sum`: Optimized version of combining a sequence of routers with `+`. Parses or writes with the first router in the sequence that can handle the path or value.
* `.Map`: A bijection (or just surjection) between representations handled by routers. For example if you have a class named `Person` with fields `string Name` and `int Age`, then you can define a router for it by mapping from a `Router<Tuple<string, int>>` like so
    ```csharp
    var rPerson =
        Router.Combine(rString, rInt)
            .Map(
                (n, a) => new Person { Name = n, Age = a },
                p => (p.Name, p.Age).ToTuple()
            );
    ```
    See that `Map` needs two function arguments, to convert data back and forth between representations. All values of the resulting type must be mapped back to underlying type by the second function in a way compatible with the first function to work correctly.
* `.MapTo`: Maps a non-generic `Router` to a single valued `Router<T>`. For example if `Home` is a base class for your endpoint type hierarchy with a singleton instance, you can create a router for it by:
    ```csharp
    var rHome = rRoot.MapTo(Home.Instance);
    ```
    This only needs a single value as argument, but the type used must be comparable, so the writer part of the newly created `Router<T>` can decide if it is indeed the `Home.Instance` value that it needs to write by the underlying router (in our case producing a root URL).
* `.Filter`: restricts a router to parse/write values only that are passing a check. Usage: `rInt.Filter(x => x >= 0)`, which won't parse and write negative values.
* `.Query`: Modifies a router to parse from and write to a specific query argument instead of main URL segments. Usage: `rInt.Query("x")`, which will read/write query segments like `?x=42`. You should pass only a router that is always reading/writing a single segment, which inclide primitive routers, `Router.Nullable`, and `Sum`s and `Map`s of these.
* `.QueryNullable`: Modifies a router to read an optional query value as a `System.Nullable`. Creates a `Router<Nullable<T>`, same restrictions apply as to `Query`.
* `.Box`: Converts a `Router<T>` to a `Router<object>`. When writing, it uses a type check to see if the object is of type `T` so it can be passed to underlying router.
* `.Unbox`:  Converts a `Router<object>` to a `Router<T>`. When parsing, it uses a type check to see if the object is of type `T` so that the parsed value can be represented in `T`.
* `.Array`: Creates an array parser/writer. The URL will contain the length and then the items, so for example `rString.Array()` can handle `2/x/y`.
* `.Nullable`: Creates a `Nullable` value parser/writer. Writes or reads `null` for null or a value that is handled by the input router. For 
* `InferRouter.Router.Infer`: Creates a router based on type shape. The attributes recognized are the same as `Sitelet.Infer` described in the [Sitelets documentation](sitelets.md).
* `Router.Table`: Creates a router mapping from any number of `Tuple<Endpoint, string>` arguments, connecting the given endpoint values and paths.
* `Router.Method`: Creates a router that only parses request with the inner router, it the HTTP method methes the given method argument. By default, routers ignore the method.
* `Router.Body` : Creates a router that parses and serializes any value to and from the request body with custom functions. If the will be used on server-side only to parse requests and generate links, the serialize function can return just a null or empty string. For example `Router.Body(x => x, x => x)` just gets the request body as a string.
* [`Router.Json`](/api/v4.1/WebSharper.Sitelets.Router#Json\`\`1) creates a router that parses the request body by the JSON format derived from the type argument.
* [`Router.FormData`](/api/v4.1/WebSharper.Sitelets.Router#FormData) creates a router from an underlying router handling query arguments that parses query arguments from the request body of a form post instead of the URL.
* `Router.Delay` can be used to construct routers for recursive data types. Takes an `Func<Router<'T>>` function, and evaluates it firsthe t time the router is used for parsing and writing (never just when combining them).

### Using the router

* `Router.Link` creates a (relative) link using a router.
A useful helper to have in the file defining your router is:
    ```csharp
        public Doc MakeLink(EndPoint page, string content) =>
            a(attr.href(router.Link(page)), content);
    ```
This works the same on both server and client-side to create basic `<a>` links to pages of your web application.
* `Sitelet.New` creates a Sitelet from a router and handler. Example:
```csharp
    [Website]
    static Sitelet<object> Main =>
        Sitelet.New(rPages, (ctx, ep) => {
            switch (ep)
            {
                case Home h: return div("This is the home page");
                case Contact c: return client (() => ContactMain());
                default: return null;
            }
        }
```
Here we return a static page for the root, but call into a client-side generated content in the `Contact` pages, which is parsing the URL again to show the contact details from the URL.
Sitelets are only a server-side type.
* `Router.Ajax` makes a request from an endpoint value on the client and executes it using `jQuery.ajax`. Returns a `Task<string>`, which raises an exception internally if the request fails. Example:
```csharp
    // [<EndPoint "/get-data/{i}">] public class GetData { public int i; }

    public async Task<string> GetDataAsyncSafe(int i) {
        try
            return await router.Ajax(new GetData { i = i });
        else 
            return null;
    }

```
