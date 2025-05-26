namespace Units

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
        let rvInput1 = Var.Create "1 cm"
        let rvInput2 = Var.Create "1 cm"
        let rvTranslate = Var.Create "m"
        
        let viewInput1 = rvInput1.View
        let viewInput2 = rvInput2.View
        let viewTranslate = rvTranslate.View
        
        let result operator = 
            View.Map2 (fun (l: string) (r: string) ->
                try
                    (operator (Math.Unit l) (Math.Unit r)).ToString()
                with _ ->
                    "Wrong input"
            ) viewInput1 viewInput2
            
        let resultAdd = result (fun l r -> Math.Add(l, r))
        let resultSubtract = result (fun l r -> Math.Subtract(l, r))
        let resultMultiply = result (fun l r -> Math.Multiply(l, r))
        let resultDivide = result (fun l r -> Math.Divide(l, r))
        
        let translate unit trans = 
            View.Map2 (fun (u: string) (t: string) ->
                try
                    (Math.To(Math.Unit(u), Math.Unit(t))).ToString()
                with _ ->
                    "Couldn't translate."
            ) unit trans
            
        let translateResultAdd = translate resultAdd viewTranslate    
        let translateResultSubtract = translate resultSubtract viewTranslate    
        let translateResultMultiply = translate resultMultiply viewTranslate    
        let translateResultDivide = translate resultDivide viewTranslate
        
        IndexTemplate.Main()
            .Input1(rvInput1)
            .Input2(rvInput2)
            .Translate(rvTranslate)
            .ResultAdd(resultAdd)
            .ResultSubtract(resultSubtract)
            .ResultMultiply(resultMultiply)
            .ResultDivide(resultDivide)
            .TranslatedResultAdd(translateResultAdd)
            .TranslatedResultSubtract(translateResultSubtract)
            .TranslatedResultMultiply(translateResultMultiply)
            .TranslatedResultDivide(translateResultDivide)
            .Doc()
        |> Doc.RunById "main"
