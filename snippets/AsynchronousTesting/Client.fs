namespace AsynchronousTesting

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI.Templating

[<JavaScript>]
module Client =
    open WebSharper.Testing
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let AsyncTest = 
        Test "Equality" {
            equal 1 1 
            let! one = async { return 1 }
            equal one 1
        }

    let MyTests = 
        TestCategory "MyTests" {
            AsyncTest
        }

    let RunAllTests() =
        Runner.RunTests [|
            MyTests
        |]

    [<SPAEntryPoint>]
    let Main () =
        RunAllTests() 
        |> fun control -> control.ReplaceInDom(JS.Document.GetElementById "test-root")
