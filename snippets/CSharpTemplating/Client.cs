using System;
using WebSharper;
using WebSharper.UI;
using WebSharper.UI.Client;
using static WebSharper.UI.Client.Html;

namespace CSharpTemplating
{
    [JavaScript]
    public class App
    {
        [SPAEntryPoint]
        public static void ClientMain()
        {
            // Create a reactive variable for the name
            var nameVar = Var.Create("Guest");

            // Create a reactive variable for the counter
            var countVar = Var.Create(0);

            // Create a list model, keyed by an index
            var myList = new ListModel<int, (int Index, string Item)>(i => i.Index);

            // Create an index for uniquely tracking items
            var itemIndex = 0;

            // Setting up the view through the template
            new Template.Index.MainTemplate()
                .Name(nameVar)
                .Count(countVar.View.Map(c => c.ToString()))
                .Increment(() => countVar.Value++)
                .Decrement(() => countVar.Value--)
                .Greeting(V.V($"Hello, {nameVar.V}! Current count is {countVar.V}."))
                .AddItem((e) =>
                    {
                        // We don't need to define a Var for the new item name input,
                        // the template will create one for us if we don't provide it.
                        myList.Add((itemIndex, e.Vars.NewItem.Value));
                        itemIndex++;
                        e.Vars.NewItem.Value = "";
                    }
                )
                .Items(
                    myList.View.DocSeqCached(i =>
                        new Template.Index.ItemTemplate()
                            .ItemName(i.Item)
                            .RemoveItem(() =>
                                myList.RemoveByKey(i.Index)
                            )
                            .Doc()
                    )
                )
                .Doc()
                .RunById("main");
        }
    }
}
