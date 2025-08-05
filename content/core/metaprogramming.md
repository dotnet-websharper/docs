---
title: Metaprogramming
---

WebSharper provides two main features for metaprogramming:
* Macros, which can determine how a member call is translated, usually using some .NET type information to customize the JavaScript equivalent. It intercepts any regular translation logic and allows full control instead.
* Generators, which can produce JavaScript output by setting a member body or modifying it.

Both can make use of a `WebSharper.Core.Metadata.ICompilation` instance, which exposes information and provides features to affect the compilation of the current project.
They also rely on the types in the `WebSharper.Core.AST` which is WebSharper's representation of types and code.

## Macros

WebSharper uses macros internally for many of its helpers. For example `New ["x" => 1]` translates to the object `{x: 1}` (assuming `open WebSharper.JavaScript`).
Here, the `New` function has a signature of `seq<string * obj> -> obj` and can take any sequence of tuples to create an object dynamically, but when the parameter is a list or array expression that is already known at compile-time, the resulting code is optimized to a minimal object expression. This optimization is not hard-coded into the compiler logic, but implemented with a macro tied to the `New` helper.

To create a macro, inherit from the `WebSharper.Core.Macro` class. 
Then you can annotate a type or member with the `[<Macro(typeof<MyMacro>)>]` attribute.
This will invoke the macro wherever that member (or members of the type) are called.

For example:
```fsharp
open WebSharper
open WebSharper.Core
open WebSharper.Core.AST

type NameOfMacro() =
    inherit Macro()
    override this.TranslateCall(c) =
        match c.Method.Generics with
        | [t] ->
            if t.IsParameter then
                MacroNeedsResolvedTypeArg t
            else
                MacroOk (Value (String t.TypeDefinition.Value.FullName))
        | _ ->
            MacroError "NameOfMacro expects a type argument"

open WebSharper.JavaScript

[<Macro(typeof<NameOfMacro>)>]
let nameof<'a> = X<string>
```

WebSharper does not replicate generics in the translated JavaScript code, that's why macros are useful to resolve behavior at compile time from .NET type information.
The above example will make `nameof<string>` translate to `'System.String'`, and in general, any resolved .NET type name will be translated to a string literal containing the full type name.
The `MacroNeedsResolvedTypeArg` return ensures that if the type argument is generic, you will get a compile-time error, unless you use `nameof` inside an `[<Inline>]` annotated member so that the generics can be resolved later.

### Macro implementation

The `WebSharper.Core.Macro` base class has the following members to override:

