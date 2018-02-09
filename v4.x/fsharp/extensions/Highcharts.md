# HighCharts WebSharper extension

[Highcharts][hc] is a JavaScript charting library, which can produce
interactive HTML5 charts from a config object.
Highstock is an extension of Highcharts adding more chart types geared for stock price visualization.
Highmaps creates interactive map charts with drilldown feature.

The WebSharper bindings for Highcharts provide a strongly typed interface to
to the configuration object and helper functions. These are automatically
generated from the official [API documentation][hcapi], see the full list
of settings there. Similar API documentation for [Highstock][hsapi] and 
[Highmaps][hmapi] is also available.

The simplest way to define a chart with HighCharts in a WebSharper
`Web.Control` class:


```
Div [] |>! OnAfterRender (fun el ->
    Highcharts.Create(JQuery.Of el.Body,
        HighchartsCfg(
            // config properties
        )
    )
```

Sometimes a config property can accept multiple types, for example an array instead of a config object. In these cases, use WebSharper's `As` helper
function to cast it to the required type. As this cast is erased from the
resulting JavaScript code, the library works as intended.

## Resources

If you want to use Highcharts only, use this line on your `JavaScript` annotated code or `Web.Config` class. 

```
open IntelliFactory.WebSharper.Highcharts

[<Require(typeof<Resources.Highcharts>)>]
```

Highstock contains Highcharts, so just add `Resources.Highstock` instead and this will
enable using the Highcharts functions too.

Highmaps is available as an extension to either Highcharts or Highstock.
Use one of these lines to get the correct extension module:

```
[<Require(typeof<Resources.MapModuleForCharts>)>]
[<Require(typeof<Resources.MapModuleForStock>)>]
```

[hc]: http://www.highcharts.com/
[hcapi]: http://api.highcharts.com/highcharts
[hsapi]: http://api.highcharts.com/highstock
[hmapi]: http://api.highcharts.com/highmaps
