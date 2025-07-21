namespace Vars

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html

[<JavaScript>]
module Client =
    [<SPAEntryPoint>]
    let Main () =

        // Create a reactive variable for the name
        let nameVar = Var.Create "Guest"
        
        // Create a reactive variable for the counter
        let countVar = Var.Create 0
        
        // Create a reactive view that combines name and count
        let greetingView = 
            View.Map2 (fun name count -> 
                sprintf "Hello, %s! Current count is %d." name count)
                nameVar.View
                countVar.View

        // UI components
        div [] [
            // Name input with two-way binding
            div [] [
                text "Enter your name: "
                Doc.InputType.Text [] nameVar
                // Display the current name
                div [] [
                    text "Current name: "
                    Doc.TextView nameVar.View
                ]
            ]
            
            // Counter controls
            div [] [
                button [
                    on.click (fun _ _ -> countVar.Value <- countVar.Value + 1)
                ] [text "Increment"]
                button [
                    on.click (fun _ _ -> countVar.Value <- countVar.Value - 1)
                ] [text "Decrement"]
                // Display the current count
                div [] [
                    text "Current count: "
                    Doc.TextView (countVar.View.Map string)
                ]
            ]
            
            // Display the combined greeting
            div [] [
                h3 [] [text "Greeting:"]
                Doc.TextView greetingView
            ]
        ]
        |> Doc.RunById "main"