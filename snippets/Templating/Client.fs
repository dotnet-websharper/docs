namespace Templating

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating

[<JavaScript>]
module Client =
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    [<SPAEntryPoint>]
    let Main () =
        // Create a reactive variable for the name
        let nameVar = Var.Create "Guest"
        
        // Create a reactive variable for the counter
        let countVar = Var.Create 0

        // Create a list model, keyed by an index
        let myList = ListModel.Create fst []

        // Create a mutable index for uniquely tracking items
        let itemIndex = ref 0

        // Setting up the view through the template
        IndexTemplate.MainTemplate()
            .Name(nameVar)
            .Count(string countVar.V)
            .Increment(fun _ -> countVar.Value <- countVar.Value + 1)
            .Decrement(fun _ -> countVar.Value <- countVar.Value - 1)
            .Greeting($"Hello, {nameVar.V}! Current count is {countVar.V}.")
            .AddItem(fun e ->
                // We don't need to define a Var for the new item name input,
                // the template will create one for us if we don't provide it.
                myList.Add(itemIndex.Value, e.Vars.NewItem.Value)
                itemIndex.Value <- itemIndex.Value + 1
                e.Vars.NewItem.Value <- ""
            )
            .Items(
                myList.View.DocSeqCached(fun (index: int, item: string) ->
                    IndexTemplate.ItemTemplate()
                        .ItemName(item)
                        .RemoveItem(fun _ ->
                            myList.RemoveByKey(index)
                        )
                        .Doc()
                )
            )
            .Doc()
        |> Doc.RunById "main"
