namespace Flow

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html

[<JavaScript>]
module Client =

    let Stage1 (actions: FlowActions<_>) =
        let nameVar = Var.Create ""
        div [] [
            p [] [ 
                text "Name: " 
                Doc.InputType.Text [] nameVar
            ]
            p [] [
                Doc.Button "Next" [ attr.style "margin-right: 0.5em" ] (fun () -> 
                    actions.Next nameVar.View
                )
                Doc.Button "Cancel" [] actions.Cancel
            ]
        ]

    let ValidateStage1 (name: string) =
        if name = "" then
            JS.Alert "Please provide a name"
            false
        else
            true

    let Stage2 (actions: FlowActions<_>) =
        let ageVar = Var.Create (CheckedInput.Make 1)
        div [] [
            p [] [ 
                text "Age: " 
                Doc.InputType.Int [] ageVar
            ]
            p [] [
                Doc.Button "Back" [ attr.style "margin-right: 0.5em" ] actions.Back
                Doc.Button "Submit" [ attr.style "margin-right: 0.5em" ] (fun () -> 
                    actions.Next ageVar.View
                )
                Doc.Button "Cancel" [] actions.Cancel
            ]
        ]

    let ValidateStage2 (age: CheckedInput<int>) =
        match age with
        | Valid (a, _) ->
            if 0 <= a && a <= 99 then
                true
            else
                JS.Alert "Please provide and age between 0 and 99"
                false
        | _ -> 
            JS.Alert "Please enter a valid number"
            false

    let StageEnd (name: View<string>) (age: View<int>) (actions: EndedFlowActions) =
        div [] [
            p [] [ 
                text $"Hello {name.V}, age {age.V}" 
            ]
            p [] [
                Doc.Button "Restart" [] actions.Restart
            ]
        ]

    let CombinedFlow() =
        flow {
            let! name = Flow.Define Stage1 |> Flow.ValidateView ValidateStage1
            let! age = 
                Flow.Define Stage2 |> Flow.ValidateView ValidateStage2
                |> Flow.Map (View.Map (function Valid (a, _) -> a | _ -> 0))
            return! Flow.EndRestartable (StageEnd name age)
        }

    let Cancelled (actions: EndedFlowActions) =
        div [] [
            p [] [ text "Canceled" ]
            p [] [ Doc.Button "Restart" [] actions.Restart ]
        ]    

    [<SPAEntryPoint>]
    let Main () =
        CombinedFlow()
        |> Flow.EmbedWithCancel Cancelled
        |> Doc.RunById "main"
