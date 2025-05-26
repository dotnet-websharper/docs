namespace Vectors

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating

[<JavaScript>]
module Client =    
    open WebSharper.MathJS
    open WebSharper.MathJax

    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    [<SPAEntryPoint>]
    let Main () =
        MathJax.Hub.Config(
            MathJax.Config(
                Extensions = [| "tex2jax.js" |],
                Jax = [| "input/TeX"; "output/HTML-CSS"; |],
                Tex2jax = MathJax.Tex2jax(InlineMath = [| ("$", "$"); ("\\(", "\\)") |])
            )
        )

        let vector1 = [| 8.; 6.; 4.; 6. |]
        let vector2 = [| 6.; 14.; 8.; 2. |]

        let toPrettyVector (vector: float array) = 
            "$" + ((Math.Parse("[" + string vector + "]") |> As<Node>).ToTex()) + "$"

        let prettyVector1 = toPrettyVector vector1
        let prettyVector2 = toPrettyVector vector2

        let result op = "[" + string (op vector1 vector2) + "]"

        let resultAdd = result (fun l r -> Math.Add(MathNumber(l), MathNumber(r)))
        let resultSubtract = result (fun l r -> Math.Subtract(MathNumber(l), MathNumber(r)))
        let resultMultiply = result (fun l r -> Math.Multiply(MathNumber(l), MathNumber(r)))

        let toPrettyResult (result: string) =
            "$" + ((Math.Parse(string result) |> As<Node>).ToTex()) + "$"

        let prettyResultAdd = toPrettyResult resultAdd
        let prettyResultSubtract = toPrettyResult resultSubtract
        let prettyResultMultiply = toPrettyResult resultMultiply

        IndexTemplate.Main()
            .Vector1(prettyVector1)
            .Vector2(prettyVector2)
            .ResultAdd(prettyResultAdd)
            .ResultSubtract(prettyResultSubtract)
            .ResultMultiply(prettyResultMultiply)
            .Doc()
        |> Doc.RunById "main"