* `TranslateCall`: This method is invoked every time a call to a method annotated with this macro type is being translated. Gets an object with relevant context of the call:
  * `This`: the instance expression if it's an instance member call.
  * `DefiningType`: the type (including generic information) that defines the method of which the translation is intercepted.
  * `Method`: the method call (including generic information) of which the translation is intercepted.
  * `Arguments`: the arguments of the call to the annotated member.
  * `Parameter`: a parameter object if any is specified on the `[<Macro>]` attribute.
  * `IsInline`: is the macroed member also annotated with `[<Inline>]`.
  * `Compilation`: the [compilation object](#the-icompilation-interface).
  * `BoundVars`: variables that are bound by F# `let` expressions outside the call to the macroed member.

* `TranslateCtor`: This method is invoked every time a call to a constructor annotated with this macro type is being translated. Gets an object with relevant context of the call:
  * `Constructor`: the constructor (including generic information) of which the translation is intercepted.
  * Otherwise all the same properties as `TranslateCall` except not having the `This` and `Method` properties.

* `Close`: A method that will be called for each macro instance once, when the compilation is in the ending phase.

* `NeedsTranslatedArguments`: If true, the WebSharper translator will recursively translate .NET forms within argument expressions to their JavaScript counterpart before passing them to the macro. Default is false

### Macro results

The `TranslateCall` and `TranslateCtor` methods both should retunr a `MacroResult` value. This is a union that specifies how the translator should proceed. The cases are:

* `MacroOk`: return an AST `Expression` as a successful result of the macro operation.
* `MacroWarning`: this is a recursive case, adds a warning at current expression source position, but also returns some proper value.
* `MacroError`: macro reports an error, no expression result is returned.
* `MacroDependencies`: this is a recursive case, adds a list of dependency nodes to be factored in when doing dead code elimination.
* `MacroFallback`: the macro does nothing and falls back to the next way of translation. This can be a `[<JavaScript>]` or `[<Inline>]` code defined on the member, or even another macro.
* `MacroNeedsResolvedTypeArg`: the macro signals to the translator that there is a generic type argument it needs resolved to work with type information. If the member is marked with `[<Inline>]`, the execution of the macro is delayed when the inline code is applied and generics are resolved.
* `MacroUsedBoundVar`: this is a recursive case, the macro signals to the translator that the value of `let`-bound variable has been used and can be eliminated from the external code.

## Generators

Here is an example of a generator:

```fsharp
open WebSharper
open WebSharper.Core
open WebSharper.Core.AST

type ReadFileGenerator() =
    inherit Generator()
    override this.Generate g =
        match g.Parameter with
        | Some (:? string as path) ->
            let contents = System.IO.File.ReadAllText(path)
            GeneratedAST (Lambda([], None, Value (String contents)))
        | _ -> GeneratorError "ReadFileGenerator expects a file path parameter"

open WebSharper.JavaScript

[<Generated(typeof<ReadFileGenerator>, "data.txt")>]
let GetData() = X<string>
```

It reads a file at compilation time and makes the annotated function returns the contents of that file from the generated JavaScript code.
The file path is passed in as a parameter on the `Generated` attribute for flexibility.
During a compilation, the working directory is set to be the project directory, so relative paths will be resolved accordingly.

A generator must always return a function, not just the function body.
Instead of `GeneratedAST`, the same function could be returned as three more possible ways:
* As an F# quotation: `GeneratedQuotation <@@ fun () -> contents @@>`
* As a string: `GeneratedString ("function() { return '"  + contents.Replace("'", "\'") + "'}")`
* As JavaScript syntax AST: `GeneratedJavaScript (S.Lambda (None, [], [ S.Return (Some (S.Constant (S.String contents))) ], false))` (using `module S = WebSharper.Core.JavaScript.Syntax`)

### Generator implementation

The `WebSharper.Core.Generator` base class has a single member to override:
* `Generate`: This method is invoked once for each annotated member to generate it's translated body. Gets an object with relevant context of the call:
  * `Member`: the member that contents is being generated for, it can be a constructor, method, or implementation method.
  * `Parameter`: the optional parameter passed in to the `Generated` attribute.
  * `Compilation`: the [compilation object](#the-icompilation-interface).
  * `Expression`: if the member is annotated with `[<JavaScript>]`, the code contents of it. This allows making decorator-like functionality.
  * `CompiledMember`: describes how the member will end up in the JavaScript output.

### Generator results

The `Generate` method both should retunr a `GeneratorResult` value. This is a union that specifies how the translator should proceed. The cases are:

* `GeneratedQuotation`: returns generator result as an F# quotation value.
* `GeneratedAST`: returns generator result as a WebSharper AST `Expression` value.
* `GeneratedString`: returns generator result as a string, this will be parsed and checked by the WebSharper compiler. Only the syntax forms supported by `[<Inline>]` are accepted.
* `GeneratedJavaScript`: returns generator result as a JavaScript AST (used by WebSharper for final step of writing code). This has to be converted back to WebSharper AST to interact with code bundling, so still only the syntax forms supported by `[<Inline>]` are accepted.
* `GeneratorError`: the generator raises a compilation error.
* `GeneratorWarning`: this is a recursive case, adds a warning at the member, but also returns some proper value.
* `GeneratorCompiledMemberChange`: this is a recursive case, allows modifying how the member will be represented i n the compiled JavaScript code.

## The ICompilation interface

The `ICompilation` interface provides access to underlying F# and C# code information with a unified API, as well as WebSharper functionality.

The following methods are available to get information about and affect the current project's compilation:

* `GetCustomTypeInfo`: Get information about an F# records, F# unions, delegates, enums, structs.
* `GetInterfaceInfo`: Get information about an interface.
* `GetClassInfo`: Get all information about a class, including what name and module file (address) it is translated.
* `GetQuotation`: Get information about a translated quotation at a given source position.
* `GetTypeAttributes`: Get the list of attributes on a type.
* `GetFieldAttributes`: Get the list of attributes on a field.
* `GetMethodAttributes`: Get the list of attributes on a method.
* `GetConstructorAttributes`: Get the list of attributes on a constructor.
* `GetJavaScriptClasses`: Gets a list of all classes with known JavaScript translations (including all referenced projects and current project).
* `GetTSTypeOf`: Translates a .NET type information to its TypeScript equivalent.
* `ParseJSInline`: Parses a JavaScript string, to the extent of what WebSharper supports via the `[<Inline>]` attribute.
* `NewGenerated`: Creates a unique location for generated function within a special `$Generated` module.
* `NewGeneratedVar`: Creates a variable in the special `$Generated` module.
* `AddGeneratedCode`: Adds code to the `$Generated` module, pass it a method that has been previously created to be unique with `NewGenerated`.
* `AddGeneratedInline`: Adds an inlined method inside the `$Generated` module, pass it a method that has been previously created to be unique with `NewGenerated`.
* `AssemblyName`: The current project's assembly name.
* `GetMetadataEntries`: Get a dictionary entry from WebSharper metadata. This can be used so instances of macros/generators can leave information to each other across projects.
* `AddMetadataEntry`: Adds a dictionary entry to WebSharper metadata
* `GetJsonMetadataEntry`: Specialized to get the dictionary entry about the JSON encoder or decoder of a given type. The first argument is true for encoder, false for decoder.
* `AddJsonMetadataEntry`: Specialized to add a dictionary entry about the JSON encoder or decoder of a given type. The first argument is true for encoder, false for decoder.
* `AddError`: Registers an error at a given source position.
* `AddWarning`: Registers a warning at a given source position. 
* `AddBundle`: Creates a web worker bundle.
* `AddJSImport`: Generates an expression that is equivalent to using the `JS.Import` helper.
* `AddJSImportSideEffect`: Generates an expression that is equivalent to using the `JS.Import` helper for side effect only.

## The WebSharper AST

Here is a macro that uses AST (abstract syntax tree) patterns:

```fsharp
open WebSharper
open WebSharper.Core
open WebSharper.Core.AST

type AddMacro() =
    inherit Macro()
    override this.TranslateCall(c) =
        match c.Arguments with
        | [a; b] ->
            match IgnoreExprSourcePos a, IgnoreExprSourcePos b with
            | Value (Int ai), Value (Int bi) ->
                Value (Int (ai + bi)) |> MacroOk
            | _ -> MacroFallback
        | _ -> MacroError "AddMacro expects two arguments"

[<Macro(typeof<AddMacro>); JavaScript>]
let add a b = a + b
```

This simple macro checks at compile time if the two arguments are both int literals, and if so, it adds them up. So `add 1 2` will translate directly to `3`. 
For any other case, it uses the `MacroFallback` result, to invoke the next compilation path, which in this case is a normal JavaScript call to the `add` function.

The `IgnoreExprSourcePos` function skips source position cases to match against the actual expression/statement form.
The `IgnoreSourcePos` module has helpers for each AST form.

### AST overview

WebSharper's AST contains forms for both .NET and JavaScript/TypeScript concepts, with an overlap on common concepts.
During a compilation, all the .NET-specific forms are gradually transformed for the result to be compatible with JavaScript.

After a macro or generator returns an expression, it will be traversed again for eliminating .NET-specific forms, so they are fine to use in the output.

There are also some forms marked "Temporary" below, these are only utilized for the early part of the F#/C# compilation pipelines, not the common transformer. Do not use them in macro or generator outputs.

### Expression AST forms

Shared cases:
  * `Undefined`: JavaScript `undefined` value or `void` in .NET
  * `Var`: Gets the value of a variable
  * `Value`: Contains a literal value
  * `Application`: Function application with extra information. The `Purity` field should be `NoSideEffect` if the function call can be omitted if the result is not used, and `Pure` when the function is deterministic. The `KnownLength` field should be `Some x` only when the function is known to have `x` number of arguments and does not use the `this` value.
  * `Function`: Function declaration
  * `VarSet`: Variable set
  * `Sequential`: Sequential evaluation of expressions, value is taken from the last
  * `NewTuple`: Creating a new tuple in .NET, array expression in JavaScript
  * `Conditional`: Conditional operator
  * `Binary`: Binary operation
  * `MutatingBinary`: Binary operation mutating right side
  * `Unary`: Unary operation
  * `MutatingUnary`: Unary operation mutating value
  * `ExprSourcePos`: Original source location for an expression
  * `Base`: Refers to the base class from an instance method, `super` in JavaScript
JavaScript-specific cases:
  * `ItemGet`: JavaScript object property get
  * `ItemSet`: JavaScript object property set
  * `JSThis`: JavaScript `this` value
  * `Object`: JavaSript object
  * `GlobalAccess`: A global or imported value
  * `SideeffectingImport`: An import statement with no value used
  * `GlobalAccessSet`: A global or imported value setter
  * `New`: JavaScript `new` call
  * `Cast`: TypeScript type cast `<...>...`
  * `ClassExpr`: JavaScript `class { ... }` expression
  * `Verbatim`: JavaScript verbatim code
.NET-specific cases:
  * `Call`: .NET method call
  * `CallNeedingMoreArgs`: Temporary - Partial application, workaround for FCS issue #414
  * `CurriedApplication`: Temporary - F# function application, bool indicates if the argument has type unit
  * `OptimizedFSharpArg`: Temporary - optimized curried or tupled F# function argument
  * `Ctor`: .NET constructor call
  * `ChainedCtor`: .NET chained or base constructor call
  * `CopyCtor`: .NET creating an object from a plain object, used for F# records/unions
  * `FieldGet`: .NET field getter
  * `FieldSet`: .NET field setter
  * `Let`: .NET immutable value definition used only in expression body
  * `NewVar`: .NET expression-level variable declaration
  * `Coalesce`: .NET null-coalescing
  * `TypeCheck`: .NET type check, returns bool
  * `Coerce`: .NET type coercion
  * `NewDelegate`: .NET delegate construction
  * `StatementExpr`: .NET statement inside an expression. Optional result should be an identifier for a variable which is not explicitly defined inside the statement
  * `LetRec`: F# `let rec`
  * `NewRecord`: F# record constructor
  * `NewUnionCase`: F# union case constructor
  * `UnionCaseTest`: F# union case test
  * `UnionCaseGet`: F# union case field getter
  * `UnionCaseTag`: F# union case tag getter
  * `MatchSuccess`: F# successful match
  * `TraitCall`: F# trait call (use of type member within functions with statically resolved type parameters)
  * `Await`: Temporary - C# await expression
  * `NamedParameter`: Temporary - C# named parameter
  * `RefOrOutParameter`: Temporary - C# ref or out parameter
  * `ComplexElement`: Temporary - C# complex element in initializer expression
  * `Hole`: Temporary - A hole in an expression for inlining
  * `ObjectExpr`: F# object expression

### Statement AST forms

Shared cases:
  * `Empty`: Empty statement
  * `Break`: break statement
  * `Continue`: continue statement
  * `ExprStatement`: Expression as statement
  * `Return`: Return a value
  * `Block`: Block of statements
  * `VarDeclaration`: Variable declaration
  * `FuncDeclaration`: Function declaration
  * `While`: `while` loop
  * `DoWhile`: `do ... while` loop
  * `For`: `for` loop
  * `If`: `if` statement
  * `Throw`: F# `raise` or C#/JavaScript `throw` statement
  * `TryWith`: `try ... with` statement
  * `TryFinally`: `try ... finally` statement
  * `Labeled`: Statement with a label
  * `StatementSourcePos`: Original source location for a statement
JavaScript-specific cases:
  * `ForIn`: JavaScript `for ... in` loop
  * `Switch`: JavaScript `switch` expression
  * `Import`: JavaScript `import * as ... from ...` statement
  * `ExportDecl`: JavaScript `export` statement
  * `Declare`: TypeScript `declare ...` statement
  * `Class`: JavaScript `class { ... }` statement
  * `ClassMethod`: JavaScript class method
  * `ClassConstructor`: JavaScript class constructor
  * `ClassProperty`: JavaScript class plain property
  * `ClassStatic`: JavaScript class static block
  * `Interface`: TypeScript `interface { ... }` declaration
  * `Alias`: TypeScript `type` or `import` alias
  * `XmlComment`: TypeScript triple-slash directive
  * `LazyClass`: Temporary - class during packaging that might need `Lazy` wrapper
  * `FuncSignature`: TypeScript function signature
.NET-specific cases:
  * `Goto`: Temporary - C# `goto` statement
  * `Continuation`: Temporary - go to next state in state-machine for iterators, async methods, or methods containing gotos
  * `Yield`: Temporary - C# `yield return` statement
  * `CSharpSwitch`: Temporary - C# `switch` statement
  * `GotoCase`: Temporary - C# `goto case` statement
  * `DoNotReturn`: F# tail call position

## Using transformers and visitors

Inherit from the `WebSharper.Core.AST.Transformer` type to create AST transformers to recursively modify AST values. It has overridable methods named `Transform...` for each AST form that recurses on each child expression/statement, as well as a `TransformExpression` and `TransformStatement` that matches all cases and calling their case transformer methods, and also a `TransformId` for identifiers. An example:

```fsharp
type SubstituteId(fromId, toId) =
    inherit Transformer()

    override this.TransformId(i) = if i = fromId then toId else i
```

With this transformer, `SubstituteId(v, w).TransformExpression(expr)` will return an expression with the same structure as `expr` except all occurrences of the identifier `v` are replaced with `w`.

Inherit from the `WebSharper.Core.AST.Visitor` type to create AST visitors. Visitors can be used to evaluate AST values recursively for certain logic, without constructing a new AST. A mutable property can keep track of the relevant information. For example:

```fsharp
type CountIdUse(countingId) =
    inherit Visitor()

    let mutable c = 0

    override this.VisitId(i) =
        if i = countingId then 
            c <- c + 1

    member this.Get(expr) =
        this.VisitExpression(expr) 
        c
```

With this visitor, `CountIdUse(v).Get()` will return the number of occurrences the identifier `v` is referenced within the `expr` AST value.
