# Using WebSharper with ASP.NET MVC

WebSharper is self-sufficient and can run as the single component of a web application, but it can also be integrated into an existing ASP.NET MVC application. This integration is twofold:

* Client-side WebSharper controls can be used within a Razor page. This allows you to use C#/F#-compiled-to-JavaScript directly within an existing page, and take advantage of client-side reactive markup with WebSharper UI and easy remote calls.

* Full WebSharper [sitelets](Sitelets.md) can run alongside an ASP.NET application, sharing the same URL space, as well as the same server state, sessions, etc.

In any case, you need to add the following references to your ASP.NET web project:

* Your WebSharper project;

* [The WebSharper.CSharp NuGet package](http://www.nuget.org/packages/WebSharper.CSharp/);

* [The WebSharper.AspNetMvc package](http://www.nuget.org/packages/WebSharper.AspNetMvc).

* FSharp.Core. We recommend using [the NuGet package](http://www.nuget.org/packages/FSharp.Core/).

    For the latest FSharp.Core 4.3.x, you also need to add the following assembly redirection in your Web.config:

    ```xml
    <configuration>
      <runtime>
        <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
          <dependentAssembly>
            <assemblyIdentity name="FSharp.Core" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
            <bindingRedirect oldVersion="0.0.0.0-4.4.3.0" newVersion="4.4.3.0" />
          </dependentAssembly>
          ...
    ```

## Integrating client-side controls in ASP.NET pages

Ordinary ASP.NET applications are heavily server-based: content is rendered on the server and sent to the client, and any client interaction typically involves a server roundtrip. This can be eased somewhat using Ajax techniques on the client, where content or data is retrieved from the server asynchronously. Such content then is integrated into the presentation layer (on the client) once it becomes available after the call.

WebSharper makes it even easier to communicate with the server by enabling clients to make RPC calls seamlessly, as easily as making a client-side call. Here is a snippet to illustrate this in both languages:

* Define a remote method and a client-side `Web.Control` in C#:

    ```csharp
    public static class Server
    {
        [Remote]
        public static Task<int[]> GetData()
        {
            return Task.FromResult(new[] { 1, 2, 3});
        }
    }

    [Serializable]
    public class SimpleClientServerControl : WebSharper.Web.Control
    {
        [JavaScript]
        public override IControlBody Body
        {
            get
            {
                var model = new ListModel<int, int>(x => x);
                // call the server-side asynchronously, insert response items to ListModel
                Task.Run(async () =>
                {
                    foreach (var n in await Server.GetData())
                        model.Add(n);
                });
                return ul(model.View.DocSeqCached((int x) => li("Value= ", x)));
            }
        }
    }
    ```

* Define a remote method and a client-side `Web.Control` in F#:

    ```fsharp
    namespace WebSharperProject

    open WebSharper

    module Server =
        [<Rpc>]
        let GetData () = async { return [1; 2; 3] }

    module Client =
        open WebSharper.Html.Client

        [<JavaScript>]
        let Main () =
            let ul = UL
            // call the server-side asynchronously, insert response items to DOM
            async {
                let! data = Server.GetData ()
                for i in data do
                    ul.Append(LI [Text ("Value= " + i)])
            } |> Async.Start
            ul

    type SimpleClientServerControl() =
        inherit Web.Control()

        [<JavaScript>]
        override this.Body = Client.Main () :> _
    ```

Here is how to integrate the above client-side control in your Razor page.

### Integrating client-side controls in Razor pages

Razor integration is provided by WebSharper.AspNetMvc. Here are the necessary steps:

* In your main razor layout, inside the head tag, add the following:

    ```csharp
    @WebSharper.AspNetMvc.ScriptManager.Head()
    ```

    This will include all the CSS and scripts needed by the controls on your page.

* To insert a control in a view, first create it at the top of the view:

    ```csharp
    @{
        var myControl = WebSharper.AspNetMvc.ScriptManager.Register(new MyControl());
    }
    ```

    and then you can insert it in the view, eg:

    ```xml
    <div>
        <h1>My control:</h1>
        @myControl
    </div>
    ```

    It is important to create the control before the view starts rendering, or else `ScriptManager.Head()` will already have been rendered without this control's dependencies.
    
    Here is an example resulting full page:
    
    ```xml
    @{
        var myControl = WebSharper.AspNetMvc.ScriptManager.Register(new MyControl());
    }
    <!DOCTYPE html>
    <html>
      <head>
        <title>My sample Razor page</title>
        @WebSharper.AspNetMvc.ScriptManager.Head()
      </head>
      <body>
        <div>
          <h1>My control:</h1>
          @myControl
        </div>
      </body>
    </html>
    ```

### Using remoting

To enable WebSharper remoting, `Web.Config` must contain the declaration for the WebSharper HTTP module. This module is responsible for all RPC communication and comes already configured in the WebSharper Visual Studio project templates for ASP.NET. You should see something like this:

* For IIS 7.x+:

    ``` xml
    <configuration>
      <system.webServer>
        <modules>
          <add name="WebSharper.RemotingModule"
               type="WebSharper.Web.RpcModule, WebSharper.Web"/>
        </modules>
        ...
    ```

* For IIS 6.x:

    ``` xml
    <configuration>
      <system.web>
        <httpModules>
          <add name="WebSharper.RemotingModule"
               type="WebSharper.Web.RpcModule, WebSharper.Web"/>
        </httpModules>
        ...
    ```

## Running Sitelets alongside ASP.NET

If you want to take full advantage of WebSharper's server-side facilities, including type-safe URLs and composable websites, as part of an ASP.NET MVC application, it is very easy to do so.

In order to integrate a WebSharper Sitelet into an existing ASP.NET MVC application, you need to reference your Sitelet project from the ASP.NET website project, and add the following code to your `Web.config` file:

* For IIS 7.x+:

    ``` xml
    <configuration>
      <system.webServer>
        <modules>
          <add name="WebSharper.Sitelets"
               type="WebSharper.Sitelets.HttpModule, WebSharper.Sitelets"/>
        </modules>
        ...
    ```

* For IIS 6.x:

    ``` xml
    <configuration>
      <system.web>
        <httpModules>
          <add name="WebSharper.Sitelets"
               type="WebSharper.Sitelets.HttpModule, WebSharper.Sitelets"/>
        </httpModules>
        ...
    ```

You can specify in your application startup which of ASP.NET MVC or WebSharper Sitelets' routing takes precedence in case their URL space overlaps. By default, WebSharper takes precedence.

```csharp
public class MyMvcApplication : System.Web.HttpApplication
{
    protected void Application_Start()
    {
        WebSharper.Sitelets.HttpModule.OverrideHandler = false;
    }
}
```
