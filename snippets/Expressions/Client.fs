namespace Expressions

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
    open WebSharper.MathJS

    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    [<SPAEntryPoint>]
    let Main () =
        Hub.Config(
            MathJax.Config(
                Extensions = [| "tex2jax.js" |],
                Jax = [| "input/TeX"; "output/HTML-CSS"; |],
                Tex2jax = Tex2jax(InlineMath = [| ("$", "$"); ("\\(", "\\)") |])
            )
        )

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

        let tex = 
            div [
                on.viewUpdate texFormula (fun e v -> 
                    JQuery.Of("#tex div").Empty().Append("$$" + v + "$$").Ignore
                    Hub.Queue([| "Typeset", MathJax.Hub :> obj, [| e :> obj |] |]) |> ignore
                )
            ] [
                textView <| texFormula.Map (fun x -> "$$" + x + "$$")
            ]

        Doc.RunById "tex" tex

        IndexTemplate.Main()
            .Formula(rvFormula)
            .DerivateBy(rvDerivateBy)
            .Doc()
        |> Doc.RunById "main"
