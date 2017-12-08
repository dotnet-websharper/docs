# Project Variables

WebSharper introduces unique project file variables in `.fsproj` files. They are used to
communicate e.g. the project type, the output directory, and optimizations to the WebSharper
compiler. These parameters are the following:

* `WebSharperProject`
* `WebProjectOutputDir`
* `WebSharperBundleOutputDir`
* `WebSharperHtmlDirectory`
* `WebSharperSourceMap`
* `WebSharperTypeScriptDeclaration`

You can set these variables by inserting e.g. `<WebSharperSourceMap>True</WebSharperSourceMap>` into a
`<PropertyGroup></PropertyGroup>` in your `.fsproj` project file.

## WebSharperProject

**Obligatory** (\*either this or `WebProjectOutputDir` is needed)

Specifies the WebSharper project type. The valid values and their corresponding project types
are listed below.

|Project type|WebSharperProject value|
|-|-|
|Client-Server Application|`Site`,`Web`,`Website`,`Export`|
|Extension|`Extension`,`InterfaceGenerator`|
|Library|`Library`|
|HTML Application|`Html`|
|Single Page Application|`Bundle`|

There's also a special value `Ignore`, which leaves the project type explicitly unset.

Please also note that if `WebSharperProject` is empty but `WebProjectOutputDir` is specified
then this variable will implicitly have the value `Site`, which means a *Client-Server Application*
project type.

## WebProjectOutputDir

**Obligatory** (\*either this or `WebSharperProject` is needed in cases of _web projects_)

Specifies the compilation output directory when no project type is specified.
Only needed when the project type is *Client-Server Application*.

## WebSharperBundleOutputDir

**Optional**

Specifies the path of the compilation output directory relative to the project file when
the project type is *Single Page Application*.

## WebSharperHtmlDirectory

**Optional**

Specifies the relative path of the compilation output directory when the project
type is *HTML Application*.

## WebSharperSourceMap

**Optional**

If the variable is set to `True`, the compiler will include source maps and the required source files
in a WebSharper assembly.

*Sitelets* and *Single Page Application* projects are supported, while offline Sitelets are not. Read more
about setting up Source Mapping in
[the documentation](https://github.com/intellifactory/websharper.docs/blob/master/SourceMapping.md).

## WebSharperTypeScriptDeclaration

**Optional**

If the variable is set to `True`, turns on TypeScript definition file output which
allows TypeScript users to link against WebSharper-generated code.

**Note:** This feature is currently not available in WebSharper 4.0. The value of the project
variable will be ignored.
