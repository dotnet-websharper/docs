# ExtJS WebSharper extension

[Ext JS][extjs] is a rich, industry-standard HTML5+JavaScript framework for web application
development.

## Notes on usage

This extension has been automatically generated from the official Ext JS documentation.
This can sometimes lead to insufficient typing, requiring explicit type annotations in consuming
code. The easiest way to do this is by using the `As<_>` primitive provided by WebSharper.

For the complete API, please consult the [official documentation][extjsdocs].

### Names and casing

The names of classes, including letter casing inside the `Ext` main class are the same as the JavaScript versions.
Properties and methods have their names converted to Pascal casing as is standard in .NET libraries.
In some instances, the names of properties and methods are modified for disambiguation:

* Properties having the same name as the class they are in get a "Prop" postfix.
* Methods having the same name as the class they are in get a "Do" prefix.
* Static methods have a "_" prefix.

### Configuration objects

All Sencha framework components can be created by the function `Ext.create` by passing it 
the name of the class and a configuration object, which describes the initial state of the created component. 
In the WebSharper bindings, these config classes are represented as nested classes in the `ExtCfg` class.
For example this code creates and displays an empty Ext JS window in JavaScript:

```javascript
Ext.create('Ext.window.Window', {
    title: 'Hello',
    height: 200,
    width: 400
}).show();
```

The straightforward translation to F#:

```
(Ext.Create(
    ExtCfg.window.Window(
        Title = "Hello",
        Height = 200,
        Width = 400
    )
) |> As<Ext.window.Window>).Show()
```

`ExtCfg.window.Window` is the class of the configuration objects for `Ext.window.Window`, 
you can set available config properties with F#'s conscructor with property initializers syntax.
`Ext.Create` returns an `obj` because it can create any kind of objects, so a cast is necessary.
Howewer there is a way to avoid this, a `.Create()` method is generated for every config object which
has the correct JavaScript translation for a call to `Ext.create`, and the return value is correctly typed,
so the above can be rewritten as:

```
ExtCfg.window.Window(
    Title = "Hello",
    Height = 200,
    Width = 400
).Create().Show()
```

When adding additional fields on a config object is needed, the `.With` method defined on all config
object types can help. It has two overloads, for adding a single key-value pair and for copying all the
fields from another config object.

Config object properties often can take an array of config objects or already initialized components, even mixing these two.
There is no easy way to correctly represent it in .NET, usually the type for the property gets generated as an array of 
components from the documentation, but you can use `As` and pass config objects too, without calling `.Create()` on them.

### Event setters

Sencha framework classes using `Ext.mixin.Observable` have an `On` and an `Un` method for adding and removing event handlers. 
WebSharper binding has generated methods for this for every event on a component, for example instead of `.On('click', ... )` 
you can write `.OnClick( ... )`, and `.Un('click', ... )` becomes `.UnClick( ... )`. 
If you want to pass additional arguments to `On` or `Un`, currently you can do that only using the untyped versions.

### Function arguments

For some methods having functions as arguments where the documentation details the signature of the function, 
the argument is typed in the binding. Where the type details are missing from the documentation of the method, 
the function argument is typed as `EcmaScript.Function`, and need an `As` cast to pass a typed function.

### Object arguments and return types

For some methods, the documentation details the fields expected on an object argument.
For these cases, a class nested in `ExtParam` exists for creating these objects.

### Enumerations

For some methods, the documentation details the accepted values of a string argument.
These are reflected as enum types contained in `ExtEnum` in the binding.

## Using Ext JS in a WebSharper project

Create a class inheriting WebSharper's `Web.Control` class like this, and use it in a sitelet:

```
type AppControl() =
    inherit Web.Control()

    [<JavaScript>]
    override this.Body =
        upcast Div [] |>! OnAfterRender (fun el ->
            Ext.OnReady(As (fun () -> onReady el.Body), null, New [])
        )
```

In the function `onReady renderTo`, define some container, for example an `ExtCfg.container.Viewport` with
`RenderTo = renderTo` to have a full screen container that you can use for your web application. 

### Adding CDN references

In the `WebSharper.ExtJS.Resources` namespace you can find classes defining links to js and cs
resources to Ext JS GPL versions on Sencha's CDN.
You can automatically insert these script and style links to your pages by using the `Require` attribute
on a `JavaScript` function using Ext JS, for example `[<Require(typeof<Resources.ExtAll>)>]`.

[extjs]: http://www.sencha.com/products/extjs/
[extjsdocs]: http://docs.sencha.com/extjs/
