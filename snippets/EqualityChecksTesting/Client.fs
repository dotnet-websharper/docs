namespace EqualityChecksTesting

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

    let EqualTest = 
        Test "Equality" {
            equal (Some 1) (Some 1)
            equalMsg (Some 1) (Some 1) "Option equality"
            equalAsync (async { return Some 1 }) (Some 1)
            equalMsgAsync (async { return Some 1 }) (Some 1) "Option equality, async version"
        }

    let JsEqualTest = 
        Test "Equality" {
            jsEqual 1 1
            jsEqualMsg 1 1 "One equals one"
            jsEqualAsync (async { return 1 }) 1
            jsEqualMsgAsync (async { return 1 }) 1 "One equals one, async version"
        }

    let MyTests = 
        TestCategory "MyTests" {
            EqualTest
            JsEqualTest
        }

    let RunAllTests() =
        Runner.RunTests [|
            MyTests
        |]

    [<SPAEntryPoint>]
    let Main () =
        RunAllTests() 
        |> fun control -> control.ReplaceInDom(JS.Document.GetElementById "test-root")
