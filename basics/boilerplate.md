---
order: -40
label: "ASP.NET Core boilerplate"
---
# Creating a new WebSharper project from scratch

If you don't have the [WebSharper project templates](/basics/templates.md) installed and need a basic, server-only setup to experiment with (the equivalent of the `websharper-min` project template), you can create it as follows:

1. Create an empty F\# ASP.NET Core project

    ```text
    dotnet new web -lang f# -n HelloWorld
    ```

2. Add the `WebSharper` and `WebSharper.AspNetCore` packages to it:

    ```text
    dotnet add package WebSharper
    dotnet add package WebSharper.AspNetCore
    ```

3. Add `Site.fs` to the project with the following content:

    ```fsharp
    module Site

    open WebSharper
    open WebSharper.Sitelets

    [<Website>]
    let Main = Application.Text (fun ctx -> "Hello World!")
    ```

4. Modify `Startup.fs` as follows:

    ```fsharp
    ...
    open WebSharper.AspNetCore

    type Startup() =

        member this.ConfigureServices(services: IServiceCollection) =
            services.AddSitelet(Site.Main)
                .AddAuthentication("WebSharper")
                .AddCookie("WebSharper", fun options -> ())
            |> ignore

        member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
            if env.IsDevelopment() then app.UseDeveloperExceptionPage() |> ignore

            app.UseAuthentication()
                .UseStaticFiles()
                .UseWebSharper()
                .Run(fun context ->
                    context.Response.StatusCode <- 404
                    context.Response.WriteAsync("Page not found"))
    ```

5. Add a WebSharper configuration file for future use:

    ```text
    {
      "$schema": "https://websharper.com/wsconfig.schema.json",
      "project": "site",
      "outputDir": "wwwroot"
    }
    ```

6. Run the app with `dotnet run`.
