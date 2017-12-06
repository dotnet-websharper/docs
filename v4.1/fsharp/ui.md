# Functional Reactive Programming and HTML

WebSharper.UI is a library providing a novel, pragmatic and convenient approach to UI reactivity. It includes:

* An [HTML library](#html) usable both from the server side and from the client side, which you can use to build HTML pages either by calling F# functions to create elements, or by instantiating template HTML files.
* A [reactive layer](#reactive) for expressing user inputs and values computed from them as time-varying values. This approach is related to Functional Reactive Programming (FRP). This reactive system integrates with the HTML library to create reactive documents. If you are familiar with Facebook React, then you will find some similarities with this approach: instead of explicitly inserting, modifying and removing DOM nodes, you return a value that represents a DOM tree based on inputs. The main difference is that these inputs are nodes of the reactive system, rather than a single state value associated with the component.
* A [declarative animation system](#animation) for the client-side HTML layer.

This page is an overview of the capabilities of WebSharper.UI. You can also check [the full reference of all the API types and modules](http://developers.websharper.com/api/WebSharper.UI).

## Using HTML

WebSharper.UI's core type for HTML construction is [`Doc`](/api/WebSharper.UI.Doc). A Doc can represent a single DOM node (element, text), but it can also be a sequence of zero or more nodes. This allows you to treat equally any HTML snippet that you want to insert into a document, whether it consists of a single element or not.

Additionally, client-side Docs can be reactive. A same Doc can consist of different elements at different moments in time, depending on user input or other variables. See [the reactive section](#reactive) to learn more about this.

### Constructing HTML

#### Docs

The main means of creating Docs is by using the functions in the [`WebSharper.UI.Html`](/api/WebSharper.UI.Html) module. Every HTML element has a dedicated function, such as [`div`](/api/WebSharper.UI.Html#div) or [`p`](/api/WebSharper.UI.Html#p), which takes a sequence of [attributes](#attr) (of type [`Attr`](/api/WebSharper.UI.Attr)) and a sequence of child nodes (of type `Doc`). Additionally, the [`text`](/api/WebSharper.UI.Html#text) function creates a text node.

```fsharp
open WebSharper.UI.Html

let myDoc =
    div [] [
        h1 [] [ text "Functional Reactive Programming and HTML" ]
        p [] [ text "WebSharper.UI is a library providing a novel, pragmatic and convenient approach to UI reactivity. It includes:" ]
        ul [] [
            li [] [ text "..." ]
        ]
    ]

// <div>
//   <h1>Functional Reactive Programming and HTML</h1>
//   <p>WebSharper.UI is a library providing a novel, pragmatic and convenient
//      approach to UI reactivity. It includes:</p>
//   <ul>
//     <li>...</li>
//   </ul>
// </div>
```

Some HTML tags, such as `option`, collide with standard library names and are therefore only located in the [`Tags`](/api/WebSharper.UI.Html.Tags) submodule.

```fsharp
let myDropdown =
    select [] [
        Tags.option [] [ text "First choice" ]
        Tags.option [] [ text "Second choice" ]
        Tags.option [] [ text "Third choice" ]
    ]

// <select>
//   <option>First choice</option>
//   <option>Second choice</option>
//   <option>Third choice</option>
// </select>
```

One thing to note is that the tag functions described above actually return a value of type [`Elt`](/api/WebSharper.UI.Elt), which is a subtype of `Doc` that is guaranteed to always consist of exactly one element and provides [additional APIs](#elt). This subtyping means that you will sometimes need to upcast the result of such a function with `:> Doc` to appease the compiler; you can see an example of this below in the example for `Doc.Empty`.

Additional functions in the [`Doc`](/api/WebSharper.UI.Doc) can create or combine Docs:

* [`Doc.Empty`](/api/WebSharper.UI.Doc#Empty) creates a Doc consisting of zero nodes. This can be useful for example when you may not need to insert an element depending on a condition.

    ```fsharp
    let myForm (withDropdown: bool) =
        form [] [
            input [ attr.name "name" ] []
            (if withDropdown then myDropdown :> Doc else Doc.Empty)
        ]
        
    // <form>
    //   <input name="name" />
    // </form>
    //
    // or:
    //
    // <form>
    //   <input name="name" />
    //   <!-- ... contents of myDropdown here ... -->
    // </form>
    ```

* [`Doc.Append`](/api/WebSharper.UI.Doc#Append) creates a Doc consisting of the concatenation of two Docs.

    ```fsharp
    let titleAndBody =
        Doc.Append
            (h1 [] [ text "Functional Reactive Programming and HTML" ])
            (p [] [ text "WebSharper.UI is a library providing ..." ])
            
    // <h1>Functional Reactive Programming and HTML</h1>
    // <p>WebSharper.UI is a library providing ...</p>
    ```

For the mathematically enclined, the functions `Doc.Empty` and `Doc.Append` make Docs a monoid.

* [`Doc.Concat`](/api/WebSharper.UI.Doc#Concat) generalizes `Append` by concatenating a sequence of Docs.

    ```fsharp
    let thisPage =
        Doc.Concat [
            h1 [] [ text "Functional Reactive Programming and HTML" ]
            p [] [ text "WebSharper.UI is a library providing ..." ]
            ul [] [
                li [] [ text "..." ]
            ]
        ]
        
    // <h1>Functional Reactive Programming and HTML</h1>
    // <p>WebSharper.UI is a library providing ...</p>
    // <ul>
    //   <li>...</li>
    // </ul>
    ```

* [`Doc.Element`](/api/WebSharper.UI.Doc#Element) creates an element with the given name, attributes and children. It is equivalent to the function with the same name from the `Html` module. This function is useful if the tag name is only known at runtime, or if you want to create a non-standard element that isn't available in `Html`. The following example creates a header tag of a given level (`h1`, `h2`, etc).

    ```fsharp
    let makeHeader (level: int) (content: string) =
        Doc.Element ("h" + string level) [] [ text content ]
        
    // <h1>content...</h1>
    // or
    // <h2>content...</h2>
    // or etc.
    ```

* [`Doc.Verbatim`](/api/WebSharper.UI.Doc#Verbatim) creates a Doc from plain HTML text.  
    **Security warning:** this function does not perform any checks on the contents, and can be a code injection vulnerability if used improperly. We recommend avoiding it unless absolutely necessary, and properly sanitizing user inputs if you do use it. If you simply want to use HTML syntax instead of F# functions, take a look at [templating](#templating).

    ```fsharp
    let plainDoc =
        Doc.Verbatim "
            <h1 onclick=\"alert('And it is unsafe!')\">
                This is plain HTML!
            </h1>"

    // <h1 onclick="alert('And it is unsafe!')">
    //     This is plain HTML!
    // </h1>
    ```

<a name="attr"></a>
#### Attrs

To create attributes, use corresponding functions from the [`attr`](/api/WebSharper.UI.Html.attr) submodule.

```fsharp
let myFormControl =
    select [ attr.name "mySelect" ] [
        Tags.option [ attr.value "first" ] [ text "First choice" ]
        Tags.option [ attr.value "second" ] [ text "Second choice" ]
        Tags.option [
            attr.value "third"
            attr.selected "selected"
        ] [ text "Third choice" ]
    ]

// <select name="mySelect">
//   <option value="first">First choice</option>
//   <option value="second">Second choice</option>
//   <option value="third" selected="selected">Third choice</option>
// </select>
```

Some attributes, notably `class` and `type`, are also F# keywords, so they need to be wrapped in double backquotes.

```fsharp
let myMain =
    div [ attr.``class`` "main" ] [ text "..." ]

// <div class="main">...</div>
```

HTML5 also defines any attribute whose names starts with `data-` as a valid custom attribute. You can create such an attribute using the function `data-` from module `attr` (backquoted since it contains a non-standard character).

```fsharp
let myEltWithData =
    div [ attr.``data-`` "uid" "myDiv" ] [ text "..." ]

// <div data-uid="myDiv">...</div>
```

Like `Doc`, a value of type `Attr` can represent zero, one or more attributes. The functions in the [`Attr`](/api/WebSharper.UI.Attr) module can create such non-singleton attributes.

* [`Attr.Empty`](/api/WebSharper.UI.Attr#Empty) creates an empty attribute. This can be useful for example when you may not need to insert an attribute depending on a condition.

    ```fsharp
    let makeInput (initialValue: option<string>) =
        let valueAttr =
            match initialValue with
            | Some v -> attr.value v
            | None -> Attr.Empty
        input [ valueAttr ] []
        
    // <input value="initialValue..." />
    // or
    // <input />
    ```

* [`Attr.Append`](/api/WebSharper.UI.Attr#Append) combines two attributes.

    ```fsharp
    let passwordAttr =
        Attr.Append (attr.``type`` "password") (attr.placeholder "Password")

    // type="password" placeholder="Password"
    ```

* [`Attr.Concat`](/api/WebSharper.UI.Attr#Concat) combines a sequence of attributes.

    ```fsharp
    let passwordAttr =
        Attr.Concat [
            attr.``type`` "password"
            attr.placeholder "Password"
            attr.``class`` "pw-input"
        ]

    // type="password" placeholder="Password" class="pw-input"
    ```

* [`Attr.Create`](/api/WebSharper.UI.Attr#Create) creates a single attribute. It is equivalent to the function with the same name from the `attr` module. This function is useful if the attribute name is only known at runtime, or if you want to create a non-standard attribute that isn't available in `attr`.

    ```fsharp
    let eltWithNonStandardAttr =
        div [ Attr.Create "my-attr" "my-value" ] [ text "..." ]
        
    // <div my-attr="my-value">...</div>
    ```

#### Event handlers

A special kind of attribute is event handlers. They can be created using functions from the [`on`](/api/WebSharper.UI.Html#on) submodule.

### HTML on the client

To insert a Doc into the document on the client side, use the `Doc.Run*` family of functions from the module [`WebSharper.UI.Client`](/api/WebSharper.UI.Client). Each of these functions has two variants: one directly taking a DOM [`Element`](/api/WebSharper.JavaScript.Dom.Element) or [`Node`](/api/WebSharper.JavaScript.Dom.Node), and the other suffixed with `ById` taking the id of an element as a string.

* [`Doc.Run`](/api/WebSharper.UI.Doc#Run) and [`Doc.RunById`](/api/WebSharper.UI.Doc#RunById) insert a given Doc as the child(ren) of a given DOM element. Note that it replaces the existing children, if any.

    ```fsharp
    open WebSharper.JavaScript
    open WebSharper.UI
    open WebSharper.UI.Client
    open WebSharper.UI.Html

    let Main () =
        div [] [ text "This goes into #main." ]
        |> Doc.RunById "main"
        
        p [] [ text "This goes into the first paragraph with class my-content." ]
        |> Doc.Run (JS.Document.QuerySelector "p.my-content")
    ```

* [`Doc.RunAppend`](/api/WebSharper.UI.Doc#RunAppend) and [`Doc.RunAppendById`](/api/WebSharper.UI.Doc#RunAppendById) insert a given Doc as the last child(ren) of a given DOM element.

* [`Doc.RunPrepend`](/api/WebSharper.UI.Doc#RunPrepend) and [`Doc.RunPrependById`](/api/WebSharper.UI.Doc#RunPrependById) insert a given Doc as the first child(ren) of a given DOM element.

* [`Doc.RunAfter`](/api/WebSharper.UI.Doc#RunAfter) and [`Doc.RunAfterById`](/api/WebSharper.UI.Doc#RunAfterById) insert a given Doc as the next sibling(s) of a given DOM node.

* [`Doc.RunBefore`](/api/WebSharper.UI.Doc#RunBefore) and [`Doc.RunBeforeById`](/api/WebSharper.UI.Doc#RunBeforeById) insert a given Doc as the previous sibling(s) of a given DOM node.

* [`Doc.RunReplace`](/api/WebSharper.UI.Doc#RunReplace) and [`Doc.RunReplaceById`](/api/WebSharper.UI.Doc#RunReplaceById) insert a givev Doc instead of a given DOM node.

### HTML on the server

On the server side, using [sitelets](sitelets.md), you can create HTML pages from Docs by passing them to the `Body` or `Head` arguments of `Content.Page`.

```fsharp
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Html

let MyPage (ctx: Context<EndPoint>) =
    Content.Page(
        Title = "Welcome!",
        Body = [
            h1 [] [ text "Welcome!" ]
            p [] [ text "This is my home page." ]
        ]
    )
```

By opening `WebSharper.UI.Server`, you can also just pass a full page to `Content.Page`. This is particularly useful together with [templates](#templating).

```fsharp
let MyPage (ctx: Context<EndPoint>) =
    Content.Page(
        html [] [
            head [] [ title [] [ text "Welcome!" ] ]
            body [] [
                h1 [] [ text "Welcome!" ]
                p [] [ text "This is my home page." ]
            ]
        ]
    )
```

To include client-side elements inside a page, use the `ClientSide` method of the Sitelets context.

<a name="templating"></a>
## HTML Templates

WebSharper.UI's syntax for creating HTML is compact and convenient, but sometimes you do need to include a plain HTML file in a project. It is much more convenient for designing to have a .html file that you can touch up and reload your application without having to recompile it. This is what Templates provide. Templates are HTML files that can be loaded by WebSharper.UI, and augmented with special elements and attributes that provide additional functionality:

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

```fsharp
// my-template.html:
// <div>
//   <h1>Welcome!</h1>
//   <p>Welcome to my site.</p>
// </div>

open WebSharper.UI.Templating

type MyTemplate = Template<"my-template.html">

let myPage = MyTemplate().Doc()

// equivalent to:
// let myPage =
//     div [] [
//         h1 [] [ text "Welcome!" ]
//         p [] [ text "Welcome to my site." ]
//     ]
```

Note that the template doesn't have to be a full HTML document, but can simply be a snippet or sequence of snippets. This is particularly useful to build a library of widgets using [inner templates](#inner-templates).

You can also declare a template from multiple files at once using a comma-separated list of file names. In this case, the template for each file is a nested class named after the file, truncated of its file extension.

```fsharp
// my-template.html:
// <div>
//   <h1>Welcome!</h1>
//   <p>Welcome to my site.</p>
// </div>

// second-template.html:
// <div>
//   <h2>This is a section.</h2>
//   <p>And this is its content.</p>
// </div>

open WebSharper.UI.Templating

type MyTemplate = Template<"my-template.html">

let myPage =
    Doc.Concat [
        MyTemplate.``my-template``().Doc()
        MyTemplate.``second-template``().Doc()
    ]

// equivalent to:
// let myPage =
//     Doc.Concat [
//         div [] [
//             h1 [] [ text "Welcome!" ]
//             p [] [ text "Welcome to my site." ]
//         ]
//         div [] [
//             h2 [] [ text "This is a section." ]
//             p [] [ text "And this is its content." ]
//         ]
//    ]
```

### Holes

You can add holes to your template that will be filled by F# code. Each hole has a name. To fill a hole in F#, call the method with this name on the template instance before finishing with `.Doc()`.

* `${HoleName}` creates a `string` hole. You can use it in text or in the value of an attribute.

    ```fsharp
    // my-template.html:
    // <div style="background-color: ${Color}">
    //   <h1>Welcome, ${Name}!</h1>
    //   <!-- You can use the same hole name multiple times,
    //        and they will all be filled with the same F# value. -->
    //   <p>This div's color is ${Color}.</p>
    // </div>
    
    let myPage =
        MyTemplate()
            .Color("red")
            .Name("my friend")
            .Doc()

    // result:
    // <div style="background-color: red">
    //   <h1>Welcome, my friend!</h1>
    //   <!-- You can use the same hole name multiple times,
    //        and they will all be filled with the same F# value. -->
    //   <p>This div's color is red.</p>
    // </div>
    ```
    
    On the client side, this hole can also be filled with a `View<string>` (see [reactive](#reactive)) to include dynamically updated text content.

* The attribute `ws-replace` creates a `Doc` or `seq<Doc>` hole. The element on which this attribute is set will be replaced with the provided Doc(s). The name of the hole is the value of the `ws-replace` attribute.

    ```fsharp
    // my-template.html:
    // <div>
    //   <h1>Welcome!</h1>
    //   <div ws-replace="Content"></div>
    // </div>
    
    let myPage =
        MyTemplate()
            .Content(p [] [ text "Welcome to my site." ])
            .Doc()

    // result:
    // <div>
    //   <h1>Welcome!</h1>
    //   <p>Welcome to my site.</p>
    // </div>
    ```

* The attribute `ws-hole` creates a `Doc` or `seq<Doc>` hole. The element on which this attribute is set will have its _contents_ replaced with the provided Doc(s). The name of the hole is the value of the `ws-hole` attribute.

    ```fsharp
    // my-template.html:
    // <div>
    //   <h1>Welcome!</h1>
    //   <div ws-hole="Content"></div>
    // </div>
    
    let myPage =
        MyTemplate()
            .Content(p [] [ text "Welcome to my site." ])
            .Doc()

    // result:
    // <div>
    //   <h1>Welcome!</h1>
    //   <div>
    //       <p>Welcome to my site.</p>
    //   </div>
    // </div>
    ```

* The attribute `ws-attr` creates an `Attr` or `seq<Attr>` hole. The name of the hole is the value of the `ws-attr` attribute.

    ```fsharp
    // my-template.html:
    // <div ws-attr="MainDivAttr">
    //   <h1>Welcome!</h1>
    //   <p>Welcome to my site.</p>
    // </div>
    
    let myPage =
        MyTemplate()
            .MainDivAttr(attr.``class`` "main")
            .Doc()

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

    ```fsharp
    // my-template.html:
    // <div>
    //   <input ws-var="Name" />
    //   <div>Hi, ${Name}!</div>
    // </div>

    let myPage =
        let varName = Var.Create ""
        MyTemplate()
            .Name(varName)
            .Doc()

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

    ```fsharp
    // my-template.html:
    // <div>
    //   <input ws-var="Name" />
    //   <button ws-onclick="Click">Ok</button>
    // </div>
    
    let myPage =
        MyTemplate()
            .Click(fun t -> JS.Alert("Hi, " + t.Vars.Name.Value))
            .Doc()
    ```

<a name="inner-templates"></a>
### Inner templates

To create a template for a widget (as opposed to a full page), you can put it in its own dedicated template file, but another option is to make it an inner template. An inner template is a smaller template declared inside a template file using the following syntax:

* The `ws-template` attribute declares that its element is a template whose name is the value of this attribute.
* The `ws-children-template` attribute declares that the children of its element is a template whose name is the value of this attribute.

Inner templates are available in F# as a nested class under the main provided type.

```fsharp
// my-template.html:
// <div ws-attr="MainAttr">
//   <div ws-replace="InputFields"></div>
//   <div ws-template="Field" class="field-wrapper">
//     <label for="${Id}">${Which} Name: </label>
//     <input ws-var="Var" placeholder="${Which} Name" name="${Id}" />
//   </div>
// </div>

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

```fsharp
// my-template.html:
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

type MyTemplate = Template<"my-template.html">

let myForm =
    let firstName = Var.Create ""
    let lastName = Var.Create ""
    MyTemplate()
        .FirstVar(firstName)
        .LastVar(lastName)
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

The possible values for `serverLoad` are:

* `ServerLoad.WhenChanged` (default): The runtime sets up a file watcher on the template file, and reloads it whenever it is edited.
* `ServerLoad.Once`: The template file is loaded on first use and never reloaded.
* `ServerLoad.PerRequest`: The template file is reloaded every time it is needed. We recommend against this option for performance reasons.

### Accessing the template's model

Templates allow you to access their "model", ie the set of all the reactive `Var`s that are bound to it, whether passed explicitly or automatically created for its `ws-var`s. It is accessible in two ways:

* In event handlers, it is available as the `Vars` property of the handler argument.
* From outside the template: instead of finishing the instanciation of a template with `.Doc()`, you can call `.Create()`. This will return a `TemplateInstance` with two properties: `Doc`, which returns the template itself, and `Vars`, which contains the Vars.

    ```fsharp
    // my-template.html:
    // <div>
    //   <input ws-var="Name" />
    //   <div>Hi, ${Name}!</div>
    // </div>

    let myInstance = MyTemplate().Create()
    myInstance.Vars.Name <- "John Doe"
    let myDoc = myInstance.Doc

    // result:
    // <div>
    //   <input value="John Doe" />
    //   <div>Hi, John Doe!</div>
    // </div>
    ```

### Mixing client code in server-side templates

It is possible to include some client-side functionality when creating a template on the server side.

* If you use `ws-var="VarName"`, the corresponding Var will be created on the client on page startup. However, passing a Var using `.VarName(myVar)` is not possible, since it would be a server-side Var.

* Event handlers (such as `ws-onclick="EventName"`) work fully if you pass an anonymous function: `.EventName(fun e -> ...)`. The body of this function will be compiled to JavaScript. You can also pass a top-level function, in this case it must be declared with `[<JavaScript>]`.

## Reactive layer

WebSharper.UI's reactive layer helps represent user inputs and other time-varying values, and define how they depend on one another.

### Vars

Reactive values that are directly set by code or by user interaction are represented by values of type [`Var<'T>`](/api/WebSharper.UI.Var\`1). Vars are similar to F# `ref<'T>` in that they store a value of type `'T` that you can get or set using the `Value` property. But they can additionally be reactively observed or two-way bound to HTML input elements.

The following are available from `WebSharper.UI.Client`:

* [`Doc.Input`](/api/WebSharper.UI.Client.Doc#Input) creates an `<input>` element with given attributes that is bound to a `Var<string>`.

    ```fsharp
    let varText = Var.Create "initial value"
    let myInput = Doc.Input [ attr.name "my-input" ] varText
    ```
    
    With the above code, once `myInput` has been inserted in the document, getting `varText.Value` will at any point reflect what the user has entered, and setting it will edit the input.

* [`Doc.IntInput`](/api/WebSharper.UI.Client.Doc#IntInput) and [`Doc.FloatInput`](/api/WebSharper.UI.Client.Doc#FloatInput) create an `<input type="number">` bound to a `Var<CheckedInput<_>>` of the corresponding type (`int` or `float`). `CheckedInput` provides access to the validity and actual user input, it is defined as follows:

    ```fsharp
    type CheckedInput<'T> =
        | Valid of value: 'T * inputText: string
        | Invalid of inputText: string
        | Blank of inputText: string
    ```

* [`Doc.IntInputUnchecked`](/api/WebSharper.UI.Client.Doc#IntInputUnchecked) and [`Doc.FloatInputUnchecked`](/api/WebSharper.UI.Client.Doc#FloatInputUnchecked) create an `<input type="number">` bound to a `Var<_>` of the corresponding type (`int` or `float`). They do not check for the validity of the user's input, which can cause wonky interactions. We recommend using `Doc.IntInput` or `Doc.FloatInput` instead.

* [`Doc.InputArea`](/api/WebSharper.UI.Client.Doc#InputArea) creates a `<textarea>` element bound to a `Var<string>`.

* [`Doc.PasswordBox`](/api/WebSharper.UI.Client.Doc#PasswordBox) creates an `<input type="password">` element bound to a `Var<string>`.

* [`Doc.CheckBox`](/api/WebSharper.UI.Client.Doc#CheckBox) creates an `<input type="checkbox">` element bound to a `Var<bool>`.

* [`Doc.CheckBoxGroup`](/api/WebSharper.UI.Client.Doc#CheckBoxGroup) also creates an `<input type="checkbox">`, but instead of associating it with a simple `Var<bool>`, it associates it with a specific `'T` in a `Var<list<'T>>`. If the box is checked, then the element is added to the list, otherwise it is removed.

    ```fsharp
    type Color = Red | Green | Blue
    
    // Initially, Green and Blue are checked.
    let varColor = Var.Create [ Blue; Green ]
    
    let mySelector =
        div [] [
            label [] [
                Doc.CheckBoxGroup [] Red varColor
                text " Select Red"
            ]
            label [] [
                Doc.CheckBoxGroup [] Green varColor
                text " Select Green"
            ]
            label [] [
                Doc.CheckBoxGroup [] Blue varColor
                text " Select Blue"
            ]
        ]
        
    // Result:
    // <div>
    //   <label><input type="checkbox" /> Select Red</label>
    //   <label><input type="checkbox" checked /> Select Green</label>
    //   <label><input type="checkbox" checked /> Select Blue</label>
    // </div>
    // Plus varColor is bound to contain the list of ticked checkboxes.
    ```

* [`Doc.Select`](/api/WebSharper.UI.Client.Doc#Select) creates a dropdown `<select>` given a list of values to select from. The label of every `<option>` is determined by the given print function for the associated value.

    ```fsharp
    type Color = Red | Green | Blue

    // Initially, Green is checked.
    let varColor = Var.Create Green

    // Choose the text of the dropdown's options.
    let showColor (c: Color) =
        sprintf "%A" c

    let mySelector =
        Doc.Select [] showColor [ Red; Green; Blue ] varColor
        
    // Result:
    // <select>
    //   <option>Red</option>
    //   <option>Green</option>
    //   <option>Blue</option>
    // </select>
    // Plus varColor is bound to contain the selected color.
    ```

* [`Doc.Radio`](/api/WebSharper.UI.Client.Doc#Radio) creates an `<input type="radio">` given a value, which sets the given `Var` to that value when it is selected.

    ```fsharp
    type Color = Red | Green | Blue
    
    // Initially, Green is selected.
    let varColor = Var.Create Green
    
    let mySelector =
        div [] [
            label [] [
                Doc.Radio [] Red varColor
                text " Select Red"
            ]
            label [] [
                Doc.Radio [] Green varColor
                text " Select Green"
            ]
            label [] [
                Doc.Radio [] Blue varColor
                text " Select Blue"
            ]
        ]
        
    // Result:
    // <div>
    //   <label><input type="radio" /> Select Red</label>
    //   <label><input type="radio" checked /> Select Green</label>
    //   <label><input type="radio" /> Select Blue</label>
    // </div>
    // Plus varColor is bound to contain the selected color.
    ```

More variants are available in the [`Doc` module](/api/WebSharper.UI.Client.Doc).

### Views

The full power of WebSharper.UI's reactive layer comes with [`View`s](/api/WebSharper.UI.View\`1). A `View<'T>` is a time-varying value computed from Vars and from other Views. At any point in time the view has a certain value of type `'T`.

One thing important to note is that the value of a View is not computed unless it is needed. For example, if you use [`View.Map`](#view-map), the function passed to it will only be called if the result is needed. It will only be run while the resulting View is included in the document using [one of these methods](#view-doc). This means that you generally don't have to worry about expensive computations being performed unnecessarily. However it also means that you should avoid relying performing side-effects in functions like `View.Map`.

In pseudo-code below, `[[x]]` notation is used to denote the value of the View `x` at every point in time, so that `[[x]]` = `[[y]]` means that the two views `x` and `y` are observationally equivalent.

#### Creating and combining Views

The first and main way to get a View is using the [`View`](/api/WebSharper.UI.Var\`1#View) property of `Var<'T>`. This retrieves a View that tracks the current value of the Var.

You can create Views using the following functions and combinators from the `View` module:

* [`View.Const`](/api/WebSharper.UI.View#Const\`\`1) creates a View whose value is always the same.

    ```fsharp
    let v = View.Const 42

    // [[v]] = 42
    ```

* [`View.ConstAnyc`](/api/WebSharper.UI.View#ConstAsync\`\`1) is similar to `Const`, but is initialized asynchronously. Until the async returns, the resulting View is uninitialized.

* <a name="view-map"></a>[`View.Map`](/api/WebSharper.UI.View#Map\`\`2) takes an existing View and maps its value through a function.

    ```fsharp
    let v1 : View<string> = // ...
    let v2 = View.Map (fun s -> String.length s) v1

    // [[v2]] = String.length [[v1]]
    ```

* [`View.Map2`](/api/WebSharper.UI.View#Map2\`\`3) takes two existing Views and map their value through a function.

    ```fsharp
    let v1 : View<int> = // ...
    let v2 : View<int> = // ...
    let v3 = View.Map2 (fun x y -> x + y) v1 v2

    // [[v3]] = [[v1]] + [[v2]]
    ```

    Similarly, [`View.Map3`](/api/WebSharper.UI.View#Map3\`\`4) takes three existing Views and map their value through a function.

* [`View.MapAsync`](/api/WebSharper.UI.View#MapAsync\`2) is similar to `View.Map` but maps through an asynchronous function.

    An important property here is that this combinator saves work by abandoning requests. That is, if the input view changes faster than we can asynchronously convert it, the output view will not propagate change until it obtains a valid latest value. In such a system, intermediate results are thus discarded.
    
    Similarly, [`View.MapAsync2`](/api/WebSharper.UI.View#MapAsync2\`3) maps two existing Views through an asynchronous function.

* [`View.Apply`](/api/WebSharper.UI.View#Apply\`2) takes a View of a function and a View of its argument type, and combines them to create a View of its return type.

    While Views of functions may seem like a rare occurrence, they are actually useful together with `View.Const` in a pattern that can lift a function of any number N of arguments into an equivalent of `View.MapN`.

    ```fsharp
    // This shorthand is defined in WebSharper.UI.Notation.
    let (<*>) vf vx = View.Apply vf vx
    
    // Inputs: a function of 4 arguments and 4 Views.
    let f a b c d = // ...
    let va = // ...
    let vb = // ...
    let vc = // ...
    let vd = // ...
    
    // Equivalent to a hypothetical `View.Map4 f va vb vc vd`.
    let combinedView =
        View.Const f <*> va <*> vb <*> vc <*> vd
    ```

<a name="view-doc"></a>
#### Inserting Views in the Doc

Once you have created a View to represent your dynamic content, here are the various ways to include it in a Doc:

* [`textView`](/api/WebSharper.UI.Html#textView) is a reactive counterpart to `text`, which creates a text node a `View<string>`.

    ```fsharp
    let varTxt = Var.Create ""
    let vLength =
        varTxt.View
        |> View.Map String.length
        |> View.Map (fun l -> sprintf "You entered %i characters." l)
    div [] [
        Doc.Input [] varName
        textView vLength
    ]
    ```

* [`Doc.BindView`](/api/WebSharper.UI.Doc#BindView\`\`1) maps a View into a dynamic Doc.

    ```fsharp
    let varTxt = Var.Create ""
    let vWords =
        varTxt.View
        |> View.Map (fun s -> s.Split(' '))
        |> Doc.BindView (fun words ->
            words
            |> Array.map (fun w -> li [] [text w] :> Doc)
            |> Doc.Concat
        )
    div [] [
        text "You entered the following words:"
        ul [] [ vWords ]
    ]
    ```

* `attr.*Dyn` is a reactive equivalent to the corresponding `attr.*`, creating an attribute from a `View<string>`.

    For example, the following sets the background of the input element based on the user input value:

    ```fsharp
    let varTxt = Var.Create ""
    let vStyle =
        varTxt.View
        |> View.Map (fun s -> "background-color: " + s)
    Doc.Input [ attr.styleDyn vStyle ] varTxt
    ```

* `attr.*DynPred` is similar to `attr.*Dyn`, but it takes an extra `View<bool>`. When this View is true, the attribute is set (and dynamically updated as with `attr.*Dyn`), and when it is false, the attribute is removed.

    ```fsharp
    let varTxt = Var.Create ""
    let varCheck = Var.Create true
    let vStyle =
        varTxt.View
        |> View.Map (fun s -> "background-color: " + s)
    div [] [
        Doc.Input [ attr.styleDynPred vStyle varCheck.View ] varTxt
        Doc.CheckBox [] varCheck
    ]
    ```

## Routing

If you have a `WebSharper.Sitelets.Router<T>` value, it can be shared between server and client. A router encapsulates two things: parsing an URL path to an abstract value and writing a value as an URL fragment. So this allows generating links safely on both client  When initializing a page client-side, you can decide to install a custom click handler for your page which recognizes some or all local links to handle without browser navigation.

### Install client-side routing

There are 3 scenarios which WebSharper routing makes possible:
* For creating single-page applications, when browser refresh is never wanted, `Router.Install` creates a global click handler that prevents default behavior of `<a>` links on your page pointing to a local URL.
* If you want client-side navigation only between some part of the whole site map covered by the router, `Router.InstallPartial` creates a global click handler that now only override behavior of local links which can be mapped to the subset actions that are handled in the client. For example you can make navigating between `yoursite.com/profile/...` links happen with client-side routing, but any links that would point out of `/profile/...` are still doing browser navigation automatically.
* If you want to have client-side routing on a sub-page that the server knows nothing about, `Router.InstallHash` subscribes to `window.location.hash` changes.

In all cases, the `Install` function used returns a `Var`, which you can use to map the visible content of your page from. It has a two way binding to the URL: link or forward/back navigation changes the value of the `Var`, and setting the value does a client-side navigation which also updates the URL automatically.

Example for `Router.Install`, using the router value introduced in the [Sitelets documentation](sitelets.md):
```fsharp
let ClientMain() =
    let location = rPages |> Router.Install Home
    location.View.Doc(function
        | Home -> div [ text "This is the home page" ]
        | Contact p -> div [ text (sprintf "Contact name:%s, age:%d" p.Name p.Age) ]
    )
```
First argument (`Home`) specifies which page value to set if URL path cannot be parsed, which could be a home or an error page. 

`Router.InstallHash` have the same signature as `Router.Install`, the only difference is that URLs would look like `yoursite.com/#contact/Bob/32`.

Example for `Router.InstallPartial`:
```fsharp
let ContactMain() =    
    let location =
        rPages |> Router.InstallPartial ("", 0)
            (function Contact p -> Some p | _ -> None)
            Contact
    location.View.Doc(fun p -> 
        div [ text (sprintf "Contact name:%s, age:%d" p.Name p.Age) ]
    )
```
Here we only install a click handler for the contact pages, which means that a link to root will be a browser navigation, but links between contacts work fully on the client. The first function argument maps a full page value to an option of a value that we handle, and the second function maps this back to a full page value. So instead of a `Var<Pages>` here we get only a `Var<Person>`.

In a real world application, usually you would have some `View.MapAsync` from the `location` variable, to pull some data related to the subpage from the server by an RPC call, and exposing that as content:

```fsharp
[<Remote>] // this is a server-side function exposed as a WebSharper RPC
let GetContactDetails p = async { ... }

let ContactMain() =    
    let location = // ...
    let contactDetails = location.View |> View.MapAsync GetContactDetails
    contactDetails.View.Doc(fun p -> 
        // show contact detils
    )
```

You can navigate programmatically with `location.Value <- newLoc`, `location |> Var.Set newLoc` or `location := newLoc` (if you have `open WebSharper.UI.Next.Notation`). 

