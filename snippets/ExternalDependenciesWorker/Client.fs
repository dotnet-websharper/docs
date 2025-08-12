namespace ExternalDependenciesWorker

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Notation
open WebSharper.UI.Client
open WebSharper.UI.Templating

[<JavaScript>]
module Client =
    open WebSharper.MathJS
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let workerResponse = Var.Create ""

    let run () =
        let myWorker = new Worker(fun self ->
            self.OnMessage <- fun event ->
                let msg = event.Data :?> string
                let res = Math.Derivative(msg, "x").ToString()
                Console.Log(res)
                self.PostMessage(res)
        )

        // Print result on the webpage
        myWorker.OnMessage <- fun event ->
            let res = event.Data :?> string
            workerResponse := res

        // This will print "4 * x + 3":
        myWorker.PostMessage("2x^2 + 3x + 4")

    [<SPAEntryPoint>]
    let Main () =

        IndexTemplate.Main()
            .RunWorker(fun _ -> run())
            .WorkerResponse(workerResponse.V)
            .Doc()
        |> Doc.RunById "main"
