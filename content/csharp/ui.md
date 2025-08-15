---
title: UI in C#
---

WebSharper.UI, [documented here](../ui/html) for F#, has a companion library for C# API in the NuGet package [WebSharper.UI.CSharp](https://www.nuget.org/packages/websharper.ui.csharp).

<a name="html"></a>
## Constructing HTML

F# has modules for holding HTML construction functions, the same is replicated in C# with static classes.
You can use `using static` to import the static methods of these classes, so that you can use them without the class name prefix.
These classes are `WebSharper.UI.Html` for server-side HTML construction, and `WebSharper.UI.Client.Html` for client-side HTML construction.
Unlike F#, these classes both define all the tag and attribute helpers. Use one or the other depending on whether you are writing server-side or client-side code.
If you want to write methods that will be shared between the server and the client, you can use the `WebSharper.UI.Client.Html` class, except the `on` nested class for event handlers.

#### Docs

Every HTML element has a dedicated method, such as `div` or `p`, which takes a any number of `object` parameters, which can represent attributes or child elements. The following parameter values are accepted:

Both on the server and the client:

* value of type `WebSharper.UI.Doc`, which can represent one, none or multiple child nodes.
* value of type `WebSharper.UI.Attr`, which can represent one, none or multiple attributes.
* a string value will be added as a text node.
* `null` will be treated as empty content.

Additionally, on the server:

* an object implementing `WebSharper.INode`, which exposes a method to write to the response content directly.
* a Linq expression of type `Expr<IControlBody>`, which creates a placeholder in the returned HTML, to be replaced by client-side code.
* any other object will be converted to a string with its `ToString` function and included as a text node.

On the client:

* a `Dom.Element` will be added as a static child element.
* a `View<T>`, where `T` can be any type handled except `Attr`, and creates a reactive binding to that `View`.
* a `Var<T>`, where `T` can be any type handled except `Attr`, and creates a reactive binding to the value of that `Var`.
* any other object will be converted to a string with its `ToString` function and included as a text node.

An example:

```csharp
using static WebSharper.UI.Html;

var myDoc =
    div(
        h1("Functional Reactive Programming and HTML"),

        p("WebSharper.UI is a library providing a novel, pragmatic and convenient approach to UI reactivity. It includes:"),

        ul(
            li("...")
        )
    );

// <div>
//   <h1>Functional Reactive Programming and HTML</h1>
//   <p>WebSharper.UI is a library providing a novel, pragmatic and convenient
//      approach to UI reactivity. It includes:</p>
//   <ul>
//     <li>...</li>
//   </ul>
// </div>
```

Some HTML tags, such as `var`, collide with keyword names and are therefore only located in the `Tags` nested class.
Some other tags like `option` collide in F#, so they are only inside `Tags` too having the organization consistent between the two languages.

```csharp
var myText =
    div("The value of ", Tags.var("x"), " is 2.")

// <div>
//   The value of <var>x</var> is 2.
// </div>
```

One thing to note is that the tag functions described above actually return a value of type `Elt`, which is a subtype of `Doc` that is guaranteed to always consist of exactly one element and provides additional APIs such as the `.Dom` property to get the underlying `Dom.Element`.

