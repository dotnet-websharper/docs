namespace MathML

open WebSharper
open WebSharper.JavaScript
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
        let text = 
            @"
            <math>
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
                        <mi>π</mi>
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

        rvExpression.View
        |> View.Sink (fun newHtml ->
            let el = JS.Document.GetElementById("tex")
            el.InnerHTML <- newHtml
            MathJax.Typeset()
        )

        IndexTemplate.Main()
            .Expression(rvExpression)
            .Doc()
        |> Doc.RunById "main"
