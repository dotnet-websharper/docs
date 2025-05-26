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

        let prettyVector1 = "$" + ((Math.Parse("[" + string vector1 + "]") |> As<Node>).ToTex()) + "$"
        let prettyVector2 = "$" + ((Math.Parse("[" + string vector2 + "]") |> As<Node>).ToTex()) + "$"

        let result op = "[" + string (op vector1 vector2) + "]"

        let resultAdd = result (fun l r -> Math.Add(MathNumber(l), MathNumber(r)))
        let resultSubtract = result (fun l r -> Math.Subtract(MathNumber(l), MathNumber(r)))
        let resultMultiply = result (fun l r -> Math.Multiply(MathNumber(l), MathNumber(r)))

        let prettyResultAdd = "$" + ((Math.Parse(string resultAdd) |> As<Node>).ToTex()) + "$"
        let prettyResultSubtract = "$" + ((Math.Parse(string resultSubtract) |> As<Node>).ToTex()) + "$"
        let prettyResultMultiply = "$" + ((Math.Parse(string resultMultiply) |> As<Node>).ToTex()) + "$"

        IndexTemplate.Main()
            .Vector1(prettyVector1)
            .Vector2(prettyVector2)
            .ResultAdd(prettyResultAdd)
            .ResultSubtract(prettyResultSubtract)
            .ResultMultiply(prettyResultMultiply)
            .Doc()
        |> Doc.RunById "main"
