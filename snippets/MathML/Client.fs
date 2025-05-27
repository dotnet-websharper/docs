namespace MathML

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
                Extensions = [| "mml2jax.js" |],
                Jax = [| "input/MathML"; "output/HTML-CSS"; |],        
                Mml2jax = Mml2jax(Preview = [| "mathml" |])
            )
        )

        let text = 
            "\t" + @"<math>
                <mstyle>
                    <mi>f</mi>
                    <mrow>
                      <mo>(</mo>
                      <mi>a</mi>
                      <mo>)</mo>
                    </mrow>
                    <mo>=</mo>
                    <mfrac>
                      <mn>1</mn>
                      <mrow>
                        <mn>2</mn>
                        <mi>π
                        </mi>
                        <mi>i</mi>
                      </mrow>
                    </mfrac>
                    <msub>
                      <mo>∮</mo>
                      <mrow>
                        <mi>γ</mi>
                      </mrow>
                    </msub>
                    <mfrac>
                      <mrow>
                        <mi>f</mi>
                        <mo>(</mo>
                        <mi>z</mi>
                        <mo>)</mo>
                      </mrow>
                      <mrow>
                        <mi>z</mi>
                        <mo>−</mo>
                        <mi>a</mi>
                      </mrow>
                    </mfrac>
                    <mi>d</mi>
                    <mi>z</mi>
                  </mstyle>
                </math>"

        let rvExpression = Var.Create text

        let viewExpression = rvExpression.View

        let tex = 
            div [
                on.viewUpdate viewExpression (fun e v -> 
                    JQuery.Of("#tex div").Empty().Append(v).Ignore
                    Hub.Queue([| "Typeset", MathJax.Hub :> obj, [| e :> obj |] |]) |> ignore
                )
            ] [
                textView <| viewExpression
            ]

        Doc.RunById "tex" tex

        IndexTemplate.Main()
            .Expression(rvExpression)
            .Doc()
        |> Doc.RunById "main"
