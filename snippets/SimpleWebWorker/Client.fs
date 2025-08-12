namespace SimpleWebWorker

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Notation
open WebSharper.UI.Client
open WebSharper.UI.Templating

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let workerResponse = Var.Create ""

    let run () =
        let myWorker = new Worker(fun self ->
            Console.Log "This was written from the worker!"
            self.PostMessage("This worker's job is done, it can be terminated.")
        )
        myWorker.OnMessage <- fun event ->
            workerResponse := event.Data :?> string
            myWorker.Terminate()

    [<SPAEntryPoint>]
    let Main () =

        IndexTemplate.Main()
            .RunWorker(fun _ -> run())
            .WorkerResponse(workerResponse.V)
            .Doc()
        |> Doc.RunById "main"
