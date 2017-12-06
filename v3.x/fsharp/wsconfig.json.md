# wsconfig.json

The file `wsconfig.json` parameterizes the WebSharper compilation. Here is an
example file:

```js
{
    "project": "MyProject.fsproj",
    "type": "website",
    "output-dir": "wwwroot",
    "source-map": true
}
```

## Parameters

Here are the parameters that you can configure with `wsconfig.json`:

* **`"project"`** is a string which defines what is the project file to
  compile. It can be either an MSBuild project file (`*.fsproj`, `*.csproj`) or
  an F# script file (`*.fsx`).
  
  Default value: WebSharper looks for one of these in the same directory as
  `wsconfig.json`, in order:
  
    * A single `*.fsproj` or `*.csproj` file;
    * A single `*.fsx` file.
    
  If none of these are found, the `project` must be provided.

* **`"type"`** is a string which defines what kind of compilation WebSharper
  performs. It can have one of the following values:

    * `"ts"`: WebSharper just compiles the project to a TypeScript module. 

      This is the default if the `project` is a `*.fsx` file.
      
      This is the recommended type for `webpack`-enabled applications.

    * `"library"`: WebSharper compiles the project to a .NET assembly *and* to
      TypeScript, and embeds the TypeScript information into the assembly. It
      will then be used by other WebSharper projects referencing this assembly.

      This is the default if the `project` is a `*.*proj` file.
      
      This is the recommended type for libraries that will be referenced from
      other website projects.
      
    * `"bundle"`: WebSharper compiles the project to a single JavaScript file
      and a single CSS file which also embed all of the dependent code.
      
      This is the recommended type for single-page applications.
      
    * `"website"`: Identical to `"library"`, but WebSharper also extracts the
      TypeScript files and embedded resources for this project and all its
      dependencies.
      
      This is the recommended type for client-server applications.
      
* **`"output"`** is a string which defines in which folder WebSharper writes
  its output. Its exact meaning depends on the `type`:
  
    * `"type": "ts"`: The compiled TypeScript file is written into the `output`
      directory. The compiled file's name is the `project` file's name with its
      extension replaced with `.ts`.
      
      If `output` ends with `.ts`, then it is the compiled TypeScript file's
      full name instead of just its directory.
      
    * `"type": "library"`: `output` is ignored. The .NET assembly's location is 

## Legacy mode: MSBuild parameters

Until version 4.0, WebSharper was configured using MSBuild parameters. These
are still supported, but with a warning during compilation, and it is
recommended to use `wsconfig.json` instead. Here are the parameter names
together with their json equivalent:

| MSBuild parameter                 | JSON equivalent      |
|-----------------------------------|----------------------|
| `WebSharperProject`               | `type`               |
| `WebProjectOutputDir`             | `output`             |
| `WebSharperBundleOutputDir`       | `output`             |
| `WebSharperHtmlDirectory`         | `output`             |
| `WebSharperSourceMap`             | `source-map`         |
| `WebSharperTypeScriptDeclaration` | N/A since we now output TypeScript |
| `WebSharperErrorsAsWarnings`      | `warn-only`          |
| `WebSharperDeadCodeElimination`   | `eliminate-code`     |
| `WebSharperDownloadResources`     | `download-resources` |
| `WebSharperAnalyzeClosures`       | `analyze-closures`   |
