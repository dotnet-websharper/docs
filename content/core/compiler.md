---
title: Using the compiler
---

Using the WebSharper compiler programmatically involves 3 main steps:
 
* Gather the WebSharper metadata, which is an object of type `WebSharper.Core.Metadata.Info`. If you want to use WebSharper's .NET and `FSharp.Core` proxies, or have other WebSharper projects/bindings you rely on, you must load the embedded metadata from your referenced assemblies, then merge them to a single metadata object for optimized dictionary lookups and ensuring no conflicts exist.
* Create a `WebSharper.Compiler.Compilation` instance with this metadata, and populate it with types/members to translate. This can be done with different helpers depending on you are working from a checked F#, or C# project, or ReflectedDefinitions in an already compiled F# `dll`. Then call `WebSharper.Compiler.Translator.DotNetToJavaScript.CompileFull` on the `Compilation` to run the full translation. The resulting errors/warnings can be checked and handled.
* Depending on the output required, code can be modularized (WebSharper calls creating a standalone module *bundling*, and creating modules with interdependencies *packaging*) and written to JavaScript/TypeScript/TypeScript declaration file(s). To create a `dll` that is usable by other WebSharper projects, the resulting current metadata can be serialized and embedded into the `dll`.

The packages needed are `WebSharper.Compiler.Common` for core functionality, `WebSharper.Compiler.FSharp` for processing F# code/projects, and `WebSharper.Compiler.CSharp` for processing C# code/projects.

## Handling metadata and code dependency graphs

WebSharper-compiled `dll`s may have two kinds of metadata embedded in them: the metadata needed for compilation using that `dll` as a reference, and for web projects only, a runtime metadata that is optimized for the needs of the WebSharper sitelet and remoting middleware. For compilation purposes we need the non-runtime metadata.

Three options to load metadata from an assembly, all of these return an `option` value, `None` if no WebSharper metadata is present:

* Use `WebSharper.Core.Metadata.IO.LoadMetadata` to deserialize metadata from a `System.Reflection.Assembly`. This assembly must not be loaded in reflection-only mode (e.g. `Assembly.ReflectionOnlyLoad`), so it requires loading the assembly to the default `AssemblyLoadContext` or an isolated one.
* The `WebSharper.Compiler.FrontEnd.ReadFullFromFile` uses `Mono.Cecil` in the back to safely load metadata from a `dll` file path.
* If also want caching and looking up `dll`s by file names only in pre-determined locations or directories, you can create a `WebSharper.Compiler.AssemblyResolver` instance with `AssemblyResolver.Create()`. Then you can add `dll` file paths with the `SearchPaths` method, or directory paths to enumerate and add all `dll`s found with the `SearchDirectories` method. Once the `AssemblyResolver` is set up, create a `WebSharper.Compiler.Loader` with `Loader.Create`. It takes the `AssemblyResolver` and a logger function. Now you can use its `LoadFile` method to get a `WebSharper.Compiler.Assembly` encapsulation of the assembly, and it is also cached. Now the `WebSharper.FrontEnd.TryReadFromAssembly` or `ReadFromAssembly` gets the metadata from this assembly (the difference is that `TryReadFromAssembly` returns metadata load failures as a value, not an exception). These also take a `WebSharper.Core.Metadata.MetadataOptions` value, see the next section.

### Filtered metadata

The `WebSharper.Core.Metadata.Info` type has three methods for discarding expressions inside for optimizing memory size.

* `DiscardExpressions` removes all expressions, this is the default when creating runtime metadata, however for compilation this is undesirable.
* `DiscardInlineExpressions` can be used when the metadata will be used only to create new bundles, but not generating new code that could possibly use WebSharper's `[<Inline>]` annotated type members.
* `DiscardNotInlineExpressions` can be used when the metadata will be used only to compile and write new code, that will not include any new code from references, but using JS imports for them.

The `WebSharper.Core.Metadata.MetadataOptions` type can specify a filter on which of the above to apply or none, `FullMetadata` applies none, while the other three values are matching the name of the corresponding `Discard...` method they use.

### Merging metadata

When metadata are loaded from all references of a project, they must be merged into a single `WebSharper.Core.Metadata.Info` object for optimized dictionary lookups. This can be done with the `Metadata.Info.UnionWithoutDependencies` static method which takes a collection of metadata objects. The method name clarifies that the merged metadata will not contain a merge of the code dependency graph. This is because the dependency graph is only required for DCE (dead code elimination), not for any other compilation steps, so it should be done separately when neeeded, see later.

The `UnionWithoutDependencies` method can raise exceptions when conflicting members are found (for example the same member is proxied in two different references). This is a fatal error because it cannot be determined which client-side representation to use for a consistently working translation.

## Creating a compilation

For translating F# or C# source, the next step is to create a `WebSharper.Compiler.Compilation` object with the metadata passed in to the constructor. It has another optional constructor argument `hasGraph`, which is `true` by default, you can set it to `false` if you don't want to do any dead code elimination later.

Then you can populate it with `WebSharper.Compiler.FSharp.ProjectReader.TransformAssembly` or `WebSharper.Compiler.CSharp.ProjectReader.TransformAssembly` that takes an `FSharp.Compiler.Service` or `Microsoft.CodeAnalysis.CSharp` (Roslyn) representation of a checked project and extracts all code marked for WebSharper.