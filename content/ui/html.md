---
title: Functional Reactive Programming and HTML
---

WebSharper.UI is a library providing a novel, practical, and user-friendly approach to UI reactivity. It includes:

* An [HTML library](#constructing-html) usable on both server and client sides, allowing you to build HTML pages either by calling F# functions to create elements or by using template HTML files.
* A [reactive layer](reactive) for expressing user inputs and values computed from them as time-varying values, related to Functional Reactive Programming (FRP). 
  This reactive system integrates with the HTML library to create reactive documents.
  If familiar with React, you'll notice similarities: instead of explicitly inserting, modifying, or removing DOM nodes, you return a value representing a DOM tree based on inputs.
  The main difference is that these inputs are nodes of the reactive system, not a single state value associated with the component.
* Client-side [routing](routing) using the same endpoint type declaration as [WebSharper server-side routing](../core/routing).

This page is an overview of the capabilities of WebSharper.UI.

Get the package from NuGet: [WebSharper.UI](https://www.nuget.org/packages/websharper.ui).

## Using HTML

WebSharper.UI's core type for HTML construction is `Doc`. A Doc can represent a single DOM node (element, text), but it can also be a sequence of zero or more nodes. This allows you to treat equally any HTML snippet that you want to insert into a document, whether it consists of a single element or not.

Additionally, client-side Docs can be reactive. A same Doc can consist of different elements at different moments in time, depending on user input or other variables. See [the reactive page](reactive) to learn more about this.

<a name="html"></a>
### Constructing HTML

The main means of creating Docs is by using the functions in the `WebSharper.UI.Html` module.
Every HTML element has a dedicated function, such as `div` or `p`, which takes a sequence of [attributes](#attrs) and child nodes, of type `Attr` and `Doc`  respectively.
Additionally, the `text` function creates a text node.

```fsharp
open WebSharper.UI.Html

let myDoc =
    div [] [
        h1 [] [ text "Functional Reactive Programming and HTML" ]
        p [] [ text "WebSharper.UI is a library providing a novel, practical, and user-friendly approach to UI reactivity. It includes:" ]
        ul [] [
            li [] [ text "..." ]
        ]
    ]
```

Result:

```html
<div>
  <h1>Functional Reactive Programming and HTML</h1>
  <p>WebSharper.UI is a library providing a novel, pragmatic and convenient
     approach to UI reactivity. It includes:</p>
  <ul>
    <li>...</li>
  </ul>
</div>
```

Some HTML tags, such as `option`, collide with standard library names and are therefore only located in the `Tags` submodule.

```fsharp
let myDropdown =
    select [] [
        Tags.option [] [ text "First choice" ]
        Tags.option [] [ text "Second choice" ]
        Tags.option [] [ text "Third choice" ]
    ]
```
Result:

```html
<select>
  <option>First choice</option>
  <option>Second choice</option>
  <option>Third choice</option>
</select>
```

One thing to note is that the tag functions described above actually return a value of type `Elt`, which is a subtype of `Doc` that is guaranteed to always consist of exactly one element and provides additional APIs such as `Dom` to get the underlying `Dom.Element`. This subtyping means that you will sometimes need to upcast the result of such a function with `:> Doc` to appease the compiler; you can see an example of this below in the example for `Doc.Empty`.

Additional functions in the `Doc` can create or combine Docs:

* `Doc.Empty` creates a Doc consisting of zero nodes. This can be useful for example when you may not need to insert an element depending on a condition.

    ```fsharp
    let myForm (withDropdown: bool) =
        form [] [
            input [ attr.name "name" ] []
            (if withDropdown then myDropdown :> Doc else Doc.Empty)
        ]
    ```
    
    Result:

    ```html
    <form>
      <input name="name" />
    </form>
    
    or:
    
    <form>
      <input name="name" />
      <!-- ... contents of myDropdown here ... -->
    </form>
    ```

* `Doc.Append` creates a Doc consisting of the concatenation of two Docs.

    ```fsharp
    let titleAndBody =
        Doc.Append
            (h1 [] [ text "Functional Reactive Programming and HTML" ])
            (p [] [ text "WebSharper.UI is a library providing ..." ])
    ```
    
    Result:

    ```html
    <h1>Functional Reactive Programming and HTML</h1>
    <p>WebSharper.UI is a library providing ...</p>
    ```

For the mathematically enclined, the functions `Doc.Empty` and `Doc.Append` make Docs a monoid.

* `Doc.Concat` generalizes `Append` by concatenating a sequence of Docs.

    ```fsharp
    let thisPage =
        Doc.Concat [
            h1 [] [ text "Functional Reactive Programming and HTML" ]
            p [] [ text "WebSharper.UI is a library providing ..." ]
            ul [] [
                li [] [ text "..." ]
            ]
        ]
    ```
    
    Result:
        
    ```html
    <h1>Functional Reactive Programming and HTML</h1>
    <p>WebSharper.UI is a library providing ...</p>
    <ul>
      <li>...</li>
    </ul>
    ```

* `Doc.Element` creates an element with the given name, attributes and children. It is equivalent to the function with the same name from the `Html` module. This function is useful if the tag name is only known at runtime, or if you want to create a non-standard element that isn't available in `Html`. The following example creates a header tag of a given level (`h1`, `h2`, etc).

    ```fsharp
    let makeHeader (level: int) (content: string) =
        Doc.Element ("h" + string level) [] [ text content ]
    ```
    
    Result:
        
    ```html
    <h1>content...</h1>
    or
    <h2>content...</h2>
    or etc.
    ```

* `Doc.Verbatim` creates a Doc from plain HTML text.  
    **Security warning:** this function does not perform any checks on the contents, and can be a code injection vulnerability if used improperly. We recommend avoiding it unless absolutely necessary, and properly sanitizing user inputs if you do use it. If you simply want to use HTML syntax instead of F# functions, take a look at [templating](templating).

    ```fsharp
    let plainDoc =
        Doc.Verbatim "
            <h1 onclick=\"alert('And it is unsafe!')\">
                This is plain HTML!
            </h1>"
    ```
    
    Result:

    ```html
    <h1 onclick="alert('And it is unsafe!')">
        This is plain HTML!
    </h1>
    ```

<a name="attr"></a>
#### Attrs

To create attributes, use corresponding functions from the `attr` submodule.

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
```

Result:

```html
<select name="mySelect">
  <option value="first">First choice</option>
  <option value="second">Second choice</option>
  <option value="third" selected="selected">Third choice</option>
</select>
```

Some attributes, notably `class` and `type`, are also F# keywords, so they need to be wrapped in double backquotes.

```fsharp
let myMain =
    div [ attr.``class`` "main" ] [ text "..." ]
```

Result:

```html
<div class="main">...</div>
```

HTML5 also defines any attribute whose names starts with `data-` as a valid custom attribute. You can create such an attribute using the function `data-` from module `attr` (backquoted since it contains a non-standard character).

```fsharp
let myEltWithData =
    div [ attr.``data-`` "uid" "myDiv" ] [ text "..." ]
```

Result:

```html
<div data-uid="myDiv">...</div>
```

Like `Doc`, a value of type `Attr` can represent zero, one or more attributes. The functions in the `Attr` module can create such non-singleton attributes.

* `Attr.Empty` creates an empty attribute. This can be useful for example when you may not need to insert an attribute depending on a condition.

    ```fsharp
    let makeInput (initialValue: option<string>) =
        let valueAttr =
            match initialValue with
            | Some v -> attr.value v
            | None -> Attr.Empty
        input [ valueAttr ] []
    ```
    
    Result:
        
    ```html
    <input value="initialValue..." />
    or
    <input />
    ```

* `Attr.Append` combines two attributes.

    ```fsharp
    let passwordAttr =
        Attr.Append (attr.``type`` "password") (attr.placeholder "Password")
    ```
    
    Result:

    ```html
    type="password" placeholder="Password"
    ```

* `Attr.Concat` combines a sequence of attributes.

    ```fsharp
    let passwordAttr =
        Attr.Concat [
            attr.``type`` "password"
            attr.placeholder "Password"
            attr.``class`` "pw-input"
        ]
    ```
    
    Result:

    ```html
    type="password" placeholder="Password" class="pw-input"
    ```

* `Attr.Create` creates a single attribute. It is equivalent to the function with the same name from the `attr` module. This function is useful if the attribute name is only known at runtime, or if you want to create a non-standard attribute that isn't available in `attr`.

    ```fsharp
    let eltWithNonStandardAttr =
        div [ Attr.Create "my-attr" "my-value" ] [ text "..." ]
    ```
    
    Result:
        
    ```html
    <div my-attr="my-value">...</div>
    ```

#### Event handlers

A special kind of attribute is event handlers. They can be created using functions from the `on` submodule.

```fsharp
let myButton =
    button [ on.click (fun el ev -> JS.Alert "Hi!") ] [ text "Click me!" ]
```

The handler function takes two arguments:

* The element itself, as a native `Dom.Element`;
* The triggered event, as a native `Dom.Event`.

```fsharp
let myButton =
    button [
        attr.id "my-button"
        on.click (fun el ev ->
            JS.Alert (sprintf "You clicked %s at x = %i, y = %i." 
                        el.Id ev.ClientX ev.ClientY)
        )
    ] [ text "Click me!" ]
```

In addition to the standard HTML events, `on.afterRender` is a special handler that is called by WebSharper after inserting the element into the DOM.

### HTML on the client

To insert a Doc into the document on the client side, use the `Doc.Run*` family of functions from the module `WebSharper.UI.Client`. Each of these functions has two variants: one directly taking a DOM `Element` or `Node`, and the other suffixed with `ById` taking the id of an element as a string.

* `Doc.Run` and `Doc.RunById` insert a given Doc as the child(ren) of a given DOM element. Note that it replaces the existing children, if any.

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

* `Doc.RunAppend` and `Doc.RunAppendById` insert a given Doc as the last child(ren) of a given DOM element.

* `Doc.RunPrepend` and `Doc.RunPrependById` insert a given Doc as the first child(ren) of a given DOM element.

* `Doc.RunAfter` and `Doc.RunAfterById` insert a given Doc as the next sibling(s) of a given DOM node.

* `Doc.RunBefore` and `Doc.RunBeforeById` insert a given Doc as the previous sibling(s) of a given DOM node.

* `Doc.RunReplace` and `Doc.RunReplaceById` insert a given Doc replacing a given DOM node.

### HTML on the server

On the server side, using [sitelets](../core/sitelets), you can create HTML pages from Docs by passing them to the `Body` or `Head` arguments of `Content.Page`.

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

By opening `WebSharper.UI.Server`, you can also just pass a full page to `Content.Page`. This is particularly useful together with [templates](templating).

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

To include client-side elements inside a page, use the `client` method, from inside `WebSharper.UI.Html`.

```fsharp
[<JavaScript>]
module Client =

    let MyControl() =
        button [ on.click (fun el ev -> JS.Alert "Hi!") ] [ text "Click me!" ]

module Server =

    let MyPage (ctx: Context<EndPoint>) =
        Content.Page(
            Title = "Welcome!",
            Body = [
                h1 [] [ text "Welcome!" ]
                p [] [ client <@ Client.MyControl() @> ]
            ]
        )
```

