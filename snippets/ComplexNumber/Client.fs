namespace ComplexNumber

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
        let rvInput1 = Var.Create "1 + 2i"
        let rvInput2 = Var.Create "2 + 1i"
        
        let viewInput1 = rvInput1.View
        let viewInput2 = rvInput2.View

        let result op = 
            View.Map2 (fun (l: string) (r: string) ->
                try
                    op (Math.Complex l) (Math.Complex r) |> string
                with _ ->
                    "Wrong input"
            ) viewInput1 viewInput2
            
        let resultAdd = result ( + )
        let resultSubtract = result ( - )
        let resultMultiply = result ( * )
        let resultDivide = result ( / )

        IndexTemplate.Main()
            .Input1(rvInput1)
            .Input2(rvInput2)
            .ResultAdd(resultAdd)
            .ResultSubtract(resultSubtract)
            .ResultMultiply(resultMultiply)
            .ResultDivide(resultDivide)
            .Doc()
        |> Doc.RunById "main"
