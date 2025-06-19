namespace TeX

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

        let rvExpression = Var.Create @"\frac{2\cdot x\cdot\left({ x}^{9}+{ x}^{2}\right)-{ x}^{2}\cdot\left(9\cdot{ x}^{8}+2\cdot x\right)}{{\left({ x}^{9}+{ x}^{2}\right)}^{2}}+2\cdot{ x}^{3}"

        rvExpression.View
        |> View.Sink (fun expr ->
            let output = JS.Document.GetElementById("tex")
            output.InnerHTML <- "$$" + expr + "$$"
            MathJax.Typeset()
        )

        IndexTemplate.Main()
            .Expression(rvExpression)
            .Doc()
        |> Doc.RunById "main"
