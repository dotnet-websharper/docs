---
order: -20
label: Templating
---
# HTML templates

WebSharper.UI's [syntax for creating HTML](README.md) is compact and convenient, but sometimes you do need to include a plain HTML file in a project. It is much more convenient for designing to have a .html file that you can touch up and reload your application without having to recompile it. This is what Templates provide. Templates are HTML files that can be loaded by WebSharper.UI, and augmented with special elements and attributes that provide additional functionality:

* Declaring Holes for nodes, attributes and event handlers that can be filled at runtime by F# code;
* Declaring two-way binding between F# Vars and HTML input elements (see [reactive](#reactive));
* Declaring inner Templates, smaller HTML widgets within the page, that can be instantiated dynamically.

All of these are parsed from HTML at compile time and provided as F# types and methods, ensuring that your templates are correct.

### Basics

To declare a template, use the `Template` type provider from the namespace `WebSharper.UI.Templating`.

```fsharp
open WebSharper.UI.Templating

type MyTemplate = Template<"my-template.html">
```

To instantiate it, call your type's constructor and then its `.Doc()` method.

my-template.html:

```html
<div>
  <h1>Welcome!</h1>
  <p>Welcome to my site.</p>
</div>
```

```fsharp
open WebSharper.UI.Templating

type MyTemplate = Template<"my-template.html">

let myPage = MyTemplate().Doc()
```

equivalent to:

```html
let myPage =
    div [] [
        h1 [] [ text "Welcome!" ]
        p [] [ text "Welcome to my site." ]
    ]
```

Note that the template doesn't have to be a full HTML document, but can simply be a snippet or sequence of snippets. This is particularly useful to build a library of widgets using [inner templates](#inner-templates).

If the template comprises a single HTML element, then an additional method `.Elt()` is available. It is identical to `.Doc()`, except its return value has type `Elt` instead of `Doc`.

You can also declare a template from multiple files at once using a comma-separated list of file names. In this case, the template for each file is a nested class named after the file, truncated of its file extension.

my-template.html:

```html
<div>
  <h1>Welcome!</h1>
  <p>Welcome to my site.</p>
</div>
```

second-template.html:

```html
<div>
  <h2>This is a section.</h2>
  <p>And this is its content.</p>
</div>
```

```fsharp
open WebSharper.UI.Templating

type MyTemplate = Template<"my-template.html, second-template.html">

let myPage =
    Doc.Concat [
        MyTemplate.``my-template``().Doc()
        MyTemplate.``second-template``().Doc()
    ]
```

equivalent to:

```html
let myPage =
    Doc.Concat [
        div [] [
            h1 [] [ text "Welcome!" ]
            p [] [ text "Welcome to my site." ]
        ]
        div [] [
            h2 [] [ text "This is a section." ]
            p [] [ text "And this is its content." ]
        ]
   ]
```

### Holes

You can add holes to your template that will be filled by F# code. Each hole has a name. To fill a hole in F#, call the method with this name on the template instance before finishing with `.Doc()`.

