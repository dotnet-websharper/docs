---
label: HTML
order: -10
expanded: true
---
# HTML

Whether for your SPA or your client-server application, you will at some point undoubtedly be needing to work with HTML. To do so, you need to add `WebSharper.UI` to your project:

```text
dotnet add package WebSharper.UI
```

WebSharper.UI's core type for working with HTML is `Doc`. A Doc can represent a single DOM node (an element or plain text), or sequence of zero or more nodes.

Additionally, client-side Docs can be reactive. A same Doc can consist of different elements at different moments in time, depending on user input or other variables. See [the reactive section](#reactive) to learn more about this.

There are three main types of uses for your HTML:

1. HTML sent from the server to fulfill an incoming request.

2. 

---

## Docs

### HTML functions (aka. combinators)

The main means of creating Docs is by using the functions in the `WebSharper.UI.Html` module. Every HTML element has a dedicated function, such as `div` or `p`, which takes a sequence of [attributes](#attrs) (of type `Attr`) and a sequence of child nodes (of type `Doc`). Additionally, the `text` function creates a text node.

!!! `Elt` vs `Doc`
You may sometimes need stronger guarantees about `Doc` values, for instance, when there is a one-to-one correspondence to actual DOM nodes. To represent these "actual" HTML fragements, you can use `Elt` values.

 which is a subtype of `Doc` that is guaranteed to always consist of exactly one element and provides additional APIs such as `Dom` to get the underlying `Dom.Element`. This subtyping means that you will sometimes need to upcast the result of such a function with `:> Doc` to appease the compiler.
!!!

{% tabs %}
{% tab title="F\#" %}

```fsharp
open WebSharper.UI.Html

let myDoc =
    div [] [
        h1 [] [text "Functional Reactive Programming and HTML"]
        p [] [text "WebSharper.UI is a library providing a novel, pragmatic and convenient approach to UI reactivity. It includes:"]
        ul [] [
            li [] [text "..."]
        ]
    ]
```

{% endtab %}
{% tab title="Result" %}

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

{% endtab %}
{% endtabs %}

Some HTML tags, such as `option`, collide with standard library names and are therefore only located in the `Tags` submodule.

{% tabs %}
{% tab title="F\#" %}

```fsharp
let myDropdown =
    select [] [
        Tags.option [] [text "First choice"]
        Tags.option [] [text "Second choice"]
        Tags.option [] [text "Third choice"]
    ]
```

{% endtab %}

{% tab title="Result" %}

```html
<select>
  <option>First choice</option>
  <option>Second choice</option>
  <option>Third choice</option>
</select>
```

{% endtab %}
{% endtabs %}

The following sections list some notable functions in `Doc` that you can use to create or combine Docs.

### `Doc.Empty`

> Creates a Doc consisting of zero nodes. This can be useful for example when you may not need to insert an element depending on a condition.

{% tabs %}
{% tab title="F\#" %}

```fsharp
let myForm (withDropdown: bool) =
    form [] [
        input [attr.name "name"] []
        if withDropdown then myDropdown else Doc.Empty
    ]
```

{% endtab %}

{% tab title="Result" %}

```html
<form>
  <input name="name" />
</form>
    
```

or:

```html
<form>
  <input name="name" />
  <!-- ... contents of myDropdown here ... -->
</form>
```

{% endtab %}
{% endtabs %}

---

### `Doc.Append`

> Creates a Doc consisting of the concatenation of two Docs.

{% tabs %}
{% tab title="F\#" %}

```fsharp
let titleAndBody =
    Doc.Append
        (h1 [] [text "Functional Reactive Programming and HTML"])
        (p [] [text "WebSharper.UI is a library providing ..."])
```

{% endtab %}

{% tab title="Result" %}

```html
<h1>Functional Reactive Programming and HTML</h1>
<p>WebSharper.UI is a library providing ...</p>
```

{% endtab %}
{% endtabs %}

!!!
For the mathematically inclined, the functions `Doc.Empty` and `Doc.Append` make Docs a monoid.
!!!

---

### `Doc.Concat`

> Generalizes `Append` by concatenating a sequence of Docs.

{% tabs %}
{% tab title="F\#" %}

```fsharp
let thisPage =
    Doc.Concat [
        h1 [] [text "Functional Reactive Programming and HTML"]
        p [] [text "WebSharper.UI is a library providing ..."]
        ul [] [
            li [] [text "..."]
        ]
    ]
```

{% endtab %}

{% tab title="Result" %}

```html
<h1>Functional Reactive Programming and HTML</h1>
<p>WebSharper.UI is a library providing ...</p>
<ul>
  <li>...</li>
</ul>
```

{% endtab %}
{% endtabs %}

---

### `Doc.Element`

> Creates an element with the given name, attributes and children. It is equivalent to the function with the same name from the `Html` module. This function is useful if the tag name is only known at runtime, or if you want to create a non-standard element that isn't available in `Html`. The following example creates a header tag of a given level (`h1`, `h2`, etc).

{% tabs %}
{% tab title="F\#" %}

```fsharp
let makeHeader (level: int) (content: string) =
    Doc.Element ("h" + string level) [] [ text content ]
```

{% endtab %}

{% tab title="Result" %}

```html
<h1>content...</h1>
```

or

```html
<h2>content...</h2>
```

or etc.

{% endtab %}
{% endtabs %}

---

### `Doc.Verbatim`

> Creates a Doc from plain HTML text.  

!!!danger Security warning!
This function does not perform any checks on the contents, and can be a code injection vulnerability if used improperly. We recommend avoiding it unless absolutely necessary, and properly sanitizing user inputs if you do use it. If you simply want to use HTML syntax instead of F# functions, take a look at [templating](templates.md).
!!!

{% tabs %}
{% tab title="F\#" %}

```fsharp
    let plainDoc = """
<h1 onclick="alert('And it is unsafe!')">
    This is plain HTML!
</h1>"""
```

{% endtab %}

{% tab title="Result" %}

```html
<h1 onclick="alert('And it is unsafe!')">
    This is plain HTML!
</h1>
```

{% endtab %}
{% endtabs %}

---

## Attrs

To create attributes, use corresponding functions from the `attr` submodule.

{% tabs %}
{% tab title="F\#" %}

```fsharp
let myFormControl =
    select [attr.name "mySelect"] [
        Tags.option [attr.value "first"] [text "First choice"]
        Tags.option [attr.value "second"] [text "Second choice"]
        Tags.option [
            attr.value "third"
            attr.selected "selected"
        ] [text "Third choice"]
    ]
```

{% endtab %}

{% tab title="Result" %}

```html
<select name="mySelect">
  <option value="first">First choice</option>
  <option value="second">Second choice</option>
  <option value="third" selected="selected">Third choice</option>
</select>
```

{% endtab %}
{% endtabs %}

Some attributes, notably `class` and `type`, are also F# keywords, so they need to be wrapped in double backquotes.

{% tabs %}
{% tab title="F\#" %}

```fsharp
let myMain =
    div [attr.``class`` "main"] [text "..."]
```

{% endtab %}

{% tab title="Result" %}

```html
<div class="main">...</div>
```

{% endtab %}
{% endtabs %}

HTML5 also defines any attribute whose names starts with `data-` as a valid custom attribute. You can create such an attribute using the function `data-` from module `attr` (backquoted since it contains a non-standard character).

{% tabs %}
{% tab title="F\#" %}

```fsharp
let myEltWithData =
    div [attr.``data-`` "uid" "myDiv"] [text "..."]
```

{% endtab %}

{% tab title="Result" %}

```html
<div data-uid="myDiv">...</div>
```

{% endtab %}
{% endtabs %}

Like `Doc`, a value of type `Attr` can represent zero, one or more attributes. The functions in the `Attr` module can create such non-singleton attributes.

---

### `Attr.Empty`

> Creates an empty attribute. This can be useful for example when you may not need to insert an attribute depending on a condition.

{% tabs %}
{% tab title="F\#" %}

```fsharp
let makeInput (initialValue: option<string>) =
    let valueAttr =
        match initialValue with
        | Some v -> attr.value v
        | None -> Attr.Empty
    input [valueAttr] []
```

{% endtab %}

{% tab title="Result" %}

```html
<input value="initialValue..." />
```

or

```html
<input />
```

{% endtab %}
{% endtabs %}

---

### `Attr.Append`

> Combines two attributes.

{% tabs %}
{% tab title="F\#" %}

```fsharp
let passwordAttr =
    Attr.Append (attr.``type`` "password") (attr.placeholder "Password")
```

{% endtab %}

{% tab title="Result" %}

```html
type="password" placeholder="Password"
```

{% endtab %}
{% endtabs %}

---

### `Attr.Concat`

> Combines a sequence of attributes.

{% tabs %}
{% tab title="F\#" %}

```fsharp
let passwordAttr =
    Attr.Concat [
        attr.``type`` "password"
        attr.placeholder "Password"
        attr.``class`` "pw-input"
    ]
```

{% endtab %}

{% tab title="Result" %}

```html
type="password" placeholder="Password" class="pw-input"
```

{% endtab %}
{% endtabs %}

---

### `Attr.Create`

> Creates a single attribute. It is equivalent to the function with the same name from the `attr` module. This function is useful if the attribute name is only known at runtime, or if you want to create a non-standard attribute that isn't available in `attr`.

{% tabs %}
{% tab title="F\#" %}

```fsharp
let eltWithNonStandardAttr =
    div [Attr.Create "my-attr" "my-value"] [text "..."]
```

{% endtab %}

{% tab title="Result" %}

```html
<div my-attr="my-value">...</div>
```

{% endtab %}
{% endtabs %}

---

## Event handlers

Event handlers can be added using the functions from the `on` submodule.

```fsharp
let myButton =
    button [on.click (fun _ _ -> JS.Alert "Hi!")] [text "Click me!"]
```

The handler function takes two arguments:

* The element itself on which the event was raised, as a native `Dom.Element`, and
* The event arguments, as a subtype of the native `Dom.Event`.

```fsharp
open WebSharper.JavaScript

let myButton =
    button [
        attr.id "my-button"
        on.click (fun e args ->
            JS.Alert $"You clicked %s{e.Id} at x = %i{args.ClientX}, y = %i{args.ClientY}."
        )
    ] [text "Click me!"]
```

In addition to the standard HTML events, `on.afterRender` is a special handler that is called by WebSharper after inserting the element into the DOM.
