# Json configuration

WebSharper 4.2 introduces the option to configure the build with a `wsconfig.json` file placed in the same directory as a project file.
This must consist of a single JSON object. Keys are all case-insensitive. Boolean values can be `true` of `false` literals or strings that can be parsed to a `bool`.

# Project Variables

Most configuration options are also settable as properties in the `.csproj`/`.fsproj` files.
For example the equivalent of `"SouceMap": true` is `<WebSharperSourceMap>True</WebSharperSourceMap>`.

These are read first, the values in `wsconfig.json` are overriding the values in the project file if the
same setting is provided in both places.

# Settings

## Project (string)

**Obligatory** (\*either this or `OutputDir` is needed)

Specifies the WebSharper project type. The valid values and their corresponding project types
are listed below.

|Project type|WebSharperProject value|
|-|-|
|Client-Server Application|`Site`,`Web`,`Website`,`Export`|
|Extension|`Extension`,`InterfaceGenerator`|
|Library|`Library`|
|HTML Application|`Html`|
|Single Page Application|`Bundle`|
|Single Page Application without .NET compilation|`BundleOnly`|

If `Project` is empty but `OutputDir` is specified
then this setting will implicitly have the value `Site`, which means a *Client-Server Application*
project type.

Project property: `WebSharperProject`

Console argument: `--ws:projecttype`

## OutputDir (string)

**Obligatory** (\*either this or `Project` is needed in cases of _web projects_)

Specifies the compilation output directory when no project type is specified.
Only needed when the project type is *Client-Server Application*.

Project property: `WebSharperOutputDir`

Console argument: `--wsoutput:folder`

## OutputDir (string)

Specifies the path of the compilation output directory relative to the project file when
the project type is *Single Page Application* or *HTML Application*.
Default folder is `/Content` for SPAs and `/bin/html` for HTML apps.

Project property: `WebSharperBundleOutputDir` and `WebSharperHtmlDirectory`

Console argument: `--wsoutput:folder`

## SourceMap (bool)

If the value is `true`, the compiler will include source maps and the required source files
in a WebSharper assembly.

*Sitelets* and *Single Page Application* projects are supported, while offline Sitelets are not. Read more
about setting up Source Mapping in
[the documentation](https://github.com/intellifactory/websharper.docs/blob/master/SourceMapping.md).

Project property: `WebSharperSourceMap`

Console argument: `--jsmap`

## WarnOnly (bool)

If the value is `true`, WebSharper compiler errors will be treated as warnings.

Project property: `WebSharperErrorsAsWarnings`

Console argument: `--wswarnonly`

## DCE (bool)

Set the value to `false` to turn off dead code elimination (it is on for Single Page Applications
by default.)

If dead code elimination and source mapping are both turned off for a `Bundle`/`BundleOnly` project,
then bundling does not rewrite the JavaScript code of referenced assemblies into one scope, just
concatenates the pre-compiled `.js` files of them. This results in faster compilation speeds for iterative testing.

Project property: `WebSharperDeadCodeElimination`

Console argument: `--dce+`/`--dce-` (default is `--dce+`)

## DownloadResources (bool)

Currently implemented for Sitelet projects. Set to `true` to have WebSharper download all 
remote `js`/`css` resources defined in the current project and all references.

You also need to add `<add key="UseDownloadedResources" value="True" />` to your `Web.config`'s `<appSettings>`
so that WebSharper inserts a link to that downloaded file in your pages instead of a link to the online resource.

Project property: `WebSharperDownloadResources`

Console argument: `--dlres`

## AnalyzeClosures (bool or string)

There is an inconvenient source of memory leaks in most JavaScript engines which is
[described here](http://point.davidglasser.net/2013/06/27/surprising-javascript-memory-leak.html).

This setting can enable warnings on these kinds of captures, helping to eliminate memory leaks.

**Possible values:**
* `True` - Turns warnings on
* `MoveToTop` - Moves all non-capturing lambdas to top level automatically (experimental)
* `False` - Default setting, no JS closure analysis.

Project property: `WebSharperAnalyzeClosures`

Console argument: `--closures:true`/`--closures:movetotop`

## JavaScript (bool or array of strings)

Setting this to `true` is equivalent to having a `JavaScript` attribute on the assembly level:
it marks the entire assembly for JavaScript compilation.
You can still exclude types by using the `JavaScript(false)` attribute.

Alternatively, you can pass an array of strings, containing file or type names. 
This is marking the given files or types for JavaScript compilation.

## JsOutput (string)

Provide a path to a file to make WebSharper write the `.js` output for the project.

## MinJsOutput (string)

Provide a path to a file make WebSharper write the `.min.js` output for the project.
