---
title: Multi-stage flows
---

import { FSharpSnippetTabs } from "@/lib/components/FSharpSnippetTabs";

## Defining flows

Flows are a helper for creating multi-stage controls with back/forward navigation, where each stage can bind to the results of previous stages.

Extra features are automatic browser history handling, cancellation and restart.

There are 3 kinds of stages:
* Intermediate stage, that has a result passed along to further stages, and has access to Back/Next/Cancel actions. This is created by `Flow.Define`.
* Ending stage, which is a static document, intended to be used when the collected data has already been submitted. The `Flow.End` helper creates an ending stage where there is no stepping back without rerendering the whole form. While `Flow.EndRestartable` can restart the flow from the first stage.
* A cancelled stage, which shows a static document with a Restart action available to do a fresh restart of the flow. This is created by using `Flow.EmbedWithCancel` when embedding the flow into the page.

There is a `flow` computation builder to chain stages easily. An example:

```fsharp
let CombinedFlow() =
    flow {
        let! name = Flow.Define Stage1
        let! age = Flow.Define Stage2
        return! Flow.EndRestartable (StageEnd name age)
    }
```

The most important thing to keep in mind is that every flow stage's render function is called only once, then the result will be reused if navigated away back to that stage.
This is not a problem because we can pass on `View`s or `Var`s from earlier stages to have reactive content that changes based on user inputs. 

Now, to define a first stage, we write a render function that takes a `FlowActions<_>` object.

```fsharp
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
```

Remember, a flow stage is rendered only once, so we are free to create variables, just need to make sure that all information will be visible in the value that is passed forward to the `actions.Next` method.
We can now create a `Flow<View<string>>` with `Flow.Define Stage1`.

## Adding validation

For full support of browser navigation, we will also need to add validator functions to flow stages. These ensure that the flow won't get into invalid state using forward browser navigation.
If you want to validate only before submitting, you can always define a validator for a stage that passes with any value. For example for stage 1:

```fsharp
let ValidateStage1 (name: string) =
    if name = "" then
        JS.Alert "Please provide a name"
        false
    else
        true
```

We add it to the flow with `Flow.Define Stage1 |> Flow.ValidateView ValidateStage1`. There is also a `ValidateVar` helper for validating values of `Var`s passed forward.

## Mapping

Our sample will also define a stage 2, resulting in a `View<Checkedinput<int>>`.
Once we validate for a valid value, we can map the flow's result with `Flow.Map`, for example:

```
Flow.Define Stage2
|> Flow.ValidateView ValidateStage2
|> Flow.Map (View.Map (function Valid (a, _) -> a | _ -> 0))
```

This creates a `Flow<View<int>>`, removing the UI validation concern when processing the result from further stages.

## Ending stage

We render the ending stage by taking the previous `View`s and an `EndedFlowActions` object.

```fsharp
let StageEnd (name: View<string>) (age: View<int>) (actions: EndedFlowActions) =
    div [] [
        p [] [ 
            text $"Hello {name.V}, age {age.V}" 
        ]
        p [] [
            Doc.Button "Restart" [] actions.Restart
        ]
    ]
```

Then we can chain everything using `Flow.RestartableEnd`. 

```fsharp
    let CombinedFlow() =
        flow {
            let! name = Flow.Define Stage1 |> Flow.ValidateView ValidateStage1
            let! age = 
                Flow.Define Stage2 |> Flow.ValidateView ValidateStage2
                |> Flow.Map (View.Map (function Valid (a, _) -> a | _ -> 0))
            return! Flow.EndRestartable (StageEnd name age)
        }
```

If we don't need the Restart action, we could use the simpler `Flow.End`.

## Embedding and cancelling

To convert a `Flow<_>` into a `Doc` to add to your page, use `Flow.Embed`.
However, we have used a Cancel action before, to create a cancel page, we will use `Flow.EmbedWithCancel`.

```fsharp
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

```

## Simplified handling without forward navigation

If we don't want to allow forward navigation in the browser, we don't need validators either.
Then the user will be only able to move forward by pressing the button again that calls `actions.Next`.
In this case, we can pass a plain result value only to `actions.Next` then use `Flow.View` to create a `View` of it for further stages.

Example:

```
let Stage1Val (actions: FlowActions<_>) =
    let nameVar = Var.Create ""
    div [] [
        p [] [ 
            text "Name: " 
            Doc.InputType.Text [] nameVar
        ]
        p [] [
            Doc.Button "Next" [ attr.style "margin-right: 0.5em" ] (fun () -> 
                actions.Next nameVar.Value
            )
            Doc.Button "Cancel" [] actions.Cancel
        ]
    ]
```

produces a `Flow<int>`. This would bake in the result value once navigating forward, but `Stage1Val |> Flow.View` handles back-navigating and calling `Next` with a new value.

## Sample

This is the whole sample:

<FSharpSnippetTabs
  snippet="Flow"
  liveSnippetHeight="500"
  highlightLines=""
  defTab="preview"
/>