---
title: Sitelets in C#
---

See the main documentation of sitelets in F# [here](../core/sitelets).

However, there are also API dedicated for ease of use in C#. For example, there are `Task`-based versions of the F# `Async`-based methods.

Below is the C# equivalent of the minimal example:

```csharp
using System;
using System.Threading.Tasks;
using WebSharper.Sitelets;
using static WebSharper.UI.Html;

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
        public static Sitelet<Index> MySampleWebsite =>            
            Sitelet.Content("/index", new Index(), IndexContent)
    }
}
```

## Routing

You can create endpoint types as classes.
The main difference from F# is that you need to specify the order of the fields of the class that are used to generate/parse the URL segments in your `[EndPoint]` attribute.

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

### SiteletBuilder

A handy way to create a sitelet is by using the `SiteletBuilder` class. It functions conceptually similar to `StringBuilder`, you can chain methods to assemble a final value:
* First, create an instance of `SiteletBuilder()`;
* Then, add your mappings using `.With<T>(...)` method calls;
* Finally, use `.Install()` to return your constructed `Sitelet`.

This creates a `Sitelet<object>`, which means it has less type safety than the F# version, but it is more flexible in C#. 

For example, using the endpoint types defined [in the above section](#routing), you can create the following Sitelet:

```csharp
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

### Defining endpoints

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

    [Learn more about JSON parsing.](../core/json)

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

Many of the same combinators are available in C# as in F#. See the [F# documentation](../core/sitelets#other-constructors-and-combinators).
The main differences are: 
* `Sitelet.Sum` and `Sitelet.Folder` have `params` arguments, so you can pass multiple sitelets as an array or as a comma-separated list. 
* Methods that modify a single sitelet are available as extension methods on `Sitelet<T>`, while those that combine multiple sitelets are available as static methods in the `Sitelet` class. These are `.Box`, `.Protect`, `.Map`, and `.Shift`.
For example, mapping a sitelet works like this:

```csharp
[EndPoint("/article/{Title}")]
public class Article {
    public string Title;
}

Sitelet.Infer<string>(ArticleContent).Map(t => new Article() { Title = t }, a => a.Title)
```

<a name="content"></a>
## Content

Similar content-creating functions are available in C# as in F#. See the [F# documentation](../core/content).
The main difference is that the content functions return a `Task<Content>` instead of `Async<Content>`.

For example, the following code creates a simple HTML page:

```csharp
using static WebSharper.UI.Html;

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

<a name="context"></a>
## Using the Context

The method `SiteletBuilder.With()` provides a context of type `Context`. This is similar to the web context object [available in F#](../core/webcontext), but it is not generic in C#.

<a name="linking"></a>
### Creating links

Here is an example using `context.Link` from C# to create links from endpoint objects:

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

Note how `context.Link` is used in order to resolve the URL to the `Article` endpoint.
`context.ResolveUrl` helps to manually construct application-relative URLs to resources that do not map to sitelet endpoints.

### Managing User Sessions

The `Context` object can also be used to access the currently logged in user. The member `UserSession` has the following `Task`-based extension members, which require `using WebSharper.Web;`:

* `Task LoginUserAsync(string username, bool persistent = false)`  
  `Task LoginUserAsync(string username, System.TimeSpan duration)`

    Logs in the user with the given username. This sets a cookie that is uniquely associated with this username. The second parameter determines the expiration of the login:

    * `LoginUserAsync("username")` creates a cookie that expires with the user's browser session.
    
    * `LoginUserAsync("username", persistent: true)` creates a cookie that lasts indefinitely.
    
    * `LoginUserAsync("username", duration: d)` creates a cookie that expires after the given duration.
    
    Example:
    
    ```csharp
    public async Task<Content<EndPoint>> LoggedInPage(Context<EndPoint> context, string username) 
    {
        // We're assuming here that the login is successful,
        // eg you have verified a password against a database.
        await context.UserSession.LoginUserAsync(username, 
                duration: TimeSpan.FromDays(30.));
        return Content.Page(
            Title: "Welcome!",
            Body: text($"Welcome, {username}!")
        );
    } 
    ```

* `Task<string> GetLoggedInUserAsync()`

    Retrieves the currently logged in user's username, or `null` if the user is not logged in.
    
    Example:
    
    ```csharp
    public async Task<Content<EndPoint>> HomePage(Context<EndPoint> context) 
    {
        var username = await context.UserSession.GetLoggedInUserAsync();
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

* `Task LogoutAsync()`

    Logs the user out.
    
    Example:
    
    ```csharp
    public async Content<EndPoint> Logout(Context<EndPoint> context)
    {
        await context.UserSession.LogoutAsync();
        return Content.RedirectTemporary(new Home());
    }
    ```

The implementation of these functions relies on cookies and thus requires that the browser has enabled cookies.
