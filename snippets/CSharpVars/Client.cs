using WebSharper;
using WebSharper.UI;
using WebSharper.UI.Client;
using static WebSharper.UI.Client.Html;

namespace CSharpVars
{
    [JavaScript]
    public class App
    {
        [SPAEntryPoint]
        public static void ClientMain()
        {
            // Create a reactive variable for the name
            var nameVar = Var.Create("Guest");

            // Create a reactive variable for the counter
            var countVar = Var.Create(0);

            // Create a reactive view that combines name and count
            var greetingView =
                nameVar.View.Map2(countVar.View, (name, count) =>
                    $"Hello, {name}! Current count is {count}."
                );

            // UI components
            div(
                // Name input with two-way binding
                div(
                    "Enter your name: ",
                    input(nameVar),
                    // Display the current name
                    div(
                        "Current name: ",
                        nameVar
                    )
                ),
            
                // Counter controls
                div(
                    button("Increment",
                        () => countVar.Value++),
                    button("Decrement",
                        () => countVar.Value--),
                    // Display the current count
                    div(
                        "Current count: ",
                        countVar
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
