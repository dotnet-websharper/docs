# Type provider for Sencha Architect projects

Create awesome user interfaces in [Sencha Architect][architect] and use them in your WebSharper applications in a type-safe and robust manner.
True WYSIWYG for your web and mobile applications, while keeping ALL of your application logic in F#.

(**Note**: separate licensing for Sencha Tools is required.  Please consult the [Sencha][sencha] website for more details.)

## Key benefits

 * **View and model always in sync**: change your UI code in Sencha Architect, and see what further changes you need to make
to keep your application consistent and semantically correct.
 * **Build all or incrementally**: Add UI controls in your Sencha Architect project, and
access them in your F# code with full code completion.
 * **Seamless server calls**: Write your backend in F# and call it from your Sencha UI - all in F#.
 * **Unprecedented efficiency**: your UI and your application code has never been closer, UI updates hit your F# code instantly.

## Using Sencha Architect in WebSharper projects

### Quick steps to get started

 * Create a WebSharper Sitelet Website project.
In `Main.fs`, remove the `About` action, page and sitelet definition.
Replace `Site.HomePage` inserted content with only `Div [new Controls.EntryPoint()]`.

 * Create new Ext JS project in Sencha Architect, set an `appFolder` value, save and publish it to your website project folder.

 * Add a script reference to your `app.js` in the `Main.html` template after the WebSharper scripts placeholder `meta` tag:

    ```
    <script type="text/javascript" src="app.js"></script>
    ```

 * Install the `WebSharper.ExtJS` NuGet package (requires a WebSharper license).
In `Client.fs`, add these attributes on the `Client` module to tell WebSharper to automatically include the basic Ext JS CSS files:

    ```
    open WebSharper.ExtJS

    [<Require(typeof<Resources.ExtAll>)>]
    [<Require(typeof<Resources.ExtThemeNeptune>)>]
    [<Require(typeof<Resources.ExtAllNeptuneCss>)>]
    ```

 * In the `Client` module, replace the `Main` function's body with the following code, where `InitMainView`, a function to be added
before `Main`, will contain your UI logic.

    ```
        Div [] |>! OnAfterRender (fun el ->
            Ext.OnReady(As InitMainView, null, null)
        )
    ```

### Raw access to UI components

In `InitMainView`, you can now access components in your UI design, set event handlers, or add other application logic using
raw, string-based accessors:

    let view = Ext.GetCmp("MainView") :?> Ext.container.Viewport
    let form = view.GetComponent("form") :?> Ext.form.Panel
    ...

However, working with these string accessors is a hassle and also very error-prone.

### Type-safe access to UI components:

Instead, you can access components from your Sencha Architect project in a robust way using the
type provider from WebSharper Extensions for Sencha Architect:

 1. Install the `WebSharper.SenchaArchitect` NuGet package into your WebSharper project.

 2. Add the line `type App = SenchaArchitect.App<"/app.js">` where you specify a correct relative or absoute path
to your Sencha Architect output `app.js` file. (be sure to have opened the `WebSharper` namespace beforehand.)

This brings all your components from your Sencha Architect UI project into F# scope. You can now do:

    let view = App.View.MainView
    ...

## Features

1. Access components having their `id` config set as static properties on their respective `View` wrapper types.
2. Access components having their `itemId` config set as instance properties on the wrapper object of the parent component.
Items can be in the `items`, `dockedItems` or `columns` configs, the `menu` of a split button or the `tools` config of a panel.
3. `Ext.form.Panel` wrappers have helper functions for getting and setting their named field values.
A `FieldValues` nested type is provided for this.
4. A wrapper type gets provided only for components having items or named fields.
Use the `self` property to get the `Ext` type for the component.
Another option is to pass the `Inherit = true` argument to the type provider.
In this case the wrapper objects inherit their framework type.  
5. Accessor for stores using `Ext.getStore` by `storeId` as static properties on the `Store` provided type.
6. Provided types for models.
The types nested in `RawModel` are creating raw models (plain JS objects), while `Model` contains wrappers around the
`Ext.data.Model` objects with typed getters/setters for the fields of the model object.
Convert between them with the `.ToModel()` and `.GetData()` helper methods.

## Additional tips

If you want to create a sitelet-based website with different Ext JS viewports loaded on the pages, use the
"Unmark as Initial View" option in Sencha Architect.
Now, running `app.js` does not create a view automatically.
You can create a full screen view by passing the element you are inserting with a `Web.Control` class as
a `RenderTo` config to the constructor of the wrapper type of the view.

    let MainView renderTo = 
        let view = App.View.MainView(ExtCfg.container.Viewport(RenderTo = renderTo))
        // ...

    let Main () =
        Div [] |>! OnAfterRender (fun el ->
            Ext.OnReady(()fun () -> MainView el.Body), null, null)
        )

[architect]: http://www.sencha.com/products/architect/
[sencha]: http://www.sencha.com
