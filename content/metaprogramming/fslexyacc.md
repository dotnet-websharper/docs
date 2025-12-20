---
title: FsLexYacc with WebSharper
---

FsLexYacc is a lexer+parser code generator from `fsy` and `fsl` files. The the official [FsYacc Overview](https://fsprojects.github.io/FsLexYacc/content/fsyacc.html) and [FsLex Overview](https://fsprojects.github.io/FsLexYacc/content/fslex.html). Also see the [JSON parser example](https://fsprojects.github.io/FsLexYacc/content/jsonParserExample.html) for getting to know the definition languages used.

If you start from a WebSharper project, to enable the FsLexYacc code generation, add the `FsLexYacc` version `11.2.0` nuget package to your project.
This will automatically add the build targets. Also include in your project file:

```xml
    <FsYacc Include="Parser.fsy">
      <OtherFlags>--module Parser</OtherFlags>
    </FsYacc>
    <FsLex Include="Lexer.fsl">
      <OtherFlags>--module Lexer --unicode</OtherFlags>
    </FsLex>
    <Compile Include="Parser.fsi" />
    <Compile Include="Parser.fs" />
    <Compile Include="Lexer.fs" />
```

(Note: `Lexer.fsi` seems to be producing incomplete output)

Create `Parser.fsy` and `Lexer.fsl` files to implement, the rest will be generated.

## Enabling the lexer+parser for client-side use

To also use the generated code from JavaScript-enabled code, add the `WebSharper.FsLexYacc` nuget package too.
It currently supports `FsLexYacc` version `11.2.0` exactly.
The only remaining piece is to mark the `Parser.fs` and `Lexer.fs` files for JavaScript translation. Because these are generated files, they should not be modified as those changes would be lost. The solution that WebSharper provides is to mark files for compilation either through `wsconfig.json` with:

```json
  "javascript": [ "Parser.fs", "Lexer.fs" ]
```

or by assembly-level attributes:

```fsharp
[<assembly: JavaScript("Parser.fs")>]
[<assembly: JavaScript("Lexer.fs")>]
do()
```

After this, you will be able to use your lexer+parser e.g.:

```fsharp
open FSharp.Text.Lexing

let lexbuf = LexBuffer<char>.FromString text
let parsed = Parser.start Lexer.tokenstream lexbuf
```