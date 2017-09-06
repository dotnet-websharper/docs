# WebSharper 3 to 4-beta update guide

## Changes

WebSharper 4 was designed to be as compatible as possible with code written for WebSharper 3 while moving
closer to .NET semantics, and adding C# support.
Most code should compile and run without errors.

## NuGet packages

To update your WebSharper 3 project, update all package references to `4.0` versions and
add `WebSharper.FSharp` for F# projects, and `WebSharper.CSharp` for C# projects.
(Previously there was no compilation from C#, but C# web projects supported unpacking and serving
WebSharper sites defined in dependent F# libraries. Now this unpacking needs `WebSharper.CSharp`
even if you do not plan to add C# code you would compile to JavaScript).

During the beta period packages were released under codename `Zafir`.
If you were using the WebSharper 4 pre-releases, uninstall all `Zafir.*`
packages, and install their `WebSharper.*` counterparts.

### JavaScript Attribute changes

The `JavaScript` attribute is no longer an alias for `ReflectedDefinition`.
New features: you can use the attribute on assembly level: `[<assembly: JavaScript>]`. 
Also `[JavaScript(false)]` removes a module, type or member from the scope of the JavaScript translation.

### Extra F# language features

Syntax forms previously disallowed by the limitations of ReflectedDefinition becomes available to use for client-side code.
These are object expressions, byref and `&` operator, inner generic functions, pattern matching on arrays, statically resolved type parameters.
Also object-oriented features has been expanded, see below.

### Object-oriented features

WebSharper now supports .NET semantics for method overrides, interface implementations, static constructors,
base calls, having no implicit constructor.
Previous workarounds for these missing features may be breaking:
JavaScript-compiled names of implementation and override methods are now resolved automatically, and using the `Name` attribute on them is disallowed.
You can specify the JavaScript-compiled name at the interface or abstract method declaration.

Module-bound `let` values are no more initialized on page load, but on the first access of any value in the same source file (as in .NET).

### Namespace changes

Attribute types are now in `WebSharper` namespace, not `WebSharper.Pervasives` module.

### Single Page Application changes

Bundling now creates minimal code using code path exploration.
You have to mark the entry point with the new `[<SPAEntryPoint>]` attribute.
This must be a single static method with no arguments.

### Translation changes

These changes may be breaking if some of your code relies on exact JavaScript form of some translated code.

* Union types now respect the `UseNullAsTrueValue` flag.
For example the `None` value is now translated to `null`.

* Delegates are now having proper proxies, `Combine`, `Remove`, `Target` and `GetInvocationList` are usable on client-side.
Previously delegates were a shortcut to create multiple-argument JavaScript functions (already deprecated in WebSharper 3 but not removed). 

* Constructing a default value for example `Unchecked.defaultof` now always uses compile-time type information.
This can be problematic if the type is a generic parameter. For example using `match dict.TryGetValue x with ...`
can throw an error if dict has a generic value type, as the F# compiler is implicitly creating a default value
to pass to the out parameter of `TryGetValue`. You can get around it by wrapping the expression in the new
helper `DefaultToUndefined` which allows translating default values inside to just `undefined`:
`match DefaultToUndefined(dict.TryGetValue x) with ...`

* By default, classes (including F# records and unions) with no methods translated to JS instance methods and
having no base class are now translated not to have a prototype. JSON serializer was fixed to handle this.
This is a performance optimization, but disallows type checks or inheriting from a class like this. So a new
attribute `Prototype` was added to opt-in for generating a prototype for the type in the translation. Extra
features: `Prototype(false)` forces the type to have no prototype, converting instance methods to static in
translation.

* Previously, having `[<Inline "myLibrary.doSomething()">]` and `[<Inline "$global.myLibrary.doSomething()">]`
was working equivalently. Now access to global scope without the `$global` object is assumed to be initialized
before the current script starts running (WebSharper itself takes care of this, if you use the `Require`
attribute) and will not change, so it is safe to shorten. Whenever you want the exact call on the current
global scope (window object), be sure to use `$global`.

* Classes have now automatic reference equality. If you have relied on classes having automatic structural equality
in the WebSharper translation, this is now incorrect. Implement an override for `Equals` to fix it.

    Create an empty JavaScript plain object with `new JSObject()` / `New []`. Previously this was equivalent to `new
object()` / `obj()`, but now the latter translates to an instance of `WebSharper.Obj` which defines its own
`Equals` and `GetHashCode` methods.

* Default hash value of classes are now always -1. This should not be breaking, but if you use a class as keys
or rely on hashing in any other way, be sure to override `GetHashCode` on your class for performance.

* `System.Decimal` support has been removed from WebSharper main libraries. It is now a part of
`WebSharper.MathJS` and has correct precision.

### WebSharper Extension changes

C#-friendly overloads (using delegates and new function wrapper types) has been added to all libraries created by the WebSharper Interface Generator.
In some cases where the overloads would be ambigous with the F# versions (function with 0 or 1 arguments), the F# style overload has been removed.
You may need to convert to a delegate when using these methods, you can do this implicitly by having a lambda as the argument.
If the argument was previously an F# function value, you have to write a lambda for calling it.

### Project settings

WebSharper compiler now replaces `fsc.exe` and does the .NET and JavaScript translation, as well as unpacking resources in a single pass.
For sitelet projects, previously all dlls available in bin folder when the WebSharper build task ran was unpacked to Scripts and Content folders.
Now only explicit references are searched for WebSharper resources and scripts to unpack.

### Macros and generators

As the compiler pipeline of WebSharper has been replaced, the intermediate AST representation has changed.
Macros and generators also gained new features, they get more info as input and has more options for output.
Full API documentation will be available later.

Type inference changes which were at some point introduced `FSharp.Compiler.Service` can be breaking.
We have changed WIG Generic helper operators which can be used to construct generic types and members
to have different character lengts based on arity: for example use `Generic -- fun t1 t2 -> ...`
instead of just `Generic - fun t1 t2 -> ...`

`MacroResult.MacroNeedsResolvedTypeArg` now needs the offending type parameter as a field. You can decide
if a type is a type parameter of any kind by using the new `IsParameter` property.

### Macros relying on type information

Some built-in macros like the client-side JSON serialization relies on compile-time type information to generate JavaScript code.
For example `WebSharper.Json.Encode` is a function that previously could be called only with fully-specified type argument.
Now, you can use it in a generic function if you mark that function `[<Inline>]`.
In general, tranlation that relies on type information is delayed within inlines until the type arguments has been resolved at the call point.

### JSON APIs

Instead of using the `CompiledName` attribute to specify JSON-serialized name of an F# union case, use WebSharper's `Name` attribute.

`Json.SerializeWith` and similar functions taking a custom serializer object have been removed.
 
## Missing features compared to WebSharper 3

* TypeScript definition output (was outdated and under-tested)
* `WebSharper.Warp` has not been released yet for WS4 (compiling from `ReflectedDefinition`s in FSI)
* Clean does not remove unpacked code 