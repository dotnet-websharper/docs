namespace VShorthand

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html

[<JavaScript>]
module Client =

    // Define application state
    type State = 
        {
            Name: string
            Count: int
        }
    
    [<SPAEntryPoint>]
    let Main () =

        // Create a reactive variable for the application state
        let state = Var.Create { Name = "Guest"; Count = 0 }
                
        // Create a reactive view that combines name and count
        let greetingView = 
            V $"Hello, {state.V.Name}! Current count is {state.V.Count}."

        // UI components
        div [] [
            // Name input with two-way binding
            div [] [
                text $"Enter your name: "
                Doc.InputType.TextV [] state.V.Name
                // Display the current name
                div [] [
                    text $"Current name: {state.V.Name}"
                ]
            ]
            
            // Counter controls
            div [] [
                button [
                    on.click (fun _ _ -> state.Update(fun st -> { st with Count = st.Count + 1 }))
                ] [text "Increment"]
                button [
                    on.click (fun _ _ -> state.Update(fun st -> { st with Count = st.Count - 1 }))
                ] [text "Decrement"]
                // Display the current count
                div [] [
                    text $"Current count: {state.V.Count}"
                ]
            ]
            
            // Display the combined greeting
            div [] [
                h3 [] [text "Greeting:"]
                Doc.TextView greetingView
            ]
        ]
        |> Doc.RunById "main"