namespace AsciiMath

open WebSharper
open WebSharper.JavaScript
open WebSharper.JQuery
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.UI.Templating

[<JavaScript>]
module Client =
    open WebSharper.MathJax

    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    [<SPAEntryPoint>]
    let Main () =
        Hub.Config(
            MathJax.Config(
                Extensions = [| "asciimath2jax.js" |],
                Jax = [| "input/AsciiMath"; "output/HTML-CSS"; |],
                Asciimath2jax = Asciimath2jax(
                    Delimiters = [| ("`", "`") |],
                    Preview = [| "AsciiMath" |]
                )
            )
        )

        let rvExpression = Var.Create @"sum_(i=1)^n i^3=((n(n+1))/2)^2"
        
        let viewExpression = rvExpression.View

        let tex = 
            div [
                on.viewUpdate viewExpression (fun e v -> 
                    JQuery.Of("#tex div").Empty().Append("`" + v + "`").Ignore
                    Hub.Queue([| "Typeset", MathJax.Hub :> obj, [| e :> obj |] |]) |> ignore
                )
            ] [
                textView <| viewExpression.Map (fun x -> "`" + x + "`")
            ]

        Doc.RunById "tex" tex

        IndexTemplate.Main()
            .Expression(rvExpression)
            .Doc()
        |> Doc.RunById "main"
