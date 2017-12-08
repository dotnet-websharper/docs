# Sencha Touch WebSharper extension

[Sencha Touch][touch] is a leading HTML5+JavaScript framework for mobile application development.
This matching WebSharper extension brings this powerful library in a type safe manner to F#, enabling
developers to author awesome mobile web applications in pure F# code.

## Notes on usage

This extension has been automatically generated from the official Sencha Touch documentation.
This can sometimes lead to insufficient typing, requiring explicit type annotations in consuming
code. The easiest way to do this is by using the `As<_>` primitive provided by WebSharper.

For the complete API, please consult the [official documentation][touchdocs].

Using the WebSharper extensions for Sencha Touch and Ext JS share the same principles
and guidelines, outlined in the [WebSharper Extensions for Ext JS][extjsextension] page.

In addition to that document, the following considerations need your attention:

### Changes in API names and casing

* `Ext.version` property is renamed to `Ext.ExtVersion` to avoid naming conflict.

### Configuration objects

All Sencha framework components can be created by the function `Ext.create` by passing it 
the name of the class and a configuration object, which describes the initial state of the created component. 
In the WebSharper bindings, these config classes are represented as nested classes in the `ExtCfg` class.
For example this code creates and displays an empty Sencha Touch container in JavaScript:

```javascript
Ext.create('Ext.Container', {
    layout: fit,
    fullscreen: true
}).show();
```

The straightforward translation to F#:

```
(Ext.Create(
    ExtCfg.Container(
        Layout = "fit",
        Fullscreen = true
    )
) |> As<Ext.Container>).Show()
```

`ExtCfg.window.Window` is the class of the configuration objects for `Ext.Container`,
you can set available config properties with F#'s conscructor with property initializers syntax.
`Ext.Create` returns an `obj` because it can create any kind of objects, so a cast is necessary.
Howewer there is a way to avoid this, a `.Create()` method is generated for every config object which
has the correct JavaScript translation for a call to `Ext.create`, and the return value is correctly typed,
so the above can be rewritten as:

```
ExtCfg.Container(
    Layout = "fit",
    Fullscreen = true
).Create().Show()
```

When adding additional fields on a config object is needed, the `.With` method defined on all config
object types can help. It has two overloads, for adding a single key-value pair and for copying all the
fields from another config object.

Config object properties often can take an array of config objects or already initialized components, even mixing these two.
There is no easy way to correctly represent it in .NET, usually the type for the property gets generated as an array of 
components from the documentation, but you can use `As` and pass config objects too, without calling `.Create()` on them.

## Using Sencha Touch in a WebSharper project

Use a class inheriting WebSharper's `Web.Control` class like this, and use it in a sitelet:

```
type AppControl() =
    inherit Web.Control()

    [<JavaScript>]
    override this.Body =
        Ext.Application(ExtParam.ApplicationConfig(Launch = As onLaunch))
        upcast Div []
```

In the function `onLaunch`, create a container, for example an `ExtCfg.Container` with
`FullScreen = true` to have a full screen container that you can use for your mobile application.

### Adding themes

The `senchatouch-all.js` file is automatically included in the HTML+JavaScript package created with the Sencha Touch binding.
You have to specify which css (theme) you want to use by using a `Require` attribute,
for example `[<Require(typeof<Resources.SenchaTouchCss>)>]`.

[touch]: http://www.sencha.com/products/touch/
[touchdocs]: http://docs.sencha.com/touch/
[extjsextension]: /extensions/ExtJS.md
