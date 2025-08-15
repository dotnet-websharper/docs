using WebSharper;
using WebSharper.UI;
using WebSharper.UI.Client;
using static WebSharper.UI.Client.Html;

namespace CSharpRouting
{
    [JavaScript]
    public class App
    {
        // Define the possible routes
        [EndPoint("")]
        public class Root
        {
            [EndPoint("about")]
            public class About : Root;

            [EndPoint("contact")]
            public class Contact : Root;

            [EndPoint("notfound")]
            public class NotFound : Root;
        }

        // Main UI component
        public static WebSharper.UI.Doc Render(WebSharper.Sitelets.Router<Root> router, Var<Root> currentRoute) 
        {
            // Navigation bar
            var navBar =
                div(
                    attr.@class("navbar"),
                    a(attr.href(router.HashLink(new Root())), "Home"),
                    a(attr.href(router.HashLink(new Root.About())), "About"),
                    a(attr.href(router.HashLink(new Root.Contact())), "Contact")
                );

            // Page content based on current route
            var pageContent =
                currentRoute.View.Doc(route =>
                    route switch
                    {
                        Root.About _ =>
                            div(
                                h2("About Us"),
                                p("Learn more about our application."),
                                button("Go to Contact",
                                    () => currentRoute.Set(new Root.Contact())
                                )
                            ),
                        Root.Contact _ =>
                            div(
                                h2("Contact Us"),
                                p("Get in touch with us!")
                            ),
                        Root.NotFound _ =>
                            div(
                                h2("404 - Page Not Found"),
                                p("The page you requested does not exist.")
                            ),
                        Root _ =>
                            div(
                                h2("Welcome to the Home Page"),
                                p("This is the main page of our SPA.")
                            )
                    }
                );

            // Combine navigation and content
            return div(
                navBar,
                pageContent
            );
        }
        
        [SPAEntryPoint]
        public static void ClientMain()
        {
            var router = WebSharper.Sitelets.InferRouter.Router.Infer<Root>();

            // Install the router, with a fallback if no route matches
            var currentRoute =
                router.InstallHash(new Root.NotFound());

            Render(router, currentRoute)
                .RunById("main");
        }
    }

}
