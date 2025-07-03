---
title: Bundling and exporting
---

Bundling in WebSharper refers to three main use cases:
* SPA project types, where a single JavaScript file is generated to hold the client-side code.
* Multi-page sitelet applications in production-ready mode, where possibly multiple JavaScript files are generated, but they are optimized for one or more pages.
* Automatic web worker script generation. See [web workers](webworkers).

While exporting is the process of creating an npm package from your WebSharper library code.

<a name="spa"></a>
## Bundling in SPA projects

First, for SPA projects, the WebSharper compiler will generate a single JavaScript file that contains all the client-side code necessary for your application.
The location of this file is determined by the [outputDir](wsconfig#outputdir) setting in your `wsconfig.json` file.
You should directly link this file in your `index.html` file for your application.
This is set up by default in the SPA project template.

The code generated for the SPA project will use the function annotated with the `[<SPAEntryPoint>]` attribute as the entry point for the application.
By default, dead code elimination is enabled, meaning that only the code that is actually used in the application will be included in the final output.
You can disable this by setting `"dce": false` in your `wsconfig.json` file, but this is not recommended for production builds, only for testing.

You can also include additional JavaScript functions/types, by using the `[<JavaScriptExport>]` attribute on types or methods that you want to be included in the final output.
This is useful for interoperability with hand-written JavaScript that you might have in your project.

<a name="sitelets"></a>
## Bundling in multi-page sitelet applications

For `web` project type production-ready mode, [turn on the prebundling](wsconfig#prebundle) by setting `"prebundle": true` in your `wsconfig.json` file.
Now the WebSharper compiler can generate JavaScript bundles.
This is readable format code, possibly importing npm packages, so it needs a proper JavaScript bundler before serving.
It is recommended that this output goes to a `build` folder, and then bundling can be set up in project file like this: 

```xml
  <Target Name="ESBuildBundle" AfterTargets="WebSharperCompile">
    <Exec Command="npm install" />
    <Exec Command="node ./esbuild.config.mjs" />
  </Target>
```

where `esbuild.config.mjs` contains:

```javascript
import { existsSync, cpSync, readdirSync } from 'fs'
import { build } from 'esbuild'

if (existsSync('./build/Content/WebSharper/')) {
  cpSync('./build/Content/WebSharper/', './wwwroot/Content/WebSharper/', { recursive: true });
}

const files = readdirSync('./build/Scripts/WebSharper/$YOURPROJECTNAME$/');

files.forEach(file => {
  if (file.endsWith('.js')) {
    var options =
    {
      entryPoints: ['./build/Scripts/WebSharper/$YOURPROJECTNAME$/' + file],
      bundle: true,
      minify: true,
      format: 'iife',
      outfile: 'wwwroot/Scripts/WebSharper/' + file,
      globalName: 'wsbundle'
    };

    console.log("Bundling:", file);
    build(options);
  }
});
```

The contents of these prebundles are determined by the scoping of `Content.Page`, `Content.Bundle`, and `Content.BundleScope` functions.
These are all server-side functions, but within the expression passed to them, you can create `Web.Control` instances, or have quoted client-side code (e.g. using the `client` helper function).
The prebundle created will contain all necessary JavaScript code to run this included client-side functionality in the browser.

Example:
```fsharp
let HomePage ctx =
    Content.Page(
        Templating.Main ctx EndPoint.Home "Home" [
            div [] [client (Client.Main())] // the Client.Main() function will be included in the prebundle "home"
        ], 
        Bundle = "home" // name of the bundle
    )
```

There is always a bundle called "all" that contains all client-side code that is discovered by the compiler, to serve as a fallback when the bundle to use is badly configured.
This can happen when you use helper functions that invoke client-side content outside of a `Content.Page` functtion.
This is when you can use the `Content.Bundle` and `Content.BundleScope` functions to mark those code pieces as part of a bundle too. For example:

```fsharp
    let SidebarWidget () =
        Content.BundleScopes [| "home"; "about" |]
            (div [] [ client (Client.SidebarWidget()) ])

```

<a name="npm"></a>
## npm package export

To create a library that can be used as an npm package, set `"dce": true` on a libray project and specify an [outputDir](wsconfig#outputdir).
You might also want to set `"javascriptExport": true` to make the whole current project exported into the final output, otherwise only classes and methods marked with the `JavascriptExport` attribute will be available.

To package the output for npm set `"outputDir": "build"` and then you can add a section to your project file like this:

```xml
  <Target Name="CleanBuildDir" BeforeTargets="CoreCompile">
    <RemoveDir Directories="build" />
  </Target>
  
  <Target Name="CopyPackageJsonAndPack" AfterTargets="WebSharperCompile">
    <Copy SourceFiles="assets/package.json" DestinationFolder="build" />
    <Exec Command="npm pack" WorkingDirectory="build" />
  </Target>
```

This cleans the build folder before a new build. After a successful WebSharper build, it copies over a `package.json` file to serve as your package declaration to your WebSharper project's output folder.

WebSharper will create an `index.js` to serve as the root of the npm package, so in your `assets/package.json` file, set `"main": "index.js"`. Also take note that all static methods on static classes will be exported as top level functions, make sure to give expressive names for your functions for npm library use that does not depend on F# module name for example to disambiguate them.
