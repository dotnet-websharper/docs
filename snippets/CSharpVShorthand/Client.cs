using Microsoft.AspNetCore.Components.Forms;
using System.Collections;
using System.Diagnostics.Metrics;
using WebSharper;
using WebSharper.UI;
using WebSharper.UI.Client;
using static WebSharper.UI.Client.Html;

namespace CSharpVShorthand
{
    [JavaScript]
    public record State(string Name, int Count);

    [JavaScript]
    public class App
    {
        [SPAEntryPoint]
        public static void ClientMain()
        {
            // Create a reactive variable for the application state
            var state = Var.Create(new State("Guest", 0));

            // Create a reactive view that combines name and count
            var greetingView =
                V.V($"Hello, {state.V.Name}! Current count is {state.V.Count}.");

            // UI components
            div(
                // Name input with two-way binding
                div(
                    "Enter your name: ",
                    input(state.V.Name),
                    // Display the current name
                    div(
                        $"Current name: {state.V.Name}"
                    )
                ),

                // Counter controls
                div(
                    button("Increment",
                        () => state.Update(st => st with { Count = st.Count + 1 })
                    ),
                    button("Decrement",
                        () => state.Update(st => st with { Count = st.Count - 1 })
                    ),
                    // Display the current count
                    div(
                        $"Current count: {state.V.Count}"
                    )
                ),
            
                // Display the combined greeting
                div(
                    h3("Greeting:"),
                    greetingView
                )
            ).RunById("main");
        }
    }
}
