<!-- ID:83541 -->

You can construct various types of content to return from your [sitelet-based applications and services](//forums.websharper.com/topic/83535). In this page you can see examples for:

 * [Text](http://forums.websharper.com/topic/83541#83777)
 * [Files](http://forums.websharper.com/topic/83541#83778)
 * [JSON](http://forums.websharper.com/topic/83541#83860)
 * [HTML pages](http://forums.websharper.com/topic/83541#83781)
 * Template-based HTML pages
 * HTTP error codes
 * Custom headers

Prerequisites:

 * Open `WebSharper.Sitelets` to access `Content.*` functions:
   ```fsharp
   open WebSharper.Sitelets
   ```
 * For constructing HTML, you need to open `WebSharper.UI.Next.Html`:
    ```fsharp
    open WebSharper.UI.Next.Html
    ```


---

<!-- ID:83777 -->

**Text**

```fsharp
Content.Text("Time now is " + System.DateTime.Now.ToShortTimeString())
```


---

<!-- ID:83778 -->

**Files**

Files are served, for security reasons, from your web root folder:

```fsharp
Content.File("Main.html")
```

You can serve files from other folders too:

```fsharp
Content.File(@"c:\Main.html", AllowOutsideRootFolder=true)
```

Files by default are returned as `text/html` content. You can change the content type by supplying a new one:

```fsharp
Content.File("countries.json", ContentType="application/json")
```


---

<!-- ID:83781 -->

**HTML pages**

You can return HTML fragments by wrapping them in `Content.Page`:

```fsharp
Content.Page(
    div [
        ...
    ]
)
```


---

<!-- ID:83860 -->

**JSON**

You can serialize an F#/C# value to JSON using `Content.Json`:

```fsharp
type Person =
    {
        Name: string
        Age: int
    }

let john = { Name="John Smith"; Age=40; }

Content.Json john
```

Sometimes, you may want to return a string (say, `content`) as JSON, for instance if you used a third-party JSON serializer:

```fsharp
Content.Text content
|> WithContentType "application/json"
```
