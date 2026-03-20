namespace Paging

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.Mvu

[<JavaScript>]
module Client =

    type EndPoint =
        | Home
        | EditEntry of string

    type Model =
        {
            EndPoint : EndPoint
            Entries : Map<string, string>
            Input : string
        }

    type Message =
        | Goto of EndPoint
        | SetEntry of string * string
        | RemoveEntry of string

    let Update (message: Message) (model: Model) =
        match message with
        | SetEntry (k, v) ->
            { model with Entries = Map.add k v model.Entries }
        | RemoveEntry k ->
            { model with
                Entries = Map.remove k model.Entries
                EndPoint =
                    match model.EndPoint with
                    | EditEntry k' when k = k' -> Home
                    | ep -> ep
            }
        | Goto ep ->
            { model with EndPoint = ep }

    module Pages =

        let Home = Page.Single(attrs = [Attr.Class "home-page"], usesTransition = true, render = fun dispatch model ->
            let inp = Elt.input [Attr.Class "input"] []
            Doc.Concat [
                div [Attr.Class "section"] [
                    div [Attr.Class "field has-addons"] [
                        div [Attr.Class "control"] [inp]
                        div [Attr.Class "control"] [
                            button [
                                Attr.Class "button"
                                on.click (fun _ _ -> dispatch (Goto (EndPoint.EditEntry inp.Value)))
                            ] [text "Add/edit entry"]
                        ]
                    ]
                ]
                table [Attr.Class "table is-fullwidth"] [
                    V(model.V.Entries).DocSeqCached(fun key value ->
                        tr [] [
                            th [] [text key]
                            td [] [text value.V]
                            td [] [
                                div [Attr.Class "buttons has-addons"] [
                                    button [
                                        Attr.Class "button is-small"
                                        on.click (fun _ _ -> dispatch (Goto (EndPoint.EditEntry key)))
                                    ] [text "Edit"]
                                    button [
                                        Attr.Class "button is-small"
                                        on.click (fun _ _ -> dispatch (RemoveEntry key))
                                    ] [text "Remove"]
                                ]
                            ]
                        ])
                ]
            ])

        let EditEntry = Page.Create(attrs = [Attr.Class "entry-page"], usesTransition = true, render = fun key dispatch model ->
            let value = V(Map.tryFind key model.V.Entries |> Option.defaultValue "")
            let var = Var.Make value (fun v -> dispatch (SetEntry (key, v)))
            div [Attr.Class "section"] [
                label [Attr.Class "label"] [text ("Editing value for key: " + key)]
                div [Attr.Class "field has-addons"] [
                    div [Attr.Class "control"] [Doc.InputType.Text [Attr.Class "input"] var]
                    div [Attr.Class "control"] [
                        button [
                            Attr.Class "button"
                            on.click (fun _ _ -> dispatch (Goto EndPoint.Home))
                        ] [text "Ok"]
                    ]
                    div [Attr.Class "control"] [
                        button [
                            Attr.Class "button"
                            on.click (fun _ _ -> dispatch (RemoveEntry key))
                        ] [text "Remove"]
                    ]
                ]
            ])

    let Render mdl =
        match mdl.EndPoint with
        | EndPoint.Home -> Pages.Home ()
        | EndPoint.EditEntry key -> Pages.EditEntry key

    let InitModel =
        {
            EndPoint = EndPoint.Home
            Entries = Map.empty
            Input = ""
        }

    [<SPAEntryPoint>]
    let Main () =
        App.CreateSimplePaged InitModel Update Render
        |> App.WithLocalStorage "mvu-tests"
        |> App.Run
        |> Doc.RunById "main"