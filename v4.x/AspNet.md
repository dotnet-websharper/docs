# Using WebSharper with ASP.NET

WebSharper is self-sufficient and can run as the single component of a web application, but it can also be integrated into an existing ASP.NET application. This integration is twofold:

* Client-side WebSharper controls can be used within an ASPX page. This allows you to use C#/F#-compiled-to-JavaScript directly within an existing page, and take advantage of client-side reactive markup with WebSharper UI and easy remote calls.

* Full WebSharper [sitelets](Sitelets.md) can run alongside an ASP.NET application, sharing the same URL space, as well as the same server state, sessions, etc.

In any case, you need to add the following references to your ASP.NET web project:

* Your WebSharper project;

* [The WebSharper.CSharp NuGet package](http://www.nuget.org/packages/WebSharper.CSharp/);

* FSharp.Core. We recommend using [the NuGet package](http://www.nuget.org/packages/FSharp.Core/).

    For the latest FSharp.Core 4.3.x, you also need to add the following assembly redirection in your Web.config:

    ```xml
    <configuration>
      <runtime>
        <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
          <dependentAssembly>
            <assemblyIdentity name="FSharp.Core" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
            <!-- Watch out: FSharp.Core's 4.3.x NuGet package contains the 4.4.3.0 assembly -->
            <bindingRedirect oldVersion="0.0.0.0-4.4.3.0" newVersion="4.4.3.0" />
          </dependentAssembly>
          ...
    ```

## Integrating client-side controls in ASP.NET pages

Ordinary ASP.NET applications are heavily server-based: content is rendered on the server and sent to the client, and any client interaction typically involves a server roundtrip. This can be eased somewhat using Ajax techniques on the client, where content or data is retrieved from the server asynchronously. Such content then is integrated into the presentation layer (on the client) once it becomes available after the call.

WebSharper makes it even easier to communicate with the server by enabling clients to make RPC calls seamlessly, as easily as making a client-side call. Here is a snippet to illustrate this in both languages (using `WebSharper.UI`):

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
                    var data = await Server.GetData();
                    model.AppendMany(data);
                });
                return ul(model.View.DocSeqCached((int x) => li("Value= ", x)));
            }
        }
    }
    ```

* Define a remote method and a client-side `Web.Control` in F#:

    ``` fsharp
    namespace WebSharperProject

    open WebSharper

    module Server =
        [<Rpc>]
        let GetData () = 
            async { return [1; 2; 3] }

    type SimpleClientServerControl() =
        inherit Web.Control()

        [<JavaScript>]
        override this.Body =
            let model = ListModel.FromSeq []
            // call the server-side asynchronously, insert response items to ListModel
            async {
                let! data = Server.GetData()
                model.AppendMany(i)
            }
            |> Async.Start
            ul [] [
                model.Doc(fun i -> li [] [text ("Value = " + string i)])
            ] :> _
    ```

Here is how to integrate the above client-side control in your ASPX page.

### Integrating a client-side control in an ASPX page

The class `Web.Control` used in the above snippet inherits from `System.Web.UI.Control`, which means
that it can be included directly in an ASPX page. Here are the necessary steps:

* Add declarations for your content to `Web.config`:

    ``` xml
    <configuration>
      <system.web>
        <pages>
          <controls>
            <add tagPrefix="WebSharper" namespace="WebSharper.Web" assembly="WebSharper.Web"/>
            <add tagPrefix="ws" namespace="WebSharperProject" assembly="WebSharperProject"/>
          </controls>
          ...
    ```

    * The first entry makes the WebSharper ScriptManager available, and it should remain in `Web.Config` for all WebSharper ASPX-based applications.

    * The second entry declares that there is a `WebSharperProject` assembly with a namespace with the same name. This is what the default WebSharper ASP.NET and other ASP.NET-based Visual Studio project templates write by default. If you change the name of your WebSharper assemblies, or the namespaces they contain, you should update this entry or add further entries to cover those namespaces.

* In the `<head>` of your ASPX page (or `Site.Master` if you use one), add the WebSharper ScriptManager:

    ``` xml
    <head runat="server">
      <WebSharper:ScriptManager runat="server" />
      ...
    ```
    
    This will include all the CSS and scripts needed by the controls on your page. Make sure that the `<head>` tag is marked `runat="server"`.

* Insert the control itself in your ASPX page:

    ``` xml
    <ws:SimpleClientServerControl runat="server" />
    ```
    
    Even though it may seem that your WebSharper control runs on the server (as would be indicated by the `runat="server"` attribute in your ASPX markup), this is not the case. Instead, this server control embeds a placeholder in your host page, and the WebSharper ScriptManager control takes care of "bringing it to life" by populating it when your page loads on the client, also referencing any dependencies it may have to ensure correct behavior.

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

If you want to take full advantage of WebSharper's server-side facilities, including type-safe
URLs and composable websites, as part of an ASP.NET application, it is very easy to do so.

### In an existing ASP.NET application

In order to integrate a WebSharper Sitelet into an existing ASP.NET application, you need to
reference your Sitelet project from the ASP.NET website project, and add the following code
to your `Web.config` file:

* For IIS 7.x+:

    ``` xml
    <configuration>
      <system.webServer>
        <modules>
          <add name="WebSharper.RemotingModule"
               type="WebSharper.Web.RpcModule, WebSharper.Web"/>
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
          <add name="WebSharper.RemotingModule"
               type="WebSharper.Web.RpcModule, WebSharper.Web"/>
          <add name="WebSharper.Sitelets"
               type="WebSharper.Sitelets.HttpModule, WebSharper.Sitelets"/>
        </httpModules>
        ...
    ```

### In a new ASP.NET project

We generally advise against using WebSharper alongside ASP.NET for a new application,
and recommend instead to simply run a WebSharper Sitelets application to contain the
whole application. Nevertheless, the [Visual Studio extension](/downloads) includes a
template that facilitates creating an ASP.NET/Sitelets hybrid application. The
"Sitelets Host Website" template is an ASP.NET Web application template that includes
the facilities describe above to run alongside Sitelets. All you have to do is to
include a reference to your Sitelets project in this web project, and you're good to go!
