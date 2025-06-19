namespace Matrices

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating

[<JavaScript>]
module Client =
    open WebSharper.MathJax
    open WebSharper.MathJS

    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    [<SPAEntryPoint>]
    let Main () =
        let matrix1 = Math.Matrix([| [| 8.; 6. |]; [| 4.; 6. |] |])
        let matrix2 = Math.Matrix([| [| 6.; 14. |]; [| 8.; 2. |] |])

        let toPrettyMatrix (matrix: float array array) =
            "$" + ((Math.Parse(string matrix) |> As<Node>).ToTex()) + "$"

        let prettyMatrix1 = toPrettyMatrix matrix1
        let prettyMatrix2 = toPrettyMatrix matrix2

        let result op = (op matrix1 matrix2)

        let resultAdd = result (fun l r -> Math.Add(MathNumber(l), MathNumber(r)))
        let resultSubtract = result (fun l r -> Math.Subtract(MathNumber(l), MathNumber(r)))
        let resultMultiply = result (fun l r -> Math.Multiply(MathNumber(l), MathNumber(r)))
        let resultDivide = result (fun l r -> Math.Divide(MathNumber(l), MathNumber(r)))

        let toPrettyResult (resultMatrix: MathNumber) =
            "$" + ((Math.Parse(string resultMatrix) |> As<Node>).ToTex()) + "$"

        let prettyResultAdd = toPrettyResult resultAdd
        let prettyResultSubtract = toPrettyResult resultSubtract
        let prettyResultMultiply = toPrettyResult resultMultiply
        let prettyResultDivide = toPrettyResult resultDivide

        IndexTemplate.Main()
            .Matrix1(prettyMatrix1)
            .Matrix2(prettyMatrix2)
            .ResultAdd(prettyResultAdd)
            .ResultSubtract(prettyResultSubtract)
            .ResultMultiply(prettyResultMultiply)
            .ResultDivide(prettyResultDivide)
            .PageInit(fun () -> 
                MathJax.Typeset()
            )
            .Doc()
        |> Doc.RunById "main"
