namespace MessagePassingWorker

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
        let echoWorker = new Worker(fun self ->
            // This is the code of the worker
 
            // Listen to messages from the main thread
            self.OnMessage <- fun event ->
 
                // Here we're assuming we'll only ever receive strings
                let msg = event.Data :?> string
 
                // Send a message to the main thread
                self.PostMessage("The worker said: " + msg)
        )
 
        // This code is on the main thread
 
        // Listen to messages from the worker
        echoWorker.OnMessage <- fun event ->
 
            // Again we're assuming we'll only ever receive strings
            let msg = event.Data :?> string
 
            // Print the received message
            Console.Log(msg)

            Var.Update workerResponse (fun s ->
                if System.String.IsNullOrEmpty s then msg else s + "\n" + msg
            )
 
        // This will send the string to the worker, receive it back,
        // and print to the console: "The worker said: Hello world!"
        echoWorker.PostMessage("Hello world!")
 
        // The worker is still running, we can send more messages
        // and they will be sent back and forth and printed.
        echoWorker.PostMessage("Hi again!")

    [<SPAEntryPoint>]
    let Main () =

        IndexTemplate.Main()
            .RunWorker(fun _ -> run())
            .WorkerResponse(workerResponse.V)
            .Doc()
        |> Doc.RunById "main"
