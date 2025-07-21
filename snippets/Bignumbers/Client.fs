namespace Bignumbers

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.MathJS

[<JavaScript>]
module Client =
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    [<SPAEntryPoint>]
    let Main () =
        let rvInput1 = Var.Create "1"
        let rvInput2 = Var.Create "1"

        let viewInput1 = rvInput1.View
        let viewInput2 = rvInput2.View

        let result operator = 
            View.Map2 (fun (l: string) (r: string) ->
                try
                    (operator (Math.Bignumber l) (Math.Bignumber r)).ToString()
                with _ ->
                    "Wrong input"
            ) viewInput1 viewInput2

        let resultAdd = result (fun l r -> l + r)
        let resultSubtract = result (fun l r -> l - r)
        let resultMultiply = result (fun l r -> l * r)
        let resultDivide = result (fun l r -> l / r)

        IndexTemplate.Main()
            .Input1(rvInput1)
            .Input2(rvInput2)
            .ResultAdd(resultAdd)
            .ResultSubtract(resultSubtract)
            .ResultMultiply(resultMultiply)
            .ResultDivide(resultDivide)
            .Doc()
        |> Doc.RunById "main"
