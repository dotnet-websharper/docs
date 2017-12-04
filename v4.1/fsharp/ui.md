# Functional Reactive Programming and HTML

WebSharper.UI is a library providing a novel, pragmatic and convenient approach to UI reactivity. It includes:

* An [HTML library](#html) usable both from the server side and from the client side, which you can use to build HTML pages either by calling F# functions to create elements, or by instantiating template HTML files.
* A [dataflow layer](#dataflow) for expressing user inputs and values computed from them as time-varying values. This approach is related to Functional Reactive Programming (FRP). This dataflow integrates with the HTML library to create reactive documents. If you are familiar with Facebook React, then you will find some similarities with this approach: instead of explicitly inserting, modifying and removing DOM nodes, you return a value that represents a DOM tree based on inputs. The main difference is that these inputs are nodes of the dataflow layer, rather than a single state value associated with the component.
* A [declarative animation system](#animation) for the client-side HTML layer.

This page is an overview of the capabilities of WebSharper.UI. You can also check [the full reference of all the API types and modules](http://developers.websharper.com/api/WebSharper.UI).

## Using HTML

WebSharper.UI's core type for HTML construction is [`Doc`](/api/WebSharper.UI.Doc). A Doc can represent a single DOM node (element, text), but it can also be a sequence of zero or more nodes. This allows you to treat equally any HTML snippet that you want to insert into a document, whether it consists of a single element or not.

Additionally, client-side Docs can be reactive. A same Doc can consist of different elements at different moments in time, depending on user input or other variables. See [the dataflow section](#dataflow) to learn more about this.

### Constructing HTML

#### Docs

The main means of creating Docs is by using the functions in the [`WebSharper.UI.Html`](/api/WebSharper.UI.Html) module. Every HTML element has a dedicated function, such as [`div`](/api/WebSharper.UI.Html#div) or [`p`](/api/WebSharper.UI.Html#p), which takes a sequence of attributes (of type [`Attr`](/api/WebSharper.UI.Attr)) and a sequence of child nodes (of type `Doc`). Additionally, the [`text`](/api/WebSharper.UI.Html#text) function creates a text node.

```fsharp
open WebSharper.UI.Html

let myDoc =
    div [] [
        h1 [] [ text "Functional Reactive Programming and HTML" ]
        p [] [ text "WebSharper.UI is a library providing a novel, pragmatic and convenient approach to UI reactivity. It includes:" ]
        ul [] [
            li [] [ text "[...]" ]
        ]
    ]
```

Some HTML tags, such as `option`, collide with standard library names and are therefore only located in the [`Tags`](/api/WebSharper.UI.Html.Tags) submodule.

```fsharp
let myDropdown =
    select [] [
        Tags.option [] [ text "First choice" ]
        Tags.option [] [ text "Second choice" ]
        Tags.option [] [ text "Third choice" ]
    ]
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
    ```

* [`Doc.Append`](/api/WebSharper.UI.Doc#Append) creates a Doc consisting of the concatenation of two Docs.

    ```fsharp
    let titleAndBody =
        Doc.Append
            (h1 [] [ text "Functional Reactive Programming and HTML" ])
            (p [] [ text "WebSharper.UI is a library providing [...]" ])
    ```

For the mathematically enclined, the functions `Doc.Empty` and `Doc.Append` make Docs a monoid.

* [`Doc.Concat`](/api/WebSharper.UI.Doc#Concat) generalizes `Append` by concatenating a sequence of Docs.

    ```fsharp
    let thisPage =
        Doc.Concat [
            h1 [] [ text "Functional Reactive Programming and HTML" ]
            p [] [ text "WebSharper.UI is a library providing [...]" ]
            ul [] [
                li [] [ text "[...]" ]
            ]
        ]
    ```

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
```

### Rendering HTML on the client

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

### Rendering HTML on the server

## HTML Templates

## Dataflow

The dataflow layer 

### Connecting the Dataflow and the HTML

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