* `${HoleName}` creates a `string` hole. You can use it in text or in the value of an attribute.

    my-template.html:

    ```html
    <div style="background-color: ${Color}">
      <h1>Welcome, ${Name}!</h1>
      <!-- You can use the same hole name multiple times,
           and they will all be filled with the same F# value. -->
      <p>This div's color is ${Color}.</p>
    </div>
    ```

    ```fsharp
    let myPage =
        MyTemplate()
            .Color("red")
            .Name("my friend")
            .Doc()
    ```

    Result:

    ```html
    <div style="background-color: red">
      <h1>Welcome, my friend!</h1>
      <!-- You can use the same hole name multiple times,
           and they will all be filled with the same F# value. -->
      <p>This div's color is red.</p>
    </div>
    ```
    
    On the client side, this hole can also be filled with a `View<string>` (see [reactive](#reactive)) to include dynamically updated text content.

* The attribute `ws-replace` creates a `Doc` or `seq<Doc>` hole. The element on which this attribute is set will be replaced with the provided Doc(s). The name of the hole is the value of the `ws-replace` attribute.

    my-template.html:

    ```html
    <div>
      <h1>Welcome!</h1>
      <div ws-replace="Content"></div>
    </div>
    ```

    ```fsharp
    let myPage =
        MyTemplate()
            .Content(p [] [ text "Welcome to my site." ])
            .Doc()
    ```

    Result:

    ```html
    <div>
      <h1>Welcome!</h1>
      <p>Welcome to my site.</p>
    </div>
    ```

* The attribute `ws-hole` creates a `Doc` or `seq<Doc>` hole. The element on which this attribute is set will have its _contents_ replaced with the provided Doc(s). The name of the hole is the value of the `ws-hole` attribute.

    my-template.html:

    ```html
    <div>
      <h1>Welcome!</h1>
      <div ws-hole="Content"></div>
    </div>
    ```

    ```fsharp
    let myPage =
        MyTemplate()
            .Content(p [] [ text "Welcome to my site." ])
            .Doc()
    ```

    Result:

    ```html
    <div>
      <h1>Welcome!</h1>
      <div>
          <p>Welcome to my site.</p>
      </div>
    </div>
    ```

* The attribute `ws-attr` creates an `Attr` or `seq<Attr>` hole. The name of the hole is the value of the `ws-attr` attribute.

    my-template.html:

    ```html
    <div ws-attr="MainDivAttr">
      <h1>Welcome!</h1>
      <p>Welcome to my site.</p>
    </div>
    ```

    ```fsharp
    let myPage =
        MyTemplate()
            .MainDivAttr(attr.``class`` "main")
            .Doc()
    ```

    Result:

    ```html
    <div class="main">
      <h1>Welcome!</h1>
      <p>Welcome to my site.</p>
    </div>
    ```

* The attribute `ws-var` creates a `Var` hole (see [reactive](#reactive)) that is bound to the element. It can be used on the following elements:

    * `<input>`, `<textarea>`, `<select>`, for which it creates a `Var<string>` hole.
    * `<input type="number">`, for which it creates a hole that can be one of the following types: `Var<int>`, `Var<float>`, `Var<CheckedInput<int>>`, `Var<CheckedInput<float>>`.
    * `<input type="checkbox">`, for which it creates a `Var<bool>` hole.

    The name of the hole is the value of the `ws-var` attribute. Text `${Hole}`s with the same name can be used, and they will dynamically match the value of the Var.

    my-template.html:

    ```html
    <div>
      <input ws-var="Name" />
      <div>Hi, ${Name}!</div>
    </div>
    ```

    ```fsharp
    let myPage =
        let varName = Var.Create ""
        MyTemplate()
            .Name(varName)
            .Doc()
    ```

    Result:

    ```html
    <div class="main">
      <input />
      <div>Hi, [value of above input]!</div>
    </div>
    ```

    If you don't fill the hole (ie you don't call `.Name(varName)` above), the `Var` will be implicitly created, so `${Name}` will still be dynamically updated from the user's input.

* The attribute `ws-onclick` (or any other event name instead of `click`) creates an event handler hole of type `TemplateEvent -> unit`. The argument of type `TemplateEvent` has the following fields:

    * `Target: Dom.Element` is the element itself.
    * `Event: Dom.Event` is the event triggered.
    * `Vars` has a field for each of the `Var`s associated to `ws-var`s in the template.

    my-template.html:

    ```html
    <div>
      <input ws-var="Name" />
      <button ws-onclick="Click">Ok</button>
    </div>
    ```

    ```fsharp
    let myPage =
        MyTemplate()
            .Click(fun t -> JS.Alert("Hi, " + t.Vars.Name.Value))
            .Doc()
    ```

### Filling holes

There are two ways to fill the content for a given hole.

* The recommended way is by using the method with the hole's name on the template instance, as used in the examples above.

    ```fsharp
    let myPage =
        MyTemplate()
            .Color("red")
            .Name("my friend")
            .Doc()
    ```

* If you need to decide which hole to fill at runtime, you can use the method `.With(holeName, content)`. It will throw a runtime error if the content's type doesn't match the hole's type.

    ```fsharp
    let myPage =
        MyTemplate()
            .With("Color", "red")
            .With("Name", "my friend")
            .Doc()
    ```

* You can of course mix and match both styles.

    ```fsharp
    let myPage =
        MyTemplate()
            .Color("red")
            .With("Name", "my friend")
            .Doc()
    ```

<a name="inner-templates"></a>
### Inner templates

To create a template for a widget (as opposed to a full page), you can put it in its own dedicated template file, but another option is to make it an inner template. An inner template is a smaller template declared inside a template file using the following syntax:

* The `ws-template` attribute declares that its element is a template whose name is the value of this attribute.
* The `ws-children-template` attribute declares that the children of its element is a template whose name is the value of this attribute.

Inner templates are available in F# as a nested class under the main provided type.

my-template.html:

```html
<div ws-attr="MainAttr">
  <div ws-replace="InputFields"></div>
  <div ws-template="Field" class="field-wrapper">
    <label for="${Id}">${Which} Name: </label>
    <input ws-var="Var" placeholder="${Which} Name" name="${Id}" />
  </div>
</div>
```

```fsharp
type MyTemplate = Template<"my-template.html">

let inputField (id: string) (which: string) (var: Var<string>) =
    MyTemplate.Field()
        .Id(id)
        .Which(which)
        .Var(var)
        .Doc()

let myForm =
    let firstName = Var.Create ""
    let lastName = Var.Create ""
    MyTemplate()
        .MainAttr(attr.``class`` "my-form")
        .InputFields(
            [
                inputField "first" "First" firstName
                inputField "last" "Last" lastName
            ]
        )
        .Doc()
```

Result:

```html
<div class="my-form">
  <div class="field-wrapper">
    <label for="first">First Name: </label>
    <input placeholder="First Name" name="first" />
  </div>
  <div class="field-wrapper">
    <label for="last">Last Name: </label>
    <input placeholder="Last Name" name="last" />
  </div>
</div>
```

### Instantiating templates in HTML

You can also instantiate a template within another template, entirely in HTML, without the need for F# to glue them together.

A node named `<ws-TemplateName>` instantiates the inner template `TemplateName` from the same file. A node named `<ws-fileName.TemplateName>` instantiates the inner template `TemplateName` from the file `fileName`. The file name is the same as the generated class name, so with file extension excluded.

Child elements of the `<ws-*>` fill holes. These elements are named after the hole they fill.

* `${Text}` holes are filled with the text content of the element.
* `ws-hole` and `ws-replace` holes are filled with the HTML content of the element.
* `ws-attr` holes are filled with the attributes of the element.
* Other types of holes cannot be directly filled like this.

Additionally, attributes on the `<ws-*>` element itself define hole mappings. That is to say, `<ws-MyTpl Inner="Outer">` fills the hole named `Inner` of the template `MyTpl` with the value of the hole `Outer` of the containing template. As a shorthand, `<ws-MyTpl Attr>` is equivalent to `<ws-MyTpl Attr="Attr">`.

Any holes that are neither mapped by an attribute nor filled by a child element are left empty.

The following example is equivalent to the example from [Inner Templates](#inner-templates):

my-template.html:

```html
<div ws-attr="MainAttr">
  <!-- Instantiate the template for input fields. -->
  <!-- Creates the holes FirstVar and SecondVar for the main template. -->
  <!-- Fills the holes Id, Which and Var of Field in both instantiations. -->
  <ws-Field Var="FirstVar">
    <Id>first</Id>
    <Which>First</Which>
  </ws-field>
  <ws-Field Var="SecondVar">
    <Id>last</Id>
    <Which>Last</Which>
  </ws-field>
</div>
<!-- Declare the template for input fields -->
<div ws-template="Field" class="field-wrapper">
  <label for="${Id}">${Which} Name: </label>
  <input ws-var="Var" placeholder="${Which} Name" name="${Id}" />
</div>
```

```fsharp
type MyTemplate = Template<"my-template.html">

let myForm =
    let firstName = Var.Create ""
    let lastName = Var.Create ""
    MyTemplate()
        .FirstVar(firstName)
        .SecondVar(lastName)
        .Doc()
```

### Controlling the loading of templates

The type provider can be parameterized to control how its contents are loaded both on the server and the client. For example:

```fsharp
type MyTemplate = 
    Template<"my-template.html", 
        clientLoad = ClientLoad.Inline,
        serverLoad = ServerLoad.WhenChanged>
```

The possible values for `clientLoad` are:

* `ClientLoad.Inline` (default): The template is included in the compiled JavaScript code, and any change to `my-template.html` requires a recompilation to be reflected in the application.
* `ClientLoad.FromDocument`: The template is loaded from the DOM. This means that `my-template.html` *must* be the document in which the code is run: either directly served as a Single-Page Application, or passed to `Content.Page` in a Client-Server Application.

    In this mode, it doesn't make sense for client-side code to instantiate the full template, since you are already inside the document. But the following are possible:
    * [Instantiating inner templates.](#inner-templates)
    * [Binding directly to the DOM.](#binding-dom)

The possible values for `serverLoad` are:

* `ServerLoad.WhenChanged` (default): The runtime sets up a file watcher on the template file, and reloads it whenever it is edited.

* `ServerLoad.Once`: The template file is loaded on first use and never reloaded.

* `ServerLoad.PerRequest`: The template file is reloaded every time it is needed. We recommend against this option for performance reasons.

<a name="binding-dom"></a>
### Binding directly to the DOM

When using a template from the client side that is declared with `clientLoad = ClientLoad.FromDocument`, you can directly bind content, Vars, etc. to the DOM. Instead of calling `.Doc()` to create a Doc, use `.Bind()`, which returns `unit`, to just apply the template to the current document.

index.html:

```html
<html>
  <head>
    <title>Welcome!</title>
  </head>
  <body>
    <h1>Welcome!</h1>
    <div ws-replace="Paragraph"></div>
    <button ws-onclick="ClickMe">${ClickText}</button>
  </body>
</html>
```

```fsharp
type Index = Template<"index.html", ClientLoad.FromDocument>

Index()
    .Paragraph(p [] [text "Welcome to my site."])
    .ClickMe(fun _ -> JS.Alert "Clicked!")
    .ClickText("Click me!")
    .Bind()
```

Result:

```html
<html>
  <head>
    <title>Welcome!</title>
  </head>
  <body>
    <h1>Welcome!</h1>
    <p>Welcome to my site.</p>
    <button>Click me!</button>
  </body>
</html>
```

Note that for `Bind()` to work correctly, the holes need to be present in the document itself. This is not a problem if your project is an SPA. But you can also serve the page from a Sitelet, using the same template on the server side. You can fill some holes on the server side and leave some to be filled by the client side. However, by default, the server-side engine removes unfilled holes from the served document. This is correct behavior in most cases, but here, the client does need the unfilled hole markers like `ws-replace` or `${...}` to be present. So this behavior can be overridden by the optional boolean argument `keepUnfilled` of the `.Doc()` and `.Elt()` methods.

index.html:

```html
<html>
  <head>
    <title>Welcome!</title>
  </head>
  <body>
    <h1>Welcome!</h1>
    <div ws-replace="Paragraph"></div>
    <button ws-onclick="ClickMe">${ClickText}</button>
  </body>
</html>
```

```fsharp
type Index = Template<"index.html", ClientLoad.FromDocument>

[<JavaScript>]
module Client =

    let Startup() =
        Index()
            .ClickMe(fun _ -> JS.Alert "Clicked!")
            .ClickText("Click me!")
            .Bind()

module Server =
    open WebSharper.UI.Server

    let MyPage() =
        Content.Page(
            Index()
                .Paragraph(p [] [text "Welcome to my site."])
                .Elt(keepUnfilled = true)
                .OnAfterRender(fun _ -> Client.Startup())
        )
```

Served page:

```html
<html>
  <head>
    <title>Welcome!</title>
  </head>
  <body>
    <h1>Welcome!</h1>
    <p>Welcome to my site.</p>
    <button ws-onclick="ClickMe">${ClickText}</button>
  </body>
</html>
```

Result after Client.Startup() has run:

```html
<html>
  <head>
    <title>Welcome!</title>
  </head>
  <body>
    <h1>Welcome!</h1>
    <p>Welcome to my site.</p>
    <button>Click me!</button>
  </body>
</html>
```

### Accessing the template's model

Templates allow you to access their "model", ie the set of all the reactive `Var`s that are bound to it, whether passed explicitly or automatically created for its `ws-var`s. It is accessible in two ways:

* In event handlers, it is available as the `Vars` property of the handler argument.
* From outside the template: instead of finishing the instanciation of a template with `.Doc()`, you can call `.Create()`. This will return a `TemplateInstance` with two properties: `Doc`, which returns the template itself, and `Vars`, which contains the Vars. This method is only available when instantiating the template from the client side.

    my-template.html:

    ```html
    <div>
      <input ws-var="Name" />
      <div>Hi, ${Name}!</div>
    </div>
    ```

    ```fsharp
    let myInstance = MyTemplate().Create()
    myInstance.Vars.Name <- "John Doe"
    let myDoc = myInstance.Doc
    ```

    Result:

    ```html
    <div>
      <input value="John Doe" />
      <div>Hi, John Doe!</div>
    </div>
    ```

### Mixing client code in server-side templates

It is possible to include some client-side functionality when creating a template on the server side.

* If you use `ws-var="VarName"`, the corresponding Var will be created on the client on page startup. However, passing a Var using `.VarName(myVar)` is not possible, since it would be a server-side Var.

* Event handlers (such as `ws-onclick="EventName"`) work fully if you pass an anonymous function: `.EventName(fun e -> ...)`. The body of this function will be compiled to JavaScript. You can also pass a top-level function, in this case it must be declared with `[<JavaScript>]`.

    This also includes `ws-onafterrender`, which causes the given function to be called on page startup.

### Special holes in server-side templates

In a server-side template, you must specify the location of where WebSharper can include its generated content.
Three special placeholders are provided to include client-side content in the page:

* `scripts` is replaced with the JavaScript files required by the client-side code included in the page (including WebSharper-generated `.js` files). Usage: `<script ws-replace="scripts"></script>`
* `styles` is replaced with the CSS files required by the client-side code included in the page. Usage: `<link ws-replace="styles" />`
* `meta` is replaced with a `<meta>` tag that contains initialization data for client-side controls. Usage: `<meta ws-replace="meta" />`

The `scripts` hole is necessary for correct working of the served page if it contains any client-side WebSharper functionality.
The other two are optional: if neither `styles` nor `meta` is provided explicilty, then they are included automatically above the content for `scripts`.

### Dynamic templates

It is also possible to create a template without the compile-time safety of the type provider. This is done using the type `DynamicTemplate`.

This type can be used similarly to `Template<...>`, with the following limitations:

* It is server-side only.
* Its constructor must receive the HTML source as a string.
* Holes can only be filled with `.With(holeName, content)`.
* The final instantiation must be done with `.Doc()`.

```fsharp
let myPage =
    DynamicTemplate("""<div style="background-color: ${Color}">Welcome, ${Name}!</div>""")
        .With("Color", "red")
        .With("Name", "my friend")
        .Doc()
```
