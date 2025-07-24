namespace Counter

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.Mvu

[<JavaScript>]
module Counter =
    
    type Model = { Counter : int }
    
    type Message = Increment | Decrement
    
    let Update (msg: Message) (model: Model) =
        match msg with
        | Increment -> { model with Counter = model.Counter + 1 }
        | Decrement -> { model with Counter = model.Counter - 1 }
        
    let Render (dispatch: Dispatch<Message>) (model: View<Model>) =
        div [] [
            button [on.click (fun _ _ -> dispatch Decrement)] [text "-"]
            span [] [text (sprintf " %i " model.V.Counter)]
            button [on.click (fun _ _ -> dispatch Increment)] [text "+"]
        ]

    [<SPAEntryPoint>]
    let Main =
        App.CreateSimple { Counter = 0 } Update Render
        |> App.Run
        |> Doc.RunById "main"
