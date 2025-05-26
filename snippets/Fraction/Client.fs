namespace Fraction

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating

[<JavaScript>]
module Client =
    open WebSharper.MathJS

    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    [<SPAEntryPoint>]
    let Main () =
        let rvInput1 = Var.Create "0.1"
        let rvInput2 = Var.Create "0.2"
        
        let viewInput1 = rvInput1.View
        let viewInput2 = rvInput2.View
        
        let result parse operator = 
            View.Map2 (fun (l: string) (r: string) ->
                try
                    (operator (parse l) (parse r)).ToString()
                with _ ->
                    "Wrong input"
            ) viewInput1 viewInput2
            
        let resultFloatAdd = result System.Double.Parse (fun l r -> l + r)
        let resultFloatSubtract = result System.Double.Parse (fun l r -> l - r)
        let resultFloatMultiply = result System.Double.Parse (fun l r -> l * r)
        let resultFloatDivide = result System.Double.Parse (fun l r -> l / r)
        
        let resultFractionAdd = result Math.Fraction (fun l r -> Math.Add(l, r)) 
        let resultFractionSubtract = result Math.Fraction (fun l r -> Math.Subtract(l, r))
        let resultFractionMultiply = result Math.Fraction (fun l r -> Math.Multiply(l, r))
        let resultFractionDivide = result Math.Fraction (fun l r -> Math.Divide(l ,r))

        IndexTemplate.Main()
            .Input1(rvInput1)
            .Input2(rvInput2)
            .ResultFloatAdd(resultFloatAdd)
            .ResultFloatSubtract(resultFloatSubtract)
            .ResultFloatMultiply(resultFloatMultiply)
            .ResultFloatDivide(resultFloatDivide)
            .ResultFractionAdd(resultFractionAdd)
            .ResultFractionSubtract(resultFractionSubtract)
            .ResultFractionMultiply(resultFractionMultiply)
            .ResultFractionDivide(resultFractionDivide)
            .Doc()
        |> Doc.RunById "main"
