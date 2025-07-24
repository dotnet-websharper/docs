namespace CounterList

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.Mvu

/// The same Counter module as in http://try.websharper.com/snippet/loic.denuziere/0000Kf
/// with a single change -- an extra Id field on the model.
[<JavaScript>]
module Counter =
    
    type Model = { Id : int; Counter : int }
    
    let IdOf m = m.Id
    
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

[<JavaScript>]
module CountersApp =
    
    type Model = { Counters : list<Counter.Model> }
    
    type Message =
        | CounterMessage of int * Counter.Message
        
    let Update (msg: Message) (model: Model) =
        match msg with
        | CounterMessage (i, msg) ->
            let updatedCounters =
                model.Counters |> List.map (fun c ->
                    if c.Id = i then Counter.Update msg c else c)
            { model with Counters = updatedCounters }

    let Render (dispatch: Dispatch<Message>) (model: View<Model>) =
        let counters = V(model.V.Counters)
        div [] [
            // DocSeqCached renders a sequence of models.
            // The first argument is a key function used to identify each item
            // and reuse its already rendered view when the list changes.
            counters.DocSeqCached(Counter.IdOf, fun id vCounter ->
                let dispatch1 m = dispatch (CounterMessage (id, m))
                p [] [
                    text (sprintf "Counter #%i:" id)
                    Counter.Render dispatch1 vCounter
                ]
            )
        ]

    let Main =
        let initModel = { Counters = List.init 10 (fun i -> { Id = i+1; Counter = 0 }) }
        App.CreateSimple initModel Update Render
        |> App.Run
        |> Doc.RunById "main"