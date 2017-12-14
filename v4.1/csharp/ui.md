# Functional Reactive Programming and HTML

WebSharper.UI is a library providing a novel, pragmatic and convenient approach to UI reactivity. It includes:

* An [HTML library](#html) usable both from the server side and from the client side, which you can use to build HTML pages either by calling F# functions to create elements, or by instantiating template HTML files.
* A [reactive layer](#reactive) for expressing user inputs and values computed from them as time-varying values. This approach is related to Functional Reactive Programming (FRP). This reactive system integrates with the HTML library to create reactive documents. If you are familiar with Facebook React, then you will find some similarities with this approach: instead of explicitly inserting, modifying and removing DOM nodes, you return a value that represents a DOM tree based on inputs. The main difference is that these inputs are nodes of the reactive system, rather than a single state value associated with the component.

This page is an overview of the capabilities of WebSharper.UI. You can also check [the full reference of all the API types and modules](http://developers.websharper.com/api/WebSharper.UI).

## Using HTML

WebSharper.UI's core type for HTML construction is [`Doc`](/api/WebSharper.UI.Doc). A Doc can represent a single DOM node (element, text), but it can also be a sequence of zero or more nodes. This allows you to treat equally any HTML snippet that you want to insert into a document, whether it consists of a single element or not.

Additionally, client-side Docs can be reactive. A same Doc can consist of different elements at different moments in time, depending on user input or other variables. See [the reactive section](#reactive) to learn more about this.

<a name="html"></a>
### Constructing HTML

#### Docs

The main way to create `Doc`s is to use the static methods from the `WebSharper.UI.CSharp.Html` or `WebSharper.UI.CSharp.Client.Html` classes. The difference is that the first contains server-side `Doc` constructors, including taking event handlers as a LINQ expression, to be auto-translated and ran on the client. The second contains server-side `Doc` constructors, including reactive functionalities. The composing static `Doc` content is available on both with the same syntax.

Every HTML element has a dedicated method, such as `div` or `p`, which takes a any number of `object` parameters, which can represent attributes or child elements. The following parameter values are accepted:

On the server:

* value of type `WebSharper.UI.Doc`, which can represent one, none or multiple child nodes.
* a string value will be added as a text node.
* `null` will be treated as empty content.
* an object implementing `WebSharper.INode` (server only), which exposes a method to write to the response content directly.
* a Linq expression of type `Expr<IControlBody>` (server only), which creates a placeholder in the returned HTML, to be replaced by client-side code.
* any other will be converted to a string with its `ToString` function and included as a text node.

On the client:

* value of type `WebSharper.UI.Doc`, which can represent one, none or multiple child nodes.
* a string value will be added as a text node.
* `null` will be treated as empty content.
* a `Dom.Element` will be added as a static child element.

* a `View<T>`, where `T` can be any type

client

        | :? Doc as d -> d
        | :? string as t -> Doc.TextNode t
        | :? Element as e -> Doc'.Static e |> As<Doc>        
        | :? Function as v ->
            Doc'.EmbedView (
                (As<View<_>>v).Map (As Doc'.ToMixedDoc)
            ) |> As<Doc>
        | :? Var<obj> as v ->
            Doc'.EmbedView (
                v.View.Map (As Doc'.ToMixedDoc)
            ) |> As<Doc>
        | null -> Doc.Empty
        | o -> Doc.TextNode (string o)


```csharp
using WebSharper.UI.CSharp.Html;

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

One thing to note is that the tag functions described above actually return a value of type [`Elt`](/api/WebSharper.UI.Elt), which is a subtype of `Doc` that is guaranteed to always consist of exactly one element and provides additional APIs such as [`Dom`](/api/WebSharper.UI.Elt#Dom) to get the underlying `Dom.Element`.

Additional functions in the [`Doc`](/api/WebSharper.UI.Doc) can create or combine Docs:

* `doc` takes a number of object arguments, same as HTML element construction methods, but does not wrap them in an element, but returns a `Doc` that contains the given content as consecutive nodes.

* [`Doc.Empty`](/api/WebSharper.UI.Doc#Empty) creates a Doc consisting of zero nodes, optimized version of `doc()`. Inside parameter lists of HTML element constructors, you can pass a `null` to have empty content, but when you are returning a `Doc`, it is recommended to use `Doc.Empty` so that instance methods can be called on the return value without a null error.

    ```csharp
    public Doc WelcomeText(bool show) =>
        show ? div("Hello!") : Doc.Empty;
        
    // <div>Hello!</div>
    //
    // or nothing.
    ```

* [`Doc.Append`](/api/WebSharper.UI.Doc#Append) creates a Doc consisting of the concatenation of two Docs, optimized version of `doc(x, y)`.

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

* [`Doc.Concat`](/api/WebSharper.UI.Doc#Concat) generalizes `Append` by concatenating a sequence of Docs. Optimized version of `doc` that does not need to do type checks.

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

* [`Doc.Element`](/api/WebSharper.UI.Doc#Element) creates an element with the given name, attributes and children. It is equivalent to the function with the same name from the `Html` module. This function is useful if the tag name is only known at runtime, or if you want to create a non-standard element that isn't available in `Html`. The following example creates a header tag of a given level (`h1`, `h2`, etc).

    ```csharp
    public Doc MakeHeader(int level, string content) =>
        Doc.Element("h" + level, new[] {}, new[] { text(content) });
        
    // <h1>content...</h1>
    // or
    // <h2>content...</h2>
    // or etc.
    ```

* [`Doc.Verbatim`](/api/WebSharper.UI.Doc#Verbatim) creates a Doc from plain HTML text.  
    **Security warning:** this function does not perform any checks on the contents, and can be a code injection vulnerability if used improperly. We recommend avoiding it unless absolutely necessary, and properly sanitizing user inputs if you do use it. If you simply want to use HTML syntax instead of F# functions, take a look at [templating](#templating).

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

To create attributes, use corresponding functions from the [`attr`](/api/WebSharper.UI.Html.attr) nested class inside the `Html` static classes.

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
    ]

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

Like `Doc`, a value of type `Attr` can represent zero, one or more attributes. The functions in the [`Attr`](/api/WebSharper.UI.Attr) module can create such non-singleton attributes.

* [`Attr.Empty`](/api/WebSharper.UI.Attr#Empty) creates an empty attribute.

    ```csharp
    public Doc ValueAttr(string v) =>
        v is null ? Attr.Empty : attr.value(v);
         
    // value="value of v"
    //
    // or nothing.
    ```

* [`Attr.Append`](/api/WebSharper.UI.Attr#Append) combines two attributes.

    ```csharp
    var passwordAttr =
        Attr.Append (attr.type("password"), attr.placeholder("Password"))

    // type="password" placeholder="Password"
    ```

* [`Attr.Concat`](/api/WebSharper.UI.Attr#Concat) combines a sequence of attributes.

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

* [`Attr.Create`](/api/WebSharper.UI.Attr#Create) creates a single attribute. It is equivalent to the function with the same name from the `attr` module. This function is useful if the attribute name is only known at runtime, or if you want to create a non-standard attribute that isn't available in `attr`.

    ```csharp
    let eltWithNonStandardAttr =
        div(Attr.Create("my-attr", "my-value"), "...")
        
    // <div my-attr="my-value">...</div>
    ```

#### Event handlers

A special kind of attribute is event handlers. They can be created using functions from the [`on`](/api/WebSharper.UI.Html#on) nested static class.
Furthermore, some element constructing methods define an overload to add an event handler to a default event directly, like a `click` for a `button`.

```csharp
let myButton =
    button(on.click((el, ev) => JS.Window.Alert("Hi!")), "Click me!");

let myButtonShorterForm =
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
