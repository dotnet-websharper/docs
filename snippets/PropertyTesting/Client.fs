namespace PropertyTesting

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

    let PropertyTest = 
        Test "Equality on ints is reflexive" {
            property (fun (x: int) ->
                Do {
                    equal x x 
                }
            )
        }

    let PropertyWithTest = 
        Test "Equality on ints is reflexive" {
            propertyWith RandomValues.Int (fun (x: int) ->
                Do {
                    equal x x 
                }
            )
        }

    let PropertyWithSampleTest = 
        Test "Equality on ints is reflexive" {
            propertyWithSample (RandomValues.Sample [ 1; 2; 3 ]) (fun (x: int) ->
                Do {
                    equal x x 
                }
            )
        }

    let MyTests = 
        TestCategory "MyTests" {
            PropertyTest
            PropertyWithTest
            PropertyWithSampleTest
        }

    let RunAllTests() =
        Runner.RunTests [|
            MyTests
        |]

    [<SPAEntryPoint>]
    let Main () =
        RunAllTests() 
        |> fun control -> control.ReplaceInDom(JS.Document.GetElementById "test-root")

