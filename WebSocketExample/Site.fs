namespace WebSocketExample

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/about">] About

module Templating =
    open WebSharper.UI.Html

    // Compute a menubar where the menu item for the given endpoint is active
    let MenuBar (ctx: Context<EndPoint>) endpoint : Doc list =
        let ( => ) txt act =
            let isActive = if endpoint = act then "nav-link active" else "nav-link"
            li [attr.``class`` "nav-item"] [
                a [
                    attr.``class`` isActive
                    attr.href (ctx.Link act)
                ] [text txt]
            ]
        [
            "Home" => EndPoint.Home
            "About" => EndPoint.About
        ]

    let Main ctx action (title: string) wsEndpoint (body: Doc list) =
        Content.Page(
            Templates.MainTemplate()
                .Title(title)
                .MenuBar(MenuBar ctx action)
                .Body(body)
                .Doc()
        )

module Site =
    open WebSharper.UI.Html

    open type WebSharper.UI.ClientServer
    open WebSharper.AspNetCore.WebSocket

    let HomePage ctx wsEndpoint =
        
        Templating.Main ctx EndPoint.Home "Home" wsEndpoint [
            h1 [] [text "Say Hi to the server!"]
            div [] [client (Client.Main wsEndpoint)]
        ]

    let AboutPage ctx wsEndpoint =
        Templating.Main ctx EndPoint.About "About" wsEndpoint [
            h1 [] [text "About"]
            p [] [text "This is a template WebSharper client-server application."]
        ]

    [<Website>]
    let Main =
        Application.MultiPage (fun (ctx:Context<_>) endpoint ->
            let wsEndpoint = WebSocketEndpoint.Create(ctx.Request.Uri.ToString(), "ws")
            match endpoint with
            | EndPoint.Home -> 
                HomePage ctx wsEndpoint
            | EndPoint.About -> AboutPage ctx wsEnd
        )

