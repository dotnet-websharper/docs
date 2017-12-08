# KendoUI WebSharper extension

[Kendo UI][kendoui] is a rich, industry-standard HTML5+JavaScript framework for web application
development.

## Notes on usage

This extension has been automatically generated from the official KendoUI TypeScript description.
This can sometimes lead to insufficient typing, requiring explicit type annotations in consuming
code. The easiest way to do this is by using the `As<_>` primitive provided by WebSharper.

UI widgets can be initialized by passing the DOM element and optionally a configuration object to
the static `.Create` method of the widget type.   

For the complete API, please consult the [official documentation][kendouidocs].

[kendoui]: http://www.telerik.com/kendo-ui
[kendouidocs]: http://docs.telerik.com/kendo-ui/api
