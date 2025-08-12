namespace CategoriesTesting

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
    
    let EqualityTests() =
        Test "Equality" {
            equal 1 1
            notEqual 1 2    
        }

    let MyTests =
        TestCategory "MyTests" {
            Test "Equality" { equal 1 1 }
            EqualityTests()
        }    

    let RunAllTests() =
        Runner.RunTests [|
            MyTests
        |]

    [<SPAEntryPoint>]
    let Main () =
        RunAllTests() 
        |> fun control -> control.ReplaceInDom(JS.Document.GetElementById "test-root")

        IndexTemplate.Main()
            .Doc()
            |> Doc.RunById "main"