# Compiler settings

WebSharper's compiler can be customized by a number of settings. There are two ways to provide these settings: either in the `.csproj` / `.fsproj` project file, or (since version 4.2) in a `wsconfig.json` located next to the project file. The recommended way is to use `wsconfig.json`.

## Json configuration

WebSharper 4.2 introduces the option to configure the build with a `wsconfig.json` file placed in the same directory as a project file.
This must consist of a single JSON object. Keys are all case-insensitive. Boolean values can be `true` of `false` literals or strings that can be parsed to a `bool`. Here is an example `wsconfig.json`:

```json
{
  "project": "site",
  "outputDir": "wwwroot",
  "sourceMap": true,
  "downloadResources": true
}
```

Optionally, to get tooltips from supporting editors, you can add the following [JSON Schema](http://json-schema.org/) declaration:

```json
{
  "$schema": "https://websharper.com/wsconfig.schema.json",
  // ...
}
```

The [Ionide](http://ionide.io/) extension for Visual Studio Code includes the schema, and provides tooltips even without the above declaration.

## Project Variables

Most configuration options are also settable as properties in the `.csproj`/`.fsproj` files.
For example the equivalent of `"souceMap": true` is `<WebSharperSourceMap>True</WebSharperSourceMap>`.
These must be set inside a `<PropertyGroup>` element.

If a setting is provided both in the project file and in `wsconfig.json`, the value in `wsconfig.json` takes precedence.

# Available Settings

<a name="project"></a>
## Project

**Obligatory** (\*either this or [OutputDir](#outputDir) is needed)

**Type**: string (see below)

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

If Project is empty but [OutputDir](#outputDir) is specified
then this setting will implicitly have the value `Site`, which means a *Client-Server Application*
project type.

**wsconfig.json file syntax**: 

```json
  "project": "value"
```

**.\*proj file syntax**:

```xml
<WebSharperProject>value</WebSharperProject>
```

Console argument: `--ws:value`

<a name="outputDir"></a>
## OutputDir

**Obligatory** if [Project](#project) is unspecified or equal to `Site`; optional otherwise.

**Type**: relative or absolute folder path

Specifies the path of the compilation output directory relative to the project file when
the project type is *Client-Server Application*, *Single Page Application* or *HTML Application*.
Default folder is `./Content` for SPAs and `./bin/html` for HTML apps.

**wsconfig.json file syntax**: 

```json
  "outputDir": "some/folder"
```

**.\*proj file syntax**:

```xml
<!-- If Project is Html: -->
<WebSharperHtmlDirectory>some/folder</WebSharperHtmlDirectory>
<!-- If Project is Bundle: -->
<WebSharperBundleOutputDir>some/folder</WebSharperBundleOutputDir>
<!-- If Project is Site: -->
<WebProjectOutputDir>some/folder</WebProjectOutputDir>
```

Console argument: `--wsoutput:some/folder`

## SourceMap

**Type**: bool (default is `false`)

If the value is `true`, the compiler will include source maps and the required source files
in a WebSharper assembly.

*Sitelets* and *Single Page Application* projects are supported, while offline Sitelets are not. Read more
about setting up Source Mapping in
[the documentation](https://github.com/intellifactory/websharper.docs/blob/master/SourceMapping.md).

**wsconfig.json file syntax**:

```json
  "sourceMap": false
```

**.\*proj file syntax**:

```xml
<WebSharperSourceMap>false</WebSharperSourceMap>
```

Console argument: `--jsmap`

## WarnOnly

**Type**: bool (default is `false`)

If the value is `true`, WebSharper compiler errors will be treated as warnings.

**wsconfig.json file syntax**:

```json
  "warnOnly": false
```

**.\*proj file syntax**:

```xml
<WebSharperErrorsAsWarnings>false</WebSharperErrorsAsWarnings>
```

Console argument: `--wswarnonly`

## DCE

**Type**: bool (default is `true`, only affects Bundle/BundleOnly projects)

Set the value to `false` to turn off dead code elimination.

If dead code elimination and source mapping are both turned off for a `Bundle`/`BundleOnly` project,
then bundling does not rewrite the JavaScript code of referenced assemblies into one scope, just
concatenates the pre-compiled `.js` files of them. This results in faster compilation speeds for iterative testing.

**wsconfig.json file syntax**:

```json
  "dce": false
```

**.\*proj file syntax**:

```xml
<WebSharperDeadCodeElimintation>false</WebSharperDeadCodeElimintation>
```

Console argument: `--dce+`/`--dce-` (default is `--dce+`)

## DownloadResources

**Type**: bool (default is `false`)

Currently implemented for Sitelet projects. Set to `true` to have WebSharper download all 
remote `js`/`css` resources defined in the current project and all references.

You also need to add `<add key="UseDownloadedResources" value="True" />` to your `Web.config`'s `<appSettings>`
so that WebSharper inserts a link to that downloaded file in your pages instead of a link to the online resource.

**wsconfig.json file syntax**:

```json
  "downloadResources": false
```

**.\*proj file syntax**:

```xml
<WebSharperDownloadResources>false</WebSharperDownloadResources>
```

Console argument: `--dlres`

## AnalyzeClosures

**Type**: bool or `"MoveToTop"` (default is `false`)

There is an inconvenient source of memory leaks in most JavaScript engines which is
[described here](http://point.davidglasser.net/2013/06/27/surprising-javascript-memory-leak.html).

This setting can enable warnings on these kinds of captures, helping to eliminate memory leaks.

**Possible values:**
* `True` - Turns warnings on
* `MoveToTop` - Moves all non-capturing lambdas to top level automatically (experimental)
* `False` - Default setting, no JS closure analysis.

**wsconfig.json file syntax**:

```json
  "analyzeClosures": false
```

**.\*proj file syntax**:

```xml
<WebSharperAnalyzeClosures>false</WebSharperAnalyzeClosures>
```

Console argument: `--closures:true`/`--closures:movetotop`

## JavaScript

**Type**: bool or array of strings (default is `false`)

Setting this to `true` is equivalent to having a `JavaScript` attribute on the assembly level:
it marks the entire assembly for JavaScript compilation.
You can still exclude types by using the `JavaScript(false)` attribute.

Alternatively, you can pass an array of strings, containing file or type names. 
This is marking the given files or types for JavaScript compilation.

**wsconfig.json file syntax**:

```json
  "javascript": false
```

**.\*proj file syntax**: (none)

## JsOutput

**Type**: string (relative or absolute file path)

Provide a path to a file to make WebSharper write the `.js` output for the project.

**wsconfig.json file syntax**:

```json
  "jsOutput": "some/file.js"
```

**.\*proj file syntax**: (none)

## MinJsOutput

**Type**: string (relative or absolute file path)

Provide a path to a file make WebSharper write the `.min.js` output for the project.

**wsconfig.json file syntax**:

```json
  "jsOutput": "some/file.min.js"
```

**.\*proj file syntax**: (none)
