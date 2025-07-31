---
title: Using the console tooling
---

`dotnet ws` is a .NET tool for WebSharper. You can install it with (remove the `-g` option to install locally):

```
dotnet tool install -g dotnet-ws
```

# Usage

Built-in help:

* `dotnet ws --help` displays list of available commands and options.
* `dotnet ws <command> --help` displays help for a specific command.

Commands and options:

* `dotnet ws build [--project/-p <PROJ>]` builds the WebSharper project in the current folder (or in the nearest parent folder if project is unspecified). If the project hasn't been build with MSBuild (`dotnet build`) recently, it will call into that first. If there is cached information about the project, it will only run the WebSharper build step. Be sure to run any `npm` build steps after if you use a bundler.
* `dotnet ws start --rid/-r <RID> [--version/-v <VER>]` Start the Booster service (wsfscservice) with the given runtime identifier and Booster version. The valid RIDs are `win-x64`, `linux-x64`, and `linux-musl-x64`. If no version is specified, the latest version found in the local NuGet cache will be used.
* `dotnet ws stop [--version/-v <VER>] [--force/-f]` Send a stop signal to the Booster service with the given version. If no version is specified, all running instances are signaled. Use `--force` to kill the process(es) instead of sending a stop signal.
* `dotnet ws info` displays the version of the WebSharper CLI tool.
* `dotnet ws list` lists running Booster versions and their source paths.
* `dotnet ws watch [--pattern/-p <PAT>]` initiates a file watcher for an `fs`/`fsx` file, or a files matched by the given pattern. The watcher will automatically recompile the file(s) when they change, and will also start the Booster service if it is not already running.
* `dotnet ws compile <options> input` compiles a single `fs`/`fsx` file. The following options are available:
  * `--output/-o <PATH>` Specify the name of the output file. Also forces the compilation to use BundleOnly mode (no `.dll` generated, single bundled output file).
  * `--version/-v <VER>` Sperfy the WebSharper version to use for compilation. If not specified, the latest version found in the local NuGet cache will be used.
  * `--standalone/-s` The Booster service will not be used for compilation.
  * `--path/-p <PATH>` Specify the output location for library mode.
  * `--typescript/-ts` Generate TypeScript output (experimenal, no full support yet).
  * `--debug/-d` Use Debug configuration for compilation.
  * `--watch/-w` Starts a file watcher for the input file. The watcher will automatically recompile the file when it changes.
  * `--force` Forces the temporary folder to be cleaned before the compilation.
