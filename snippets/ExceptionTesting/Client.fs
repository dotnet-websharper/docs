namespace ExceptionTesting

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating

[<JavaScript>]
module Client =
    open WebSharper.Testing
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let RaisesTest = 
        Test "Exceptions" {
            raises (failwith "should fail")
            raisesMsg (failwith "should fail") "Failure is expected"
            raisesAsync (async { failwith "should fail" })
            raisesMsgAsync (async { failwith "should fail" }) "Failure is expected from inside async"
        }

    let MyTests = 
        TestCategory "MyTests" {
            RaisesTest
        }

    let RunAllTests() =
        Runner.RunTests [|
            MyTests
        |]

    [<SPAEntryPoint>]
    let Main () =
        RunAllTests() 
        |> fun control -> control.ReplaceInDom(JS.Document.GetElementById "test-root")
