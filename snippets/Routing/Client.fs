namespace Routing

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.Sitelets

[<JavaScript>]
module Client =
    // Define the possible routes
    type EndPoint =
        | [<EndPoint "">] Home
        | [<EndPoint "about">] About
        | [<EndPoint "contact">] Contact
        | [<EndPoint "notfound">] NotFound

    // Main UI component
    let Render (router: Router<EndPoint>) (currentRoute: Var<EndPoint>) =
        // Navigation bar
        let navBar =
            div [attr.``class`` "navbar"] [
                a [attr.href (router.HashLink Home)] [text "Home"]
                a [attr.href (router.HashLink About)] [text "About"]
                a [attr.href (router.HashLink Contact)] [text "Contact"]
            ]

        // Page content based on current route
        let pageContent =
            currentRoute.View.Doc (fun route ->
                match route with
                | Home ->
                    div [] [
                        h2 [] [text "Welcome to the Home Page"]
                        p [] [text "This is the main page of our SPA."]
                    ]
                | About ->
                    div [] [
                        h2 [] [text "About Us"]
                        p [] [text "Learn more about our application."]
                        button [
                            on.click (fun _ _ -> currentRoute.Set Contact)
                        ] [text "Go to Contact"]
                    ]
                | Contact ->
                    div [] [
                        h2 [] [text "Contact Us"]
                        p [] [text "Get in touch with us!"]
                    ]
                | NotFound ->
                    div [] [
                        h2 [] [text "404 - Page Not Found"]
                        p [] [text "The page you requested does not exist."]
                    ]
            )

        // Combine navigation and content
        div [] [
            navBar
            pageContent
        ]

    [<SPAEntryPoint>]
    let Main () =
        let router = Router.Infer<EndPoint>()

        // Install the router, with a fallback if no route matches
        let currentRoute =
            router 
            |> Router.InstallHash NotFound

        Render router currentRoute
        |> Doc.RunById "main"
