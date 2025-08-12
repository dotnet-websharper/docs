---
title: C# language support
---

WebSharper supports a subset of C# syntax and features, allowing you to write C# code that compiles to JavaScript.
Furthermore, WebSharper provides interoperability between C# and F#, enabling you to use both languages in a single solution.
The standard .NET interoperability between the two languages can be used, and WebSharper extends this by a (set of helper functions)[interoperability-with-f].

## Supported C# syntax

* classes, records, interfaces, enums
* control flow: `if`, `switch`, `try`/`catch`/`finally`, `break`, `continue`, `return`, `throw`, `yield return`, `while`, `do`/`while`, `for`, `foreach`, `using`, labels/`goto`
* pattern matching and deconstruction
* literals
* anonymous functions
* anonymous objects
* local functions
* operators (including user-defined operator overloading)
* interpolated strings (missing: padding)
* await on `Task`, `Task<TResult>`
* conditional access (`?.`) and null-coalescing (`??`) operators
* assignments (including composite assignment operators)
* `ref`, `in`, and `out` parameters, and `ref` return values
* optional parameters, default value
* delegates (having `Combine`, `Remove`, `Target`)
* implicit and explicit casts (including user-defined implicit/explicit operators), missing: numeric explicit casts (see planned) properly truncating value
* collection initializers (including dictionary initializers)
* query expressions - `from ... select ...`
* generator methods (with limitations: see planned features)
* async methods returning `Task`, `Task<TResult>` (with limitations: see planned features)
* `dynamic` type for easy JavaScript interop

### C# features and fixes planned

These features and fixes are planned for later release.

* struct definitions
* numeric explicit casts truncating value (currently does nothing, it is recommended to use `System.Math` methods instead)
* `checked`/`unchecked`
* padding in string interpolation
* resolving current limitations of generator and async methods: block-scoped variables and try/finally does not currently work inside a method transformed to a state-machine
* nullable operators should be absorbing (`null + 1` equals `null` in .NET)
* `Index` and `Range` types

### C# keywords not available for JavaScript

These keywords are used for interoperability with unmanaged code or in multi-threaded environment which is not supported by WebSharper:

* `typeof`
* `sizeof`
* `fixed`
* `unsafe`
* `lock`

## Interoperability with F#

There are helper classes included in WebSharper for most common conversions between `FSharp.Core` and standard .NET types.
These conversions work both on server and client side.

### Create F# values from standard .NET counterparts

The `WebSharper.FSharpConvert` static class contains static members to create values used commonly in F#.

* `FSharpConvert.Fun` overloads create curried F# functions for 1-8 arguments from `System.Action` and `System.Func` delegate values.
* `FSharpConvert.Option` overloads create an `FSharpOption<T>` value from a value of a .NET reference type or `Nullable` value type. (To create a 1)
* `FSharpConvert.List` overloads create an `FSharpList`.
* `FSharpConvert.Async` overloads create an `FSharpAsync<TResult>` from a `Task` or `Task<TResult>`.
* `FSharpConvert.Ref` overloads create an `FSharpRef` object with specified initial value or default value for a type.
* `Option.None` and F# `unit` values can be just `null`.

### Consume F# values from C#

Everything works as in .NET, there are no extra helpers provided by WebSharper.

* To call an `FSharpFunc` function, use its `Invoke` method. When calling curried F# functions, you have to chain calling `Invoke` to pass all arguments separately.
* Create F# union values using the `.New...` static method in the union type (or property for a union case with no fields).
* Create F# record values by using its constructor.
* Separate union cases by using the `Is...` methods, or the `Tag` property. Whenever a union has `null` as possible value, these methods are static.
* For the `FSharpOption` type, you can use `x?.Value`) to get the value or null.