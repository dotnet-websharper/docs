---
order: -48
label: WebSharper CLI
icon: checklist-24
---
# WebSharper CLI - `dotnet ws`

=== Where is it?

* :icon-mark-github: Sources: https://github.com/dotnet-websharper/ws-tool
* NuGet: [![](https://img.shields.io/nuget/v/dotnet-ws?label=dotnet-ws&style=for-the-badge)](https://nuget.org/packages/dotnet-ws)

===

The WebSharper CLI, a recent addition to the WebSharper toolset, aims to streamline common WebSharper tasks/chores with a simple command-line interface via a .NET tool called `dotnet-ws`. Currently, this covers the following:

* Managing [WebSharper Booster](/booster/README.md) runtimes/instances. For each version of WebSharper you use in your projects, by default, a Booster instance will be allocated to enhance the compiler's performance. Sometimes, you might need to interact with these instances, such as listing, starting or stopping them.
* Building WebSharper projects with no configuration changes. Most of the time you recompile, the compilation context (references, target runtime, etc.) are unchanged and going with the ordinary `dotnet build` performs unnecessary steps. In these cases, and in certain project types, you can use the WebSharper CLI to compile your projects faster. See the [compilation speed](/about/compilation-speed.md) page for more details.

---

## Installation

You can install the WebSharper CLI via `dotnet install`:

```text
dotnet tool install -g dotnet-ws
```

Specifying `-g` will make the tool available globally on your machine, and this is recommended. Once you have it installed, you can invoke it with `dotnet ws`.

---

## Available commands

```text
$ dotnet ws
dotnet-ws is a dotnet tool for WebSharper.

SUBCOMMANDS:

    build <options>       Build the WebSharper project in the current folder (or in the nearest parent folder). You can
                          optionally specify a project file using `--project`.
    start <options>       Start the Booster service (wsfscservice) with the given RID and version. If no value is given
                          for version, the latest (as found in the local NuGet cache) will be used.
    stop <options>        Send a stop signal to the Booster service with the given version. If no version is given all
                          running instances are signaled. Use `--force` to kill process(es) instead of sending a stop
                          signal.
    list                  List running Booster versions and their source paths.

    Use 'dotnet-ws.exe <subcommand> --help' for additional information.

OPTIONS:

    --help                display this list of options.
```

---

## Managing WebSharper Booster instances

The WebSharper Booster is started as a standalone service called `wsfscservice.exe` when you first compile a WebSharper project, unless you specifically turn the Booster off in that project.

!!!info Turning off the Booster
If you need to turn the Booster off for a given project, you can do so by setting the following in the project's `wsconfig.json`:

```json
{
  ...
  "standalone": true,
  ...
}```
!!!

The Booster manages the runtimes that do the actual compilation work, one runtime per compiler version. If you use different WebSharper compilers (by version) in your various projects, you will notice that the Booster will report a runtime instance for each.

```text
$ dotnet ws list
The following wsfscservice versions are running:

  version: 6.0.0.226; path: C:\Users\granicz\.nuget\packages\websharper.fsharp\6.0.0.226\tools\net6.0\win-x64\wsfscservice.exe
  version: 6.0.0.228; path: C:\Users\granicz\.nuget\packages\websharper.fsharp\6.0.0.228\tools\net6.0\win-x64\wsfscservice.exe.
```

---

### Listing running instances


!!! Sorry...
The rest of this page is not yet written, please check back soon!
!!!

---

### Starting a new instance

---

### Stopping an instance

---

## Building projects