Additional functions in the `Doc` static class can create or combine Docs, similar to the [F# implementation](../ui/html#constructing-html).
An additional helper is the `Doc.ConcatMixed` method that can similarly take mixed arguments as HTML element constructors, but does not wrap them in an element, but returns a `Doc` that contains the given content as consecutive nodes.
The short alias for this is `doc`.

<a name="attr"></a>
#### Attrs

To create attributes, use corresponding functions from the `attr` nested class.

```csharp
var myFormControl =
    select(
        attr.name("mySelect"),
        Tags.option(attr.value "first", "First choice"),
        Tags.option(attr.value "second", "Second choice"),
        Tags.option(
            attr.value("third"),
            attr.selected("selected"),
            "Third choice"
        )
   )

// <select name="mySelect">
//   <option value="first">First choice</option>
//   <option value="second">Second choice</option>
//   <option value="third" selected="selected">Third choice</option>
// </select>
```

Some attributes, notably `class` and `type`, are also C# keywords, so they need to be prefixed with a `@`.

```csharp
var myMain =
    div(attr.@class("main"), "...")

// <div class="main">...</div>
```

HTML5 also defines any attribute whose names starts with `data-` as a valid custom attribute. You can create such an attribute using the function `data` from module `attr`.

```csharp
var myEltWithData =
    div(attr.data("uid", "myDiv"), "...")

// <div data-uid="myDiv">...</div>
```

Like `Doc`, a value of type `Attr` can represent zero, one or more attributes. Similar functions are available as in F# to [combine attibutes](../ui/html#attrs).

#### Event handlers

A special kind of attribute is event handlers. They can be created using functions from the `on` nested static class.
Furthermore, some element constructing methods define an overload to add an event handler to a default event directly, like a `click` for a `button`.

```csharp
var myButton =
    button(on.click((el, ev) => JS.Window.Alert("Hi!")), "Click me!");

var myButtonShorterForm =
    button("Click me!", () => JS.Window.Alert("Hi!"));
```

The handler function has type `Action<Dom.Element, Dom.Event>` which will get:

* The element itself, as a native `Dom.Element`;
* The triggered event, as a native `Dom.Event`.

If you need these values, you cannot use the convenience overload, which just takes an `Action`.

```csharp
var myButton =
    button(
        attr.id("my-button"),
        on.click((el, ev) =>
            JS.Window.Alert($"You clicked {el.Id} at x = {ev.ClientX}, y = {ev.ClientY}.")),
        "Click me!"
    );

```

### HTML on the client

To insert a Doc into the document on the client side, use the `.Run*` family of extension methods.
Each of these methods has two overloads: one directly taking a `Dom.Element` or `Dom.Node`, and the other taking the id of an element as a string.

* `.Run` inserts the Doc as the child(ren) of the given DOM element. Note that it replaces the existing children, if any.

    ```csharp
    using WebSharper.JavaScript;
    using WebSharper.UI;
    using WebSharper.UI.Client;
    using static WebSharper.UI.Html;

    public static void Main() 
    {
        div("This goes into #main.").Run("main");
        
        p("This goes into the first paragraph with class my-content.").Run(JS.Document.QuerySelector("p.my-content"));
    }
    ```

* `.RunAppend` inserts the Doc as the last child(ren) of the given DOM element.

* `.RunPrepend` inserts the Doc as the first child(ren) of the given DOM element.

* `.RunAfter` inserts the Doc as the next sibling(s) of the given DOM node.

* `.RunBefore` inserts the Doc as the previous sibling(s) of the given DOM node.

* `Doc.RunReplace` inserts the Doc repacing the given DOM node.

### HTML on the server

On the server side, using [sitelets](sitelets), you can create HTML pages from Docs by passing them to the `Body` or `Head` arguments of `Content.Page`.

```csharp
using System.Threading.Tasks;
using WebSharper.Sitelets;
using WebSharper.UI;
using static WebSharper.UI.Html;

public static Task<Content> MyPage(Context ctx) =>
    Content.Page(
        Title: "Welcome!",
        Body: doc(
            h1("Welcome!"),       
            p("This is my home page.")
        )
    );
```

To include client-side elements inside a page, use the static `client` method of the class `WebSharper.UI.Html`.

<a name="templating"></a>
## HTML Templates

As far as the HTML is concerned, the very same syntax and features are available for C# templating as for F#.
See the [HTML Templates](../ui/templating) page in the F# documentation for details.
However, the mechanism is different, F# uses type providers, while C# uses a code generator that creates a class for each HTML file.

### Setup

To generate code based on a HTML file, include the `.html` as a `AdditionalFiles` element in your project.
The `WebSharper.UI.CSharp.Templating.Generator` code generator then creates a `.g.cs` file with the same name.
The recommended setup is to let the code generator write the generated files into disk for development, and then exclude them for not running into duplicated code errors.
The way to do this is to add the following to your `.csproj` file, which is already included in WebSharper templates:

```xml
  <PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
  </PropertyGroup>
  <ItemGroup>
    <!-- Exclude the earlier output of source generators from the C# compilation -->
    <Compile Remove="$(CompilerGeneratedFilesOutputPath)/**/*.cs" />
  </ItemGroup
```

The generated class will be in a namespace that is created from the assembly name and appending `.Template`.
The class will be named as the capitalized form of the name of the HTML file.

To instantiate it, call your type's constructor and then its `.Doc()` method.

```csharp
// mytemplate.html:
// <div>
//   <h1>Welcome!</h1>
//   <p>Welcome to my site.</p>
// </div>

var myPage = new Template.MyTemplate().Doc();

// equivalent to:
// var myPage =
//     div(
//         h1("Welcome!"),
//         p("Welcome to my site.")
//     );
```

Note that the template doesn't have to be a full HTML document, but can simply be a snippet or sequence of snippets. This is particularly useful to build a library of widgets using [inner templates](../ui/templating#inner-templates).

### Controlling the loading of templates

The code generator can be parameterized to control how its contents are loaded both on the server and the client by special comments on top of the html file. For example:

```html
<!-- ClientLoad = Inline 
     ServerLoad = WhenChanged -->
<!DOCTYPE html> ...
```

The comment can contain values for `ClientLoad` and `ServerLoad` as listed below, in separate lines if both are provided. Both keys and values are case-insensitive and spaces and any other kind of comment lines are ignored.

The possible values for `ClientLoad` are:

* `Inline` (default): The template is included in the compiled JavaScript code, and any change to `mytemplate.html` requires a recompilation to be reflected in the application.
* `FromDocument`: The template is loaded from the DOM. This means that `mytemplate.html` *must* be the document in which the code is run: either directly served as a Single-Page Application, or passed to `Content.Page` in a Client-Server Application.

The possible values for `ServerLoad` are:

* `WhenChanged` (default): The runtime sets up a file watcher on the template file, and reloads it whenever it is edited.
* `Once`: The template file is loaded on first use and never reloaded.
* `PerRequest`: The template file is reloaded every time it is needed. We recommend against this option for performance reasons.

<a name="reactive"></a>
## Reactive layer

For the description of `Var` and `View` types, see the [F# documentation](../ui/reactive).
The same APIs are available for them in C#, except functions like `Map` are extension methods on the `View<T>` type.

<a name="view-doc"></a>
#### Inserting Views in the Doc

Once you have created a View to represent your dynamic content, here are the various ways to include it in a Doc:

* `text` has a reactive overload, which creates a text node from a `View<string>`.

    ```csharp
    var varTxt = Var.Create("");
    var vLength =
        varTxt.View
            .Map(x => x.Length)
            .Map(l => $"You entered {l} characters.");
    var res =
        div(
            input(varName),
            text vLength // text can be even omitted here
        );
    ```

* `.Bind` maps a View into a dynamic Doc.

    ```csharp
    abstract class UserId { }

    class UserName : UserId
    {
        public string Name { get; set; }
    }

    class Email : UserId
    {
        public string Value { get; set; }
    }

    Var<bool> rvIsEmail = Var.Create(false);
    Var<UserId> rvEmail = Var.Create<UserId>(new Email { Value = "" });
    Var<UserId> rvUsername = Var.Create<UserId>(new UserName { Name = "" });

    View<UserId> vUserId = rvIsEmail.View.Bind(isEmail =>
        isEmail ? rvEmail.View : rvUsername.View
    );
    ```

* `attr.*` attribute constructors also have overloads taking a `View<string>`.

    For example, the following sets the background of the input element based on the user input value:

    ```csharp
    var varTxt = Var.Create("");
    var vStyle =
        varTxt.View
            .Map(s => "background-color: " + s);
    var res =
        input(varTxt, attr.style(vStyle));
    ```

* `attr.*` constructors also have overloads that take an extra `View<bool>`. When this View is true, the attribute is set (and dynamically updated), and when it is false, the attribute is removed.

    ```csharp
    var varTxt = Var.Create("");
    var varCheck = Var.Create(true);
    var vStyle =
        varTxt.View
            .Map(s => "background-color: " + s);
    var res =
        div(
            input(varTxt, attr.style(vStyle, varCheck.View)),
            checkbox(varCheck)
        );
    ```
