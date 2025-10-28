---
title: Code generation and output merging
---

## F# code generator

With WebSharper 10+, the F# compiler provides a built-in way to generate F# source files from some input files other than `.fs`, at the very beginning of the compilation process.
These generated files are then saved in a `.props` file to make them automatically appear in project trees of VS/VSCode.

To create a source generator, you need to define a class that implements `WebSharper.ISourceGenerator`, and then register that type and tie it to a file extension it will act upon by using `[<assembly: WebSharper.FSharpSourceGenerator("my", typeof<MyGenerator>)>]` where `"my"` is the file extension you want to handle and `MyGenerator` is your generator class.

The `ISourceGenerator` interface has a single `Generate` method to implement that gets a `GenerateCall` input record which provides context for the currently executing source generation call. The properties of this record are:

* `RelativeFilePath`: Path to the file the generator is invoked for as it is included in the project file.
* `FilePath`: Full path to the file the generator is invoked for.
* `ProjectFilePath`: Project file full path.
* `Print`: Print to the standard output with support to the WebSharper background build service.
* `PrintError`: Print to the error output with support to the WebSharper background build service.
* `PreviousOutputFiles`: The previously generated files, with the timestamp of the last compilation, if available.

The `Generate` method must return an `option<string[]>`. `None` signifies that no update is necessary, use this only if `PreviousOutputFiles` has a `Some` value and the files are checked to be present and up to date. Return a `Some` value with an array of file paths otherwise. The generator implementation must take care of writing the generated files to disk so it should returns the file locations only.

### Output merging

This is an experimental feature in WebSharper 10+ that allows making manual modification in WebSharper output files, which will be retained while you re-run the build.

If the `"changeTracking"` setting in `wsconfig.json` is set to `true`, then WebSharper creates a `.websharper` folder in project root with 3 subfolders:

* `baseline` to contain the clean output of last build.
* `modified` is used as a temporary folder to hold the previous contents of the output folder, which can contain manual modifications, while the compiler is running.
* `conflict` is used as a temporary folder if 3-way merging has conflicts that has to be resolved before continuing.

In the simplest scenario, the process is fully automatic: you can modify output files right in your configured `outputDir` directory, and when you re-run the WebSharper compiler, your modifications are kept merged with the changes coming from the F#/C# source code.

In case of merge conflicts, you should review the merge conflicts in the `conflict` folder. Once they are resolved, you can re-run the build which will automatically apply the conflict resolution and finish the merge.

If you want to abort and make changes to your F#/C# code first, then delete the `modified` and `conflict` folders.

Warning: if you remove the `baseline` folder, then your manual changes will be lost with the next build as there is no baseline to make the 3-way merging with.