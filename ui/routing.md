---
order: -70
label: Routing
---

# Routing

If you have a `WebSharper.Sitelets.Router<'T>` value, it can be shared between server and client. A router encapsulates two things: parsing an URL path to an abstract value and writing a value as an URL fragment. So this allows generating links safely on both client  When initializing a page client-side, you can decide to install a custom click handler for your page which recognizes some or all local links to handle without browser navigation.

## Install client-side routing

There are 3 scenarios for client-side routing which WebSharper routing makes possible:
* For creating single-page applications, when browser refresh is never wanted, `Router.Install` creates a global click handler that prevents default behavior of `<a>` links on your page pointing to a local URL.
* If you want client-side navigation only between some part of the whole site map covered by the router, you can use `Router.Slice` before `Router.Install`. This creates a global click handler that now only override behavior of local links which can be mapped to the subset of endpoints that are handled in the client. For example you can make navigating between `yoursite.com/profile/...` links happen with client-side routing, but any links that would point out of `/profile/...` are still doing browser navigation automatically.
* If you want to have client-side routing on a sub-page that the server knows nothing about, `Router.InstallHash` subscribes to `window.location.hash` changes only. You can use a router that is specific to that single sub-page.

In all cases, the `Install` function used returns a `Var`, which you can use to map the visible content of your page from. It has a two way binding to the URL: link or forward/back navigation changes the value of the `Var`, and setting the value does a client-side navigation which also updates the URL automatically.

Example for `Router.Install`, using the router value introduced in the [Sitelets documentation](sitelets.md):
```fsharp
let ClientMain() =
    let location = rPages |> Router.Install Home
    location.View.Doc(function
        | Home -> div [] [ text "This is the home page" ]
        | Contact p -> div [] [ text (sprintf "Contact name:%s, age:%d" p.Name p.Age) ]
    )
```
First argument (`Home`) specifies which page value to fall back on if the URL path cannot be parsed (although this won't happen if you set up your server-side correctly), which could be a home or an error page.

Also, you need to make sure that your router value is `[<JavaScript>]` annotated (or a containing type, module or the assembly is), so that it is available for cross-tier use.

`Router.InstallHash` have the same signature as `Router.Install`, the only difference is that URLs would look like `yoursite.com/#/contact/Bob/32`.

Example for `Router.Slice` and `Router.Install`:
```fsharp
let ContactMain() =    
    let location =
        rPages |> Router.Slice
            (function Contact p -> Some p | _ -> None)
            Contact
        |> Router.Install ("unknown", 0)
    location.View.Doc(fun p -> 
        div [] [ text (sprintf "Contact name:%s, age:%d" p.Name p.Age) ]
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
    contactDetails.Doc(fun p -> 
        // show contact details
    )
```

You can navigate programmatically with `location.Value <- newLoc`, `location |> Var.Set newLoc` or `location := newLoc` (if you have `open WebSharper.UI.Next.Notation`). 
