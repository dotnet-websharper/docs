namespace AsciiMath

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating

[<JavaScript>]
module Client =
    open WebSharper.MathJax

    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    [<SPAEntryPoint>]
    let Main () =
        let rvExpression = Var.Create @"sum_(i=1)^n i^3=((n(n+1))/2)^2"
        
        rvExpression.View
        |> View.Sink (fun ascii ->
            let el = JS.Document.GetElementById("tex")
            el.InnerHTML <- "`" + ascii + "`"
            MathJax.Typeset()
        )

        IndexTemplate.Main()
            .Expression(rvExpression)
            .Doc()
        |> Doc.RunById "main"
