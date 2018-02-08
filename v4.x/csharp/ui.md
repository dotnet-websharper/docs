# Functional Reactive Programming and HTML

WebSharper.UI is a library providing a novel, pragmatic and convenient approach to UI reactivity. It includes:

* An [HTML library](#html) usable both from the server side and from the client side, which you can use to build HTML pages either by calling C# functions to create elements, or by instantiating template HTML files.
* A [reactive layer](#reactive) for expressing user inputs and values computed from them as time-varying values. This approach is related to Functional Reactive Programming (FRP). This reactive system integrates with the HTML library to create reactive documents. If you are familiar with Facebook React, then you will find some similarities with this approach: instead of explicitly inserting, modifying and removing DOM nodes, you return a value that represents a DOM tree based on inputs. The main difference is that these inputs are nodes of the reactive system, rather than a single state value associated with the component.

This page is an overview of the capabilities of WebSharper.UI. You can also check [the full reference of all the API types and modules](http://developers.websharper.com/api/WebSharper.UI).

The the base library, and C#-oriented extension methods are in two separate packages, get them from NuGet: [WebSharper.UI](https://www.nuget.org/packages/websharper.ui) and [WebSharper.UI.CSharp](https://www.nuget.org/packages/websharper.ui.csharp).


## Using HTML

WebSharper.UI's core type for HTML construction is [`Doc`](/api/v4.1/WebSharper.UI.Doc). A Doc can represent a single DOM node (element, text), but it can also be a sequence of zero or more nodes. This allows you to treat equally any HTML snippet that you want to insert into a document, whether it consists of a single element or not.

Additionally, client-side Docs can be reactive. A same Doc can consist of different elements at different moments in time, depending on user input or other variables. See [the reactive section](#reactive) to learn more about this.

<a name="html"></a>
### Constructing HTML

#### Docs

The main way to create `Doc`s is to use the static methods from the `WebSharper.UI.Html` or `WebSharper.UI.Client.Html` classes. The difference is that the first contains server-side `Doc` constructors, including taking event handlers as a LINQ expression, to be auto-translated and ran on the client. The second contains server-side `Doc` constructors, including reactive functionalities. The composing static `Doc` content is available on both with the same syntax.

Every HTML element has a dedicated method, such as `div` or `p`, which takes a any number of `object` parameters, which can represent attributes or child elements. The following parameter values are accepted:

Both on the server and the client:

* value of type `WebSharper.UI.Doc`, which can represent one, none or multiple child nodes.
* value of type `WebSharper.UI.Attr`, which can represent one, none or multiple attributes.
* a string value will be added as a text node.
* `null` will be treated as empty content.

Additionally, on the server:

* an object implementing `WebSharper.INode`, which exposes a method to write to the response content directly.
* a Linq expression of type `Expr<IControlBody>`, which creates a placeholder in the returned HTML, to be replaced by client-side code.
* any other will be converted to a string with its `ToString` function and included as a text node.

On the client:

* a `Dom.Element` will be added as a static child element.
* a `View<T>`, where `T` can be any type handled except `Attr`, and creates a reactive binding to that `View` (more about [Views](#views) later).
* a `Var<T>`, where `T` can be any type handled except `Attr`, and creates a reactive binding to the value of that `Var` (more about [Vars](#vars) later).
* any other will be converted to a string with its `ToString` function and included as a text node.

```csharp
using WebSharper.UI.Html;

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

Some HTML tags, such as `var`, collide with keyword names and are therefore only located in the `Tags` nested class. Some other tags, like `option` collide in F#, and they are too only inside `Tags` to have the organization consistent between the two languages.

```csharp
var myText =
    div("The value of ", Tags.var("x"), " is 2.")

// <div>
//   The value of <var>x</var> is 2.
// </div>
```

One thing to note is that the tag functions described above actually return a value of type [`Elt`](/api/v4.1/WebSharper.UI.Elt), which is a subtype of `Doc` that is guaranteed to always consist of exactly one element and provides additional APIs such as [`Dom`](/api/v4.1/WebSharper.UI.Elt#Dom) to get the underlying `Dom.Element`.

Additional functions in the [`Doc`](/api/v4.1/WebSharper.UI.Doc) can create or combine Docs:

* `doc` takes a number of object arguments, same as HTML element construction methods, but does not wrap them in an element, but returns a `Doc` that contains the given content as consecutive nodes.

* [`Doc.Empty`](/api/v4.1/WebSharper.UI.Doc#Empty) creates a Doc consisting of zero nodes, optimized version of `doc()`. Inside parameter lists of HTML element constructors, you can pass a `null` to have empty content, but when you are returning a `Doc`, it is recommended to use `Doc.Empty` so that instance methods can be called on the return value without a null error.

    ```csharp
    public Doc WelcomeText(bool show) =>
        show ? div("Hello!") : Doc.Empty;
        
    // <div>Hello!</div>
    //
    // or nothing.
    ```

* [`Doc.Append`](/api/v4.1/WebSharper.UI.Doc#Append) creates a Doc consisting of the concatenation of two Docs, optimized version of `doc(x, y)`.

    ```csharp
    var titleAndBody =
        Doc.Append(
            h1("Functional Reactive Programming and HTML"),
            p("WebSharper.UI is a library providing ...")
        );
            
    // <h1>Functional Reactive Programming and HTML</h1>
    // <p>WebSharper.UI is a library providing ...</p>
    ```

For the mathematically enclined, the functions `Doc.Empty` and `Doc.Append` make Docs a monoid.

* [`Doc.Concat`](/api/v4.1/WebSharper.UI.Doc#Concat) generalizes `Append` by concatenating a sequence of Docs. Optimized version of `doc` that does not need to do type checks.

    ```csharp
    var thisPage =
        Doc.Concat(
            new[] {
                h1("Functional Reactive Programming and HTML"),
                p("WebSharper.UI is a library providing ..."),
                ul(
                    li("...")
                )
            }
        );
        
    // <h1>Functional Reactive Programming and HTML</h1>
    // <p>WebSharper.UI is a library providing ...</p>
    // <ul>
    //   <li>...</li>
    // </ul>
    ```

* [`Doc.Element`](/api/v4.1/WebSharper.UI.Doc#Element) creates an element with the given name, attributes and children. It is equivalent to the function with the same name from the `Html` module. This function is useful if the tag name is only known at runtime, or if you want to create a non-standard element that isn't available in `Html`. The following example creates a header tag of a given level (`h1`, `h2`, etc).

    ```csharp
    public Doc MakeHeader(int level, string content) =>
        Doc.Element("h" + level, new[] {}, new[] { text(content) });
        
    // <h1>content...</h1>
    // or
    // <h2>content...</h2>
    // or etc.
    ```

* [`Doc.Verbatim`](/api/v4.1/WebSharper.UI.Doc#Verbatim) creates a Doc from plain HTML text.  
    **Security warning:** this function does not perform any checks on the contents, and can be a code injection vulnerability if used improperly. We recommend avoiding it unless absolutely necessary, and properly sanitizing user inputs if you do use it. If you simply want to use HTML syntax instead of C# functions, take a look at [templating](#templating).

    ```csharp
    var plainDoc =
        Doc.Verbatim("
            <h1 onclick=\"alert('And it is unsafe!')\">
                This is plain HTML!
            </h1>"
        )

    // <h1 onclick="alert('And it is unsafe!')">
    //     This is plain HTML!
    // </h1>
    ```

<a name="attr"></a>
#### Attrs

To create attributes, use corresponding functions from the [`attr`](/api/v4.1/WebSharper.UI.Html.attr) nested class inside the `Html` static classes.

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

Like `Doc`, a value of type `Attr` can represent zero, one or more attributes. The functions in the [`Attr`](/api/v4.1/WebSharper.UI.Attr) module can create such non-singleton attributes.

* [`Attr.Empty`](/api/v4.1/WebSharper.UI.Attr#Empty) creates an empty attribute.

    ```csharp
    public Doc ValueAttr(string v) =>
        v is null ? Attr.Empty : attr.value(v);
         
    // value="value of v"
    //
    // or nothing.
    ```

* [`Attr.Append`](/api/v4.1/WebSharper.UI.Attr#Append) combines two attributes.

    ```csharp
    var passwordAttr =
        Attr.Append (attr.type("password"), attr.placeholder("Password"))

    // type="password" placeholder="Password"
    ```

* [`Attr.Concat`](/api/v4.1/WebSharper.UI.Attr#Concat) combines a sequence of attributes.

    ```csharp
    var passwordAttr =
        Attr.Concat(
            new[] {
                attr.type("password"),
                attr.placeholder("Password"),
                attr.@class("pw-input")
            }
        )

    // type="password" placeholder="Password" class="pw-input"
    ```

* [`Attr.Create`](/api/v4.1/WebSharper.UI.Attr#Create) creates a single attribute. It is equivalent to the function with the same name from the `attr` module. This function is useful if the attribute name is only known at runtime, or if you want to create a non-standard attribute that isn't available in `attr`.

    ```csharp
    var eltWithNonStandardAttr =
        div(Attr.Create("my-attr", "my-value"), "...")
        
    // <div my-attr="my-value">...</div>
    ```

#### Event handlers

A special kind of attribute is event handlers. They can be created using functions from the [`on`](/api/v4.1/WebSharper.UI.Html#on) nested static class.
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

To insert a Doc into the document on the client side, use the `.Run*` family of extension methods. Each of these methods has two overloads: one directly taking a DOM [`Element`](/api/v4.1/WebSharper.JavaScript.Dom.Element) or [`Node`](/api/v4.1/WebSharper.JavaScript.Dom.Node), and the other taking the id of an element as a string.

* [`.Run`](/api/v4.1/WebSharper.UI.Doc#Run) inserts the Doc as the child(ren) of the given DOM element. Note that it replaces the existing children, if any.

    ```csharp
    using WebSharper.JavaScript;
    using WebSharper.UI;
    using WebSharper.UI.Client;
    using WebSharper.UI.Html;

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

On the server side, using [sitelets](sitelets.md), you can create HTML pages from Docs by passing them to the `Body` or `Head` arguments of `Content.Page`.

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

To include client-side elements inside a page, use the `ClientSide` method of the Sitelets context.

<a name="templating"></a>
## HTML Templates

WebSharper.UI's syntax for creating HTML is compact and convenient, but sometimes you do need to include a plain HTML file in a project. It is much more convenient for designing to have a .html file that you can touch up and reload your application without having to recompile it. This is what Templates provide. Templates are HTML files that can be loaded by WebSharper.UI, and augmented with special elements and attributes that provide additional functionality:

* Declaring Holes for nodes, attributes and event handlers that can be filled at runtime by C# code;
* Declaring two-way binding between C# Vars and HTML input elements (see [reactive](#reactive));
* Declaring inner Templates, smaller HTML widgets within the page, that can be instantiated dynamically.

All of these are parsed from HTML at compile time and provided as C# types and methods, ensuring that your templates are correct.

### Basics

To generate code based on a HTML file, include the `.html` as a `Content` element in your project.
WebSharper's build as well as the analyzer then creates a `.g.cs` file with the same name.
Include this too in your project, recommended way (for an `index.html`) is:

```xml
    <Compile Include="index.g.cs">
      <AutoGen>True</AutoGen>
      <DesignTime>True</DesignTime>
      <DependentUpon>index.html</DependentUpon>
    </Compile>
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

Note that the template doesn't have to be a full HTML document, but can simply be a snippet or sequence of snippets. This is particularly useful to build a library of widgets using [inner templates](#inner-templates).

You can also declare a template from multiple files at once using a comma-separated list of file names. In this case, the template for each file is a nested class named after the file, truncated of its file extension.

```csharp
// myTemplate.html:
// <div>
//   <h1>Welcome!</h1>
//   <p>Welcome to my site.</p>
// </div>

// secondTemplate.html:
// <div>
//   <h2>This is a section.</h2>
//   <p>And this is its content.</p>
// </div>

var myPage =
    doc(
        new Template.MyTemplate().Doc(),
        new Template.SecondTemplate().Doc()
    )

// equivalent to:
// var myPage =
//     doc(
//         div(
//             h1("Welcome!"),
//             p("Welcome to my site.")
//        ),
//         div(
//             h2("This is a section."),
//             p("And this is its content.")
//        )
//    );
```

### Holes

You can add holes to your template that will be filled by C# code. Each hole has a name. To fill a hole in C#, call the method with this name on the template instance before finishing with `.Doc()`.

* `${HoleName}` creates a `string` hole. You can use it in text or in the value of an attribute.

    ```csharp
    // mytemplate.html:
    // <div style="background-color: ${Color}">
    //   <h1>Welcome, ${Name}!</h1>
    //   <!-- You can use the same hole name multiple times,
    //        and they will all be filled with the same C# value. -->
    //   <p>This div's color is ${Color}.</p>
    // </div>
    
    var myPage =
        new Template.MyTemplate()
            .Color("red")
            .Name("my friend")
            .Doc();

    // result:
    // <div style="background-color: red">
    //   <h1>Welcome, my friend!</h1>
    //   <!-- You can use the same hole name multiple times,
    //        and they will all be filled with the same C# value. -->
    //   <p>This div's color is red.</p>
    // </div>
    ```
    
    On the client side, this hole can also be filled with a `View<string>` (see [reactive](#reactive)) to include dynamically updated text content.

* The attribute `ws-replace` creates a `Doc` or `IEnumerable<Doc>` hole. The element on which this attribute is set will be replaced with the provided Doc(s). The name of the hole is the value of the `ws-replace` attribute.

    ```csharp
    // mytemplate.html:
    // <div>
    //   <h1>Welcome!</h1>
    //   <div ws-replace="Content"></div>
    // </div>
    
    var myPage =
        new Template.MyTemplate()
            .Content(p("Welcome to my site."),)
            .Doc();

    // result:
    // <div>
    //   <h1>Welcome!</h1>
    //   <p>Welcome to my site.</p>
    // </div>
    ```

* The attribute `ws-hole` creates a `Doc` or `IEnumerable<Doc>` hole. The element on which this attribute is set will have its _contents_ replaced with the provided Doc(s). The name of the hole is the value of the `ws-hole` attribute.

    ```csharp
    // mytemplate.html:
    // <div>
    //   <h1>Welcome!</h1>
    //   <div ws-hole="Content"></div>
    // </div>
    
    var myPage =
        new Template.MyTemplate()
            .Content(p("Welcome to my site."))
            .Doc();

    // result:
    // <div>
    //   <h1>Welcome!</h1>
    //   <div>
    //       <p>Welcome to my site.</p>
    //   </div>
    // </div>
    ```

* The attribute `ws-attr` creates an `Attr` or `IEnumerable<Attr>` hole. The name of the hole is the value of the `ws-attr` attribute.

    ```csharp
    // mytemplate.html:
    // <div ws-attr="MainDivAttr">
    //   <h1>Welcome!</h1>
    //   <p>Welcome to my site.</p>
    // </div>
    
    var myPage =
        new Template.MyTemplate()
            .MainDivAttr(attr.@class("main"))
            .Doc();

    // result:
    // <div class="main">
    //   <h1>Welcome!</h1>
    //   <p>Welcome to my site.</p>
    // </div>
    ```

* The attribute `ws-var` creates a `Var` hole (see [reactive](#reactive)) that is bound to the element. It can be used on the following elements:

    * `<input>`, `<textarea>`, `<select>`, for which it creates a `Var<string>` hole.
    * `<input type="number">`, for which it creates a hole that can be one of the following types: `Var<int>`, `Var<float>`, `Var<CheckedInput<int>>`, `Var<CheckedInput<float>>`.
    * `<input type="checkbox">`, for which it creates a `Var<bool>` hole.

    The name of the hole is the value of the `ws-attr` attribute. Text `${Hole}`s with the same name can be used, and they will dynamically match the value of the Var.

    ```csharp
    // mytemplate.html:
    // <div>
    //   <input ws-var="Name" />
    //   <div>Hi, ${Name}!</div>
    // </div>

    var myPage =
        var varName = Var.Create("");
        new Template.MyTemplate()
            .Name(varName)
            .Doc();

    // result:
    // <div class="main">
    //   <input />
    //   <div>Hi, [value of above input]!</div>
    // </div>
    ```

    If you don't fill the hole (ie you don't call `.Name(varName)` above), the `Var` will be implicitly created, so `${Name}` will still be dynamically updated from the user's input.

* The attribute `ws-onclick` (or any other event name instead of `click`) creates an event handler hole of type `TemplateEvent -> unit`. The argument of type `TemplateEvent` has the following fields:

    * `Target: Dom.Element` is the element itself.
    * `Event: Dom.Event` is the event triggered.
    * `Vars` has a field for each of the `Var`s associated to `ws-var`s in the template.

    ```csharp
    // mytemplate.html:
    // <div>
    //   <input ws-var="Name" />
    //   <button ws-onclick="Click">Ok</button>
    // </div>
    
    var myPage =
        new Template.MyTemplate()
            .Click(fun t -> JS.Window.Alert("Hi, " + t.Vars.Name.Value))
            .Doc();
    ```

<a name="inner-templates"></a>
### Inner templates

To create a template for a widget (as opposed to a full page), you can put it in its own dedicated template file, but another option is to make it an inner template. An inner template is a smaller template declared inside a template file using the following syntax:

* The `ws-template` attribute declares that its element is a template whose name is the value of this attribute.
* The `ws-children-template` attribute declares that the children of its element is a template whose name is the value of this attribute.

Inner templates are available in C# as a nested class under the main provided type.

```csharp
// mytemplate.html:
// <div ws-attr="MainAttr">
//   <div ws-replace="InputFields"></div>
//   <div ws-template="Field" class="field-wrapper">
//     <label for="${Id}">${Which} Name: </label>
//     <input ws-var="Var" placeholder="${Which} Name" name="${Id}" />
//   </div>
// </div>

public static Doc InputField(string id, string which, Var<string> svar) =>
    new Template.MyTemplate.Field()
        .Id(id)
        .Which(which)
        .Var(svar)
        .Doc();

var firstName = Var.Create("");
var lastName = Var.Create("");
var myForm =
    new Template.MyTemplate()
        .MainAttr(attr.@class("my-form"))
        .InputFields(
            doc(
                InputField("first", "First", firstName),
                InputField("last", "Last", lastName)
            ),
        )
        .Doc();

// result:
// <div class="my-form">
//   <div class="field-wrapper">
//     <label for="first">First Name: </label>
//     <input placeholder="First Name" name="first" />
//   </div>
//   <div class="field-wrapper">
//     <label for="last">Last Name: </label>
//     <input placeholder="Last Name" name="last" />
//   </div>
// </div>
```

### Instantiating templates in HTML

You can also instantiate a template within another template, entirely in HTML, without the need for C# to glue them together.

A node named `<ws-TemplateName>` instantiates the inner template `TemplateName` from the same file. A node named `<ws-fileName.TemplateName>` instantiates the inner template `TemplateName` from the file `fileName`. The file name is the same as the generated class name, so with file extension excluded.

Child elements of the `<ws-*>` fill holes. These elements are named after the hole they fill.

* `${Text}` holes are filled with the text content of the element.
* `ws-hole` and `ws-replace` holes are filled with the HTML content of the element.
* `ws-attr` holes are filled with the attributes of the element.
* Other types of holes cannot be directly filled like this.

Additionally, attributes on the `<ws-*>` element itself define hole mappings. That is to say, `<ws-MyTpl Inner="Outer">` fills the hole named `Inner` of the template `MyTpl` with the value of the hole `Outer` of the containing template. As a shorthand, `<ws-MyTpl Attr>` is equivalent to `<ws-MyTpl Attr="Attr">`.

Any holes that are neither mapped by an attribute nor filled by a child element are left empty.

The following example is equivalent to the example from [Inner Templates](#inner-templates):

```csharp
// mytemplate.html:
// <div ws-attr="MainAttr">
//   <!-- Instantiate the template for input fields. -->
//   <!-- Creates the holes FirstVar and SecondVar for the main template. -->
//   <!-- Fills the holes Id, Which and Var of Field in both instantiations. -->
//   <ws-Field Var="FirstVar">
//     <Id>first</Id>
//     <Which>First</Which>
//   </ws-field>
//   <ws-Field Var="SecondVar">
//     <Id>second</Id>
//     <Which>Second</Which>
//   </ws-field>
// </div>
// <!-- Declare the template for input fields -->
// <div ws-template="Field" class="field-wrapper">
//   <label for="${Id}">${Which} Name: </label>
//   <input ws-var="Var" placeholder="${Which} Name" name="${Id}" />
// </div>

var firstName = Var.Create("");
var lastName = Var.Create("");
var myForm =
    new Template.MyTemplate()
        .FirstVar(firstName)
        .LastVar(lastName)
        .Doc();
```

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

### Accessing the template's model

Templates allow you to access their "model", ie the set of all the reactive `Var`s that are bound to it, whether passed explicitly or automatically created for its `ws-var`s. It is accessible in two ways:

* In event handlers, it is available as the `Vars` property of the handler argument.
* From outside the template: instead of finishing the instanciation of a template with `.Doc()`, you can call `.Create()`. This will return a `TemplateInstance` with two properties: `Doc`, which returns the template itself, and `Vars`, which contains the Vars.

    ```csharp
    // mytemplate.html:
    // <div>
    //   <input ws-var="Name" />
    //   <div>Hi, ${Name}!</div>
    // </div>

    var myInstance = new Template.MyTemplate().Create();
    myInstance.Vars.Name = "John Doe";
    var myDoc = myInstance.Doc;

    // result:
    // <div>
    //   <input value="John Doe" />
    //   <div>Hi, John Doe!</div>
    // </div>
    ```

### Mixing client code in server-side templates

It is possible to include some client-side functionality when creating a template on the server side.

* If you use `ws-var="VarName"`, the corresponding Var will be created on the client on page startup. However, passing a Var using `.VarName(myVar)` is not possible, since it would be a server-side Var.

* Event handlers (such as `ws-onclick="EventName"`) work fully if you pass a delegate: `.EventName(e => ...)`. The body of this function will be compiled to JavaScript. You can also pass a top-level function, in this case it must be declared with `[JavaScript]`.

<a name="reactive"></a>
## Reactive layer

WebSharper.UI's reactive layer helps represent user inputs and other time-varying values, and define how they depend on one another.

### Special holes in server-side templates

In a server-side template, you must specify the location of where WebSharper can include its generated content.
Three special placeholders are provided to include client-side content in the page:

* `scripts` is replaced with the JavaScript files required by the client-side code included in the page (including WebSharper-generated `.js` files). Usage: `<script ws-replace="scripts"></script>`
* `styles` is replaced with the CSS files required by the client-side code included in the page. Usage: `<script ws-replace="scripts"></script>`
* `meta` is replaced with a `<meta>` tag that contains initialization data for client-side controls. Usage: `<meta ws-replace="meta" />`

The `scripts` hole is necessary for correct working of the served page if it contains any client-side WebSharper functionality.
The other two are optional: if neither `styles` nor `meta` is provided explicilty, then they are included automatically above the content for `scripts`.

### Vars

Reactive values that are directly set by code or by user interaction are represented by values of type [`Var<T>`](/api/v4.1/WebSharper.UI.Var\`1). Vars store a value of type `T` that you can get or set using the `Value` property. But they can additionally be reactively observed or two-way bound to HTML input elements.

The following are available from `WebSharper.UI.Client.Html`:

* `input` creates an `<input>` element with given attributes that is bound to a `Var<string>`, `Var<int>` or `Var<double>`.

    ```csharp
    var varText = Var.Create("initial value");
    var myInput = input(varText, attr.name("my-input"), varText);
    ```
    
    With the above code, once `myInput` has been inserted in the document, getting `varText.Value` will at any point reflect what the user has entered, and setting it will edit the input.

* `textarea` creates a `<textarea>` element bound to a `Var<string>`.

* `passwordBox` creates an `<input type="password">` element bound to a `Var<string>`.

* `checkbox` creates an `<input type="checkbox">` element bound to a `Var<bool>`.

* `checkbox` has an overload that also creates an `<input type="checkbox">`, but instead of associating it with a simple `Var<bool>`, it associates it with a specific `T` in a `Var<IEnumerable<T>>`. If the box is checked, then the element is added to the list, otherwise it is removed.

    ```csharp
    enum Color { Red, Green, Blue };
    
    // Initially, Green and Blue are checked.
    var varColor = Var.Create<IEnumerable<Color>>(new[] { Color.Blue, Color.Green });
    
    var mySelector =
        div(
            label(
                checkbox(Color.Red, varColor),
                " Select Red"
            ),
            label(
                checkbox(Color.Green, varColor),
                " Select Green"
            ),
            label(
                checkbox(Color.Blue, varColor),
                " Select Blue"
            )
       );
        
    // Result:
    // <div>
    //   <label><input type="checkbox" /> Select Red</label>
    //   <label><input type="checkbox" checked /> Select Green</label>
    //   <label><input type="checkbox" checked /> Select Blue</label>
    // </div>
    // Plus varColor is bound to contain the list of ticked checkboxes.
    ```

* `select` creates a dropdown `<select>` given a list of values to select from. The label of every `<option>` is determined by the given print function for the associated value.

    ```csharp
    enum Color { Red, Green, Blue };

    // Initially, Green is checked.
    var varColor = Var.Create(Green);

    // Choose the text of the dropdown's options.
    string showColor(Color c) { 
        switch (c)
        {
            case Color.Red: return "Red";
            case Color.Green: return "Green";
            case Color.Blue: return "Blue";
            default: return "";
        }
    }

    var mySelector =
        select(varColor, new[] { Color.Red, Color.Green, Color.Blue }, showColor);
        
    // Result:
    // <select>
    //   <option>Red</option>
    //   <option>Green</option>
    //   <option>Blue</option>
    // </select>
    // Plus varColor is bound to contain the selected color.
    ```

* `radio` creates an `<input type="radio">` given a value, which sets the given `Var` to that value when it is selected.

    ```csharp
    enum Color { Red, Green, Blue };
    
    // Initially, Green is selected.
    var varColor = Var.Create(Color.Green);
    
    var mySelector =
        div(
            label(
                radio(varColor. Color.Red),
                " Select Red"
            ),
            label(
                radio(varColor. Color.Green),
                " Select Green"
            ),
            label(
                radio(varColor. Color.Blue),
                " Select Blue"
            )
       );
        
    // Result:
    // <div>
    //   <label><input type="radio" /> Select Red</label>
    //   <label><input type="radio" checked /> Select Green</label>
    //   <label><input type="radio" /> Select Blue</label>
    // </div>
    // Plus varColor is bound to contain the selected color.
    ```

### Views

The full power of WebSharper.UI's reactive layer comes with [`View`s](/api/v4.1/WebSharper.UI.View\`1). A `View<T>` is a time-varying value computed from Vars and from other Views. At any point in time the view has a certain value of type `T`.

One thing important to note is that the value of a View is not computed unless it is needed. For example, if you use [`.Map`](#view-map), the function passed to it will only be called if the result is needed. It will only be run while the resulting View is included in the document using [one of these methods](#view-doc). This means that you generally don't have to worry about expensive computations being performed unnecessarily. However it also means that you should avoid relying on side-effects performed in methods like `.Map`.

In pseudo-code below, `[[x]]` notation is used to denote the value of the View `x` at every point in time, so that `[[x]]` = `[[y]]` means that the two views `x` and `y` are observationally equivalent.

Note that several of the methods below can be used more concisely using [the V shorthand](#v).

#### Creating and combining Views

The first and main way to get a View is using the [`View`](/api/v4.1/WebSharper.UI.Var\`1#View) property of `Var<T>`. This retrieves a View that tracks the current value of the Var.

You can create Views using the following functions and combinators from the `View` module:

* [`View.Const`](/api/v4.1/WebSharper.UI.View#Const\`\`1) creates a View whose value is always the same.

    ```csharp
    var v = View.Const(42);

    // [[v]] = 42
    ```

* <a name="view-map"></a>`.Map` takes an existing View and maps its value through a function.

    ```csharp
    View<string> v1 = // ...
    var v2 = v1.Map (s => s.Length);

    // [[v2]] = String.length [[v1]]
    ```

* [`View.Map2`](/api/v4.1/WebSharper.UI.View#Map2\`\`3) takes two existing Views (the one we are calling it on and one extra) and map their value through a function.

    ```csharp
    View<int> v1 = // ...
    View<int> v2 = // ...
    var v3 = v1.Map2(v2, (x, y) => x + y);

    // [[v3]] = [[v1]] + [[v2]]
    ```

    Similarly, [`.Map3`](/api/v4.1/WebSharper.UI.View#Map3\`\`4) takes three existing Views and map their value through a function.

* `.MapAsync` is similar to `View.Map` but maps through an asynchronous function.

    An important property here is that this combinator saves work by abandoning requests. That is, if the input view changes faster than we can asynchronously convert it, the output view will not propagate change until it obtains a valid latest value. In such a system, intermediate results are thus discarded.
    
    Similarly, `.MapAsync2` maps two existing Views through an asynchronous function.

* `.Apply` takes a View of a function and a View of its argument type, and combines them to create a View of its return type.

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

<a name="lens"></a>
### IRefs and lensing

The `Var<'T>` type is actually an abstract class, this makes it possible to create instances with an implementation different from `Var.Create`. The main example of this are **lenses**.

In WebSharper.UI, a lens is a Var that "focuses" on a sub-part of an existing Var. For example, given the following:

```csharp
class Person { 
    public string FirstName;
    public string LastName;
}

var varPerson = Var.Create(new Person { FirstName = "John", LastName = "Doe" });
```

You might want to create a form that allows entering the first and last name separately. For this, you need two `Var<string>`s that directly observe and alter the `FirstName` and `LastName` fields of the value stored in `varPerson`. This is exactly what a lens does.

To create a lens, you need to pass a getter and a setter function. The getter is called when the lens needs to know its current value, and extracts it from the parent IRef's current value. The setter is called when setting the value of the lens; it receives the current value of the parent IRef and the new value of the lens, and returns the new value of the parent IRef.

```csharp
Var<string> varFirstName =
    varPerson.Lens(
        p => p.FirstName,
        (p, n) => new Person { FirstName = n, LastName = p.LastName }
    );
Var<string> varLastName =
    varPerson.Lens(
        p => p.LastName,
        (p, n) => new Person { FirstName = p.FirstName, LastName = n }
    );
Doc myForm =
    div(
        input(varFirstName, attr.placeholder("First Name")),
        input(varLastName, attr.placeholder("Last Name"))
    )
```

<a name="v"></a>
### The V Shorthand

Mapping reactive values from their model to a value that you want to display can be greatly simplified using the V shorthand. This shorthand revolves around passing calls to the property `view.V` to a number of supporting functions.

#### Views and V

When an expression containing a call to `view.V` is passed as argument to one of the supporting functions, it is converted to a call to `View.Map` on this view, and the resulting expression is used in a way relevant to the supporting function.

The simplest supporting function is called `V`, and it simply returns the view expression. It requires `using static WebSharper.UI.V`.

```csharp
class Person { 
    public string FirstName;
    public string LastName;
}

View<Person> vPerson = // ...

View<string> vFirstName = V(vPerson.V.FirstName);

// The above is equivalent to:
View<string> vFirstName = vPerson.Map(p => p.FirstName);
```

You can use arbitrarily complex expressions:

```
var vFullName = V(vPerson.V.FirstName + " " + vPerson.V.LastName);

// The above is equivalent to:
var vFirstName = vPerson.Map (p => p.FirstName + " " + p.LastName);
```

Other supporting functions use the resulting View in different ways:

* `doc` applies this transformation to every argument before concatenating the results as a Doc.

    ```csharp
    Doc showName = doc(vPerson.V.FirstName, " ", vPerson.V.LastName);

    // The above is equivalent to:
    Doc showName = 
        doc(vPerson.Map(p => p.V.FirstName), " ", vPerson.Map(p => p.V.LastName));
    ```

* Similarly, HTML element functions (`div`, etc.) apply this transformation to every non-attribute argument.

    ```csharp
    Doc showName = doc(vPerson.V.FirstName, " ", vPerson.V.LastName);

    // The above is equivalent to:
    Doc showName = 
        doc(vPerson.Map(p => p.V.FirstName), " ", vPerson.Map(p => p.V.LastName));
    ```

* `attr.*(string)` attribute creation functions pass the resulting View to the corresponding `attr.*(View<string>)`.

    ```csharp
    class ImgData
    {
        public string Src;
        public int Height;
    }
    
    var myImgData = Var.Create(new ImgData { Src = "/my-img.png", Height = 200 });
    
    var myImg =
        img(
            attr.src(myImgData.V.Src),
            attr.height(myImgData.V.Height.ToString())
        );

    // The above is equivalent to:
    var myImg =
        img(
            attr.src(myImgData.Map(i => i.Src)),
            attr.height(myImgData.Map(i => i.Height.ToString()))
        )
    ```

Calling `.V` outside of one of the above supporting functions is a compile error. There is one exception: if `view` is a `View<Doc>`, then `view.V` is equivalent to `doc(view)`.

```csharp
let vMyDoc = V(varPerson.V == null ? Doc.Empty : div(varPerson.V.FirstName))
let myDoc = vMyDoc.V

// The above is equivalent to:
let vMyDoc = varPerson.View.Map(p => p == null ? Doc.Empty : div(p.FirstName))
let myDoc = doc(vMyDoc)
```

#### Vars and V

Vars also have a `.V` property. When used with one of the above supporting functions, it is equivalent to `.View.V`.

```csharp
var varPerson = Var.Create(new Person { FirstName = "John", LastName = "Doe" });

var vFirstName = V(varPerson.V.FirstName);

// The above is equivalent to:
var vFirstName = V(varPerson.View.V.FirstName);

// Which is also equivalent to:
var vFirstName = varPerson.View.Map(p => p.FirstName);
```

### ListModels

[`ListModel<K, T>`](/api/v4.1/WebSharper.UI.ListModel\`2) is a convenient type to store an observable collection of items of type `T`. Items can be accessed using an identifier, or key, of type `K`.

ListModels are to dictionaries as Vars are to refs: a type with similar capabilities, but with the additional capability to be reactively observed, and therefore to have your UI automatically change according to changes in the stored content.

#### Creating ListModels

You can create ListModels with the following methods constructors:

* `ListModel.FromSeq` creates a ListModel where items are their own key.

    ```csharp
    var myNameColl = ListModel.FromSeq (new[] { "John", "Ana" });
    ```

* `new ListModel<K, T>(keyFunction)` creates a ListModel using a given function to determine the key of an item. You can add items with a collection initializer too.

    ```csharp
    class Person { 
        public string Username; 
        public string Name; 
    }
    
    var myPeopleColl =
        new ListModel<string, Person>(p => p.Username) {
            new Person { Username = "johnny87", Name = "John" },
            new Person { Username = "theana12", Name = "Ana" }
        };
    ```

Every following example will assume the above `Person` type and `myPeopleColl` model.

#### Modifying ListModels

Once you have a ListModel, you can modify its contents like so:

* [`.Add`](/api/v4.1/WebSharper.UI.ListModel\`2#Add) inserts an item into the model. If there is already an item with the same key, this item is replaced.

    ```csharp
    myPeopleColl.Add(new Person { Username = "mynameissam", Name = "Sam" });
    // myPeopleColl now contains John, Ana and Sam.
    
    myPeopleColl.Add(new Person { Username = "johnny87", Name = "Johnny" });
    // myPeopleColl now contains Johnny, Ana and Sam.
    ```

* [`.RemoveByKey`](/api/v4.1/WebSharper.UI.ListModel\`2#RemoveByKey) removes the item from the model that has the given key. If there is no such item, then nothing happens.

    ```csharp
    myPeopleColl.RemoveByKey("theana12");
    // myPeopleColl now contains John.
    
    myPeopleColl.RemoveByKey("chloe94");
    // myPeopleColl now contains John.
    ```

* [`.Remove`](/api/v4.1/WebSharper.UI.ListModel\`2#Remove) removes the item from the model that has the same key as the given item. It is effectively equivalent to `listModel.RemoveByKey(getKey(x))`, where `getKey` is the key function passed to the `ListModel` constructor and `x` is the argument to `Remove`.

    ```csharp
    myPeopleColl.Remove(new Person { Username = "theana12", Name = "Another Ana" });
    // myPeopleColl now contains John.
    ```

* [`.Set`](/api/v4.1/WebSharper.UI.ListModel\`2#Set) sets the entire contents of the model, discarding the previous contents.

    ```csharp
    myPeopleColl.Set(new[] {
         new Person { Username = "chloe94", Name = "Chloe" },
         new Person { Username = "a13x", Name = "Alex" }
    });
    // myPeopleColl now contains Chloe, Alex.
    ```

* [`.Clear`](/api/v4.1/WebSharper.UI.ListModel\`2#Clear) removes all items from the model.

    ```csharp
    myPeopleColl.Clear();
    // myPeopleColl now contains no items.
    ```

* [`.UpdateBy`](/api/v4.1/WebSharper.UI.ListModel\`2#UpdateBy) updates the item with the given key. If the function returns `null` or the item is not found, nothing is done. If the function does return a value, it must wrap it in an `FSharpOption` to disambiguate valid `null` values from missing values. You can do this the easiest with the `FSharpConvert.Some` helper.

    ```csharp
    myPeople.UpdateBy("theana12", u => FSharpConvert.Some (new Person { UserName = u, Name = "The Real Ana" }));
    // myPeopleColl now contains John, The Real Ana.
    
    myPeople.UpdateBy("johnny87", u => null); 
    // myPeopleColl now contains John, The Real Ana.
    ```

* [`.UpdateAll`](/api/v4.1/WebSharper.UI.ListModel\`2#UpdateAll) updates all the items of the model. If the function returns `null`, the corresponding item is unchanged.

    ```csharp
    myPeople.UpdateAll(u => 
        u.Username.Contains("ana")
            ? FSharpConvert.Some (new Person { UserName = u, Name = "The Real Ana" })
            : null
    )
    // myPeopleColl now contains John, The Real Ana.
    ```

#### Reactively observing ListModels

The main purpose for using a ListModel is to be able to reactively observe it. Here are the ways to do so:

* [`.View`](/api/v4.1/WebSharper.UI.ListModel\`2#View) gives a `View<IEnumerable<T>>` that reacts to changes to the model. The following example creates an HTML list of people which is automatically updated based on the contents of the model.

    ```csharp
    var myPeopleList =
        myPeopleColl.View
            .Doc(people =>
                ul(
                    Doc.Concat(people.Select(p => li(p.Name)))
                )
            );
    ```

* [`.ViewState`](/api/v4.1/WebSharper.UI.ListModel\`2#ViewState) is equivalent to `View`, except that it returns a `View<ListModelState<T>>`. Here are the differences:

    * `ViewState` provides better performance.
    * `ListModelState<T>` implements `IEnumerable<T>`, but it additionally provides indexing and length of the sequence.
    * However, a `ViewState` is only valid until the next change to the model.
    
    As a summary, it is generally better to use `ViewState`. You only need to choose `View` if you need to store the resulting sequence separately.

* [`.Map`](/api/v4.1/WebSharper.UI.ListModel\2#Map\`\`1) reactively maps a function on each item. It is optimized so that the mapping function is not called again on every item when the content changes, but only on changed items. There are two variants:

    * `Map(Func<T, V> f)` assumes that the item with a given key does not change.
    
        ```csharp
        var myDoc =
            myPeopleColl.Map(p => {
                Console.Log(p.Username);
                return p.Name;
            })
                .Doc(Doc.Concat)
                .RunAppend(JS.Document.Body);
        // Logs johnny87, theana12
        // Displays John, Ana
        
        myPeopleColl.Add(new Person { Username = "mynameissam", Name = "Sam" });
        // Logs mynameissam
        // Displays John, Ana, Sam
        
        myPeopleColl.Add(new Person { Username = "johnny87", Name = "Johnny" });
        // Logs nothing, since no key has been added
        // Displays John, Ana, Sam (unchanged)
        ```

    * `Map(Func<K, View<T>, V> f)` additionally observes changes to individual items that are updated.
    
        ```csharp
        var myDoc =
            myPeopleColl.Map((k, vp) => {
                Console.Log(k);
                return p(vp.Map(p => p.Name));
            })
                .Doc(Doc.Concat)
                .RunAppend(JS.Document.Body);
        // Logs johnny87, theana12
        // Displays John, Ana
        
        myPeopleColl.Add({ Username = "mynameissam"; Name = "Sam" });
        // Logs mynameissam
        // Displays John, Ana, Sam
        
        myPeopleColl.Add({ Username = "johnny87"; Name = "Johnny" });
        // Logs nothing, since no key has been added
        // Displays Johnny, Ana, Sam (changed!)
        ```

    Note that in both cases, only the current state is kept in memory: if you remove an item and insert it again, the function will be called again.

* [`.Doc`](/api/v4.1/WebSharper.UI.ListModel\2#Doc) is similar to `Map`, but the function must return a `Doc` and the resulting Docs are concatenated. It is equivalent to what we did above in the example for `Map`: `listModel.Map(f) |> Doc.BindView Doc.Concat`.

* [`.TryFindByKeyAsView`](/api/v4.1/WebSharper.UI.ListModel\`2#TryFindByKeyAsView) gives a View on the item that has the given key, or `None` if it is absent.

    ```csharp
    var showJohn =
        myPeopleColl.TryFindByKeyAsView("johnny87")
            .Doc (u =>
                u is null
                    ? text "He is not here."
                    : text $"He is here, and his name is {u.Name.Value}."
            );
    ```

* [`.FindByKeyAsView`](/api/v4.1/WebSharper.UI.ListModel\`2#FindByKeyAsView) is equivalent to `TryFindByKeyAsView`, except that when there is no item with the given key, an exception is thrown.

* [`.ContainsKeyAsView`](/api/v4.1/WebSharper.UI.ListModel\`2#ContainsKeyAsView) gives a View on whether there is an item with the given key. It is equivalent to (but more optimized than):

    ```csharp
    listModel.TryFindByKeyAsView(k).Map(v => !(v is null))
    ```

#### Inserting ListModels in the Doc

To show the contents of a ListModel in your document, you can of course use one of the above View methods and pass it to `Doc.BindView`.

## Routing

If you have a `WebSharper.Sitelets.Router<T>` value, it can be shared between server and client. A router encapsulates two things: parsing an URL path to an abstract value and writing a value as an URL fragment. So this allows generating links safely on both client  When initializing a page client-side, you can decide to install a custom click handler for your page which recognizes some or all local links to handle without browser navigation.

### Install client-side routing

There are 3 scenarios for client-side routing which WebSharper routing makes possible:
* For creating single-page applications, when browser refresh is never wanted, `.Install` creates a global click handler that prevents default behavior of `<a>` links on your page pointing to a local URL.
* If you want client-side navigation only between some part of the whole site map covered by the router, you can use `.Map` or `.Filter` to restrict the set of endpoint values handled before `.Install`. This creates a global click handler that now only override behavior of local links which can be mapped to the subset of endpoints that are handled in the client. For example you can make navigating between `yoursite.com/profile/...` links happen with client-side routing, but any links that would point out of `/profile/...` are still doing browser navigation automatically.
* If you want to have client-side routing on a sub-page that the server knows nothing about, `.InstallHash` subscribes to `window.location.hash` changes only. You can use a router that is specific to that single sub-page.

In all cases, the `Install` function used returns a `Var`, which you can use to map the visible content of your page from. It has a two way binding to the URL: link or forward/back navigation changes the value of the `Var`, and setting the value does a client-side navigation which also updates the URL automatically.

Example for `.Install`, using the router value introduced in the [Sitelets documentation](sitelets.md):
```csharp
[<SPAEntryPoint>]
public static void ClientMain() =
    var location = rPages.Install(Home.Instance);
    location.View.Doc(loc => {
        switch (loc)
        {
            case Home h: return div("This is the home page");
            case Contact p: return div($"Contact name:{p.Name}, age:{p.Age}");
            default: return null;
        }
    }).RunAppend(JS.Document.Body);    
```
First argument (`Home.Instance`) specifies which page value to fall back on if the URL path cannot be parsed (although this won't happen if you set up your server-side correctly), which could be a home or an error page.

Also, you need to make sure that your router value is `[JavaScript]` annotated (or a containing class or the assembly is), so that it is available for cross-tier use.

`.InstallHash` have the same signature as `.Install`, the only difference is that URLs would look like `yoursite.com/#/contact/Bob/32`.

Example for `.Map` and `.Install`:
```csharp
public static void ClientMain() =
    var location =
        rPages.Map(
            p => p as Contact,
            e => e
        ).Install(new Contact { Name = "unknown", Age = 0 });
    location.View.Doc(c => 
        div($"Contact name:{p.Name}, age:{p.Age}")
    ).RunAppend(JS.Document.Body);
```
Here we only install a click handler for the contact pages, which means that a link to root will be a browser navigation, but links between contacts work fully on the client. The first function argument maps a full page value to an option of a value that we handle, and the second function maps this back to a full page value. So instead of a `Var<Pages>` here we get only a `Var<Person>`.

In a real world application, usually you would have some `.MapAsync` from the `location` variable, to pull some data related to the subpage from the server by an RPC call, and exposing that as content:

```csharp
[<Remote>] // this is a server-side function exposed as a WebSharper RPC
public static async Task<ContactDetails> GetContactDetails(Person p) { ... }

public static void ClientMain() =
    var location = // ...
    var contactDetails = location.View.MapAsync(Server.GetContactDetails);
    contactDetails.View.Doc(fun p -> 
        // show contact details
    ).RunAppend(JS.Document.Body);
```

You can navigate programmatically with `location.Value <- newLoc`, `location |> Var.Set newLoc` or `location := newLoc` (if you have `open WebSharper.UI.Next.Notation`). 

