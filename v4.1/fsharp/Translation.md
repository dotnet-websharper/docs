# Translation and metaprogramming

WebSharper provides several ways to customize the way functions and
values are compiled to JavaScript:

* [Directly providing JavaScript code](#javascript);
* [Customizing the compiled name of the value](#name);
* [Transforming the F# code](#meta) during compilation, a concept
  known as metaprogramming.

<a name="javascript"></a>
## Embedding JavaScript

There are two ways of directly inserting JavaScipt code into a WebSharper project.

### JavaScript function body

The `Direct` attribute takes a JavaScript expression as a string parameter.
This will get parsed by the compiler and any syntax errors will be reported in a
compile error message.
This parsed expression will be used as a function body, `return` is automatically
added for the last value.

If you don't want the use function from .NET, the `X<'T>` type function in
`WebSharper.JavaScript` throws an exception of type `WebSharper.JavaScript.ClientSideOnly` with the message
"This function is intended for client-side use only.".

You can use placeholders for the function or method arguments.
For named parameters, the name with a `$` prepended is recognised.
For example:

    [<Direct "$x + $y" >]
    let add (x: int) (y: int) = X<int>

Also you can access parameters by index.
In let-bound functions in modules and static methods of classes, the parameters
are indexed from 0.

    [<Direct "$0 + $1" >]
    let add (x: int) (y: int) = X<int>
    
In instance methods, `$0` translates to the self indentifier, and method parameters
are indexed from 1.
(You can also use `$this` for the self identifier, but this recommended against, as
a parameter named `this` can override it, and it does not work for extension members
which are actually static methods in translated form.)

    [<Direct "Math.sqrt($0.x * $0.x + $0.y * $0.y)" >]
    member this.GetLength() = X<float>

### Inlined JavaScript code

The `Inline` attribute  takes a JavaScript expression as a string parameter.
(It can also be used together with the `JavaScript` attribute to inline a function
translated from F#.)
This will be parsed, and inlined to all call sites of this function.
Only a subset of JavaScript operators and keywords can be used which can be translated
to the "core" AST used internally by WebSharper to optimize output.

Parameter placeholders work exactly as with `Direct`. 

    [<Inline "$x + $y" >]
    let add (x: int) (y: int) = X<int>

## Inlines accessing global object

If you want a non-cached access to the global `window` object, use the `$global` fake variable inside inline strings.

For example, `[<Inline "myLibrary.doSomething()">]` assumes that `myLibrary` is initialized before the current script starts running (WebSharper itself takes care of this, if you use the `Require` attribute) and will not change, so access is safe to shorten.
On the other hand, `[<Inline "$global.myLibrary.doSomething()">]` always accesses `myLibrary` as a property of `window.` 

### Inline Helper

The `JS.Inline` function parses its first parameter at compile-time as JS code and includes
that in the result. It can contain holes, named `$0`, `$1`, ... and variable arguments will
be passed to the inline. Examples:

    open WebSharper.JavaScipt
    
    let zeroDate = JS.Inline("new Date()")
    let date = JS.Inline("new Date($0)", 1472226125177L)
	
### Constant

The `Constant` attribute takes a literal value as parameter.
It can annotate a property or a union case which will be translated to the literal provided.

<a name="name"></a>
## Naming

The `Name` attribute takes a string parameter and allows specifying
the name of a function or class in the translation.
For example:

    [<Name "add" >]
    let OriginalNameForAdd (x: int) (y: int) = x + y

<a name="naming-abstract"></a>
### Naming abstract members

If you set a fixed translated name with the `Name` attribute on an abstract member of a class
or interface, all inheriting and overriding members will have that exact translated name.
If a class is overriding or implementing two abstract members that has the same fixed name,
it will result in a compile-time error, and you have to change one of the fixed names to resolve it.

Automatically, interface members have a unique long translated name generated that contains the full type name.
This guarantees no conflicts without using the `Name` attribute.
If you want to shorten it for readability of the JS output and making it smaller,
you can use the `Name` attribute on the interface type to specify a short name for the interface.
It is recommended that it is unique across your solution.
You can also use `[<Name "">]` on the interface type to make all interface methods have the same translated name
as their original .NET name (if not specified otherwise by a `Name` on the member).

If you use the `[<JavaScipt false>]` attribute on an interface or abstract class, its member's naming will not be tracked by the compiler, this is sometimes useful for divergent behavior of the code in .NET and in JavaScipt.
	
<a name="meta"></a>
## Metaprogramming

Currently there are two ways of adding special logic to WebSharper translation.
The `Generated` attribute can be used to create a function's body by evaluating
an expression at compile-time.
The `Macro` attribute can be used to translate all call sites of a function with
a custom logic.

Documentation of macro and generator APIs are upcoming.