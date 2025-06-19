namespace Expressions

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.UI.Templating

[<JavaScript>]
module Client =
    open WebSharper.MathJax
    open WebSharper.MathJS

    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    [<SPAEntryPoint>]
    let Main () =

        let rvFormula = Var.Create "x^2/(x^9 + x^2) + x^4/2"
        let rvDerivateBy = Var.Create "x"

        let viewFormula = rvFormula.View
        let viewDerivateBy = rvDerivateBy.View

        let texFormula = 
            View.Map2 (fun (formula : string) (derivateby : string) ->
                try
                    let simplify = DerivativeOption(Simplify=true)
                    (Math.Derivative(formula, derivateby, simplify)).ToTex()
                with _ ->
                    "The\ formula\ isn\'t\ correct."
            ) viewFormula viewDerivateBy

        texFormula
        |> View.Sink (fun tex -> 
            let el = JS.Document.GetElementById("tex")
            el.InnerHTML <- "$$" + tex + "$$"
            MathJax.Typeset()
        )

        IndexTemplate.Main()
            .Formula(rvFormula)
            .DerivateBy(rvDerivateBy)
            .Doc()
        |> Doc.RunById "main"
