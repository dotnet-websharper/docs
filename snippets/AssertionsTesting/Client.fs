namespace AssertionsTesting

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

    let IsTrueTest = 
        Test "Equality" {
            isTrue (1 = 1)
            isTrueMsg (1 = 1) "One equals one"
            isTrueAsync (async { return 1 = 1 }) 
            isTrueMsgAsync (async { return 1 = 1 }) "One equals one, async version"
        }

    let IsFalseTest = 
        Test "Equality" {
            isFalse (1 = 2)
            isFalseMsg (1 = 2) "One equals two is false"
            isFalseAsync (async { return 1 = 2 }) 
            isFalseMsgAsync (async { return 1 = 2 }) "One equals two is false, async version"
        }

    let MyTests = 
        TestCategory "MyTests" {
            IsTrueTest
            IsFalseTest
        }

    let RunAllTests() =
        Runner.RunTests [|
            MyTests
        |]

    [<SPAEntryPoint>]
    let Main () =
        RunAllTests() 
        |> fun control -> control.ReplaceInDom(JS.Document.GetElementById "test-root")
