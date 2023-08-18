---
order: -40
expanded: true
label: Views
---
# Reactive views

The full power of WebSharper.UI's reactive layer comes from using `View` values. A `View<'T>` is a time-varying value computed from any number of Vars and other Views, and its value automatically changes (if read programmatically or bound on the UI, see below) each time any of those dependencies change.

!!!warning Important
One important property to remember is that the value of a View is not computed unless it is needed. For example, if you use `View.Map`, the function passed to it will only be called if the result is needed. It will only be run while the resulting View is included in the document using [one of these methods](#inserting-views-in-docs). This means that you generally don't have to worry about expensive computations being performed unnecessarily. However it also means that you should avoid relying on side-effects performed in functions like `View.Map`.
!!!

Note that several of the functions below can be used more concisely using [the V shorthand](#the-v-shorthand).

---

## Creating and combining Views

The first and main way to get a View is using the [`View`](/api/v4.1/WebSharper.UI.Var\`1#View) property of `Var<'T>`. This retrieves a View that tracks the current value of the Var.

You can create Views using the following functions and combinators from the `View` module:

---

### View.Const

Creates a View whose value is always the same.

```fsharp
let v = View.Const 42
```

---

### View.ConstAnyc

Similar to `View.Const`, but is initialized asynchronously. Until the async returns, the resulting View is uninitialized.

---

### View.Map

Takes an existing View and maps its value through a function.

```fsharp
let v1 : View<string> = // ...
let v2 = View.Map (fun s -> String.length s) v1
```

---

### View.Map2

Takes two existing Views and map their value through a function.

```fsharp
let v1 : View<int> = // ...
let v2 : View<int> = // ...
let v3 = View.Map2 (fun x y -> x + y) v1 v2
```

Similarly, `View.Map3` takes three existing Views and map their value through a function.

---

### View.MapAsync

Similar to `View.Map`, but maps through an asynchronous function.

!!!warning Important
An important property here is that this combinator saves work by abandoning requests. That is, if the input view changes faster than we can asynchronously convert it, the output view will not propagate change until it obtains a valid latest value. In such a system, intermediate results are thus discarded.
!!!

Similarly, `View.MapAsync2` maps two existing Views through an asynchronous function.

---

### View.Apply

Takes a View of a function and a View of its argument type, and combines them to create a View of its return type.

While Views of functions may seem like a rare occurrence, they are actually useful together with `View.Const` in a pattern that can lift a function of any number N of arguments into an equivalent of `View.Map`N.

```fsharp
// This shorthand is defined in WebSharper.UI.Notation.
let (<*>) vf vx = View.Apply vf vx

// Inputs: a function of 4 arguments and 4 Views.
let f a b c d = // ...
let va = // ...
let vb = // ...
let vc = // ...
let vd = // ...

// Equivalent to a hypothetical `View.Map4 f va vb vc vd`.
let combinedView =
    View.Const f <*> va <*> vb <*> vc <*> vd
```

---

## Inserting Views in Docs

Once you have created a View to represent your dynamic content, here are the various ways to include it in a Doc:

---

### textView

Is a reactive counterpart to `text`, which creates a text node from a `View<string>`.

```fsharp
let varTxt = Var.Create ""
let vLength =
    varTxt.View
    |> View.Map String.length
    |> View.Map (fun l -> sprintf "You entered %i characters." l)
div [] [
    Doc.Input [] varTxt
    textView vLength
]
```

---

### Doc.BindView

Maps a View into a dynamic Doc.

```fsharp
let varTxt = Var.Create ""
let vWords =
    varTxt.View
    |> View.Map (fun s -> s.Split(' '))
    |> Doc.BindView (fun words ->
        words
        |> Array.map (fun w -> li [] [text w])
        |> Doc.Concat
    )
div [] [
    Doc.InputType.Text [] varTxt
    text "You entered the following words:"
    ul [] [ vWords ]
]
```

---

### Doc.EmbedView

Unwraps a `View<Doc>` into a Doc. It is equivalent to `Doc.BindView id`.

---

### attr.*Dyn

The reactive equivalent to the corresponding `attr.*` functions, creating an attribute from a `View<string>`.

For example, the following sets the background of the input element based on the user input value:

```fsharp
let varTxt = Var.Create ""
let vStyle =
    varTxt.View
    |> View.Map (fun s -> "background-color: " + s)
Doc.Input [attr.styleDyn vStyle] varTxt
```

---

### attr.*DynPred

Similar to the `attr.*Dyn` family of funtions, but takeing an extra `View<bool>`. When this View is true, the attribute is set (and dynamically updated as with `attr.*Dyn`), and when it is false, the attribute is removed.

```fsharp
let varTxt = Var.Create ""
let varCheck = Var.Create true
let vStyle =
    varTxt.View
    |> View.Map (fun s -> "background-color: " + s)
div [] [
    Doc.Input [attr.styleDynPred vStyle varCheck.View] varTxt
    Doc.CheckBox [] varCheck
]
```

---

## Mapping Views on sequences

Applications often deal with varying collections of data. This means using a View of a sequence: a value of type `View<seq<T>>`, `View<list<T>>` or `View<T[]>`. In this situation, it can be sub-optimal to use `Map` or `Doc` to render it: the whole sequence will be re-computed even when a single item has changed.

The `SeqCached` family of functions fixes this issue. These functions map a View of a sequence to either a new `View<seq<U>>` (functions `View.MapSeqCached*` and method `.MapSeqCached()`) or to a `Doc` (functions `Doc.BindSeqCached` and method `.DocSeqCached()`) but avoid re-mapping items that haven't changed.

There are different versions of these functions, which differ in how they decide that an item "hasn't changed".

---

### View.MapSeqCached

`View.MapSeqCached : ('T -> 'V) -> View<seq<'T>> -> View<seq<'V>>` uses standard F# equality to check items.

```fsharp
let varNums = Var.Create [1; 2; 3]

let vStrs = 
    varNums.View
    |> View.MapSeqCached (fun i -> 
        Console.Log i
        p [] [ text (string i) ]
    )
    |> Doc.BindView Doc.Concat
    |> Doc.RunAppend JS.Document.Body
// Prints 1, 2, 3
// Displays 1, 2, 3

varNums.Value <- [1; 2; 3; 4]
// Prints 4
// Displays 1, 2, 3, 4
// Note: the existing <p> tags remain, they aren't recreated.

varNums.Value <- [3; 2]
// Prints nothing
// Displays 3, 2
```

---

### View.MapSeqCachedBy

`View.MapSeqCachedBy : ('T -> 'K) -> ('T -> 'V) -> View<seq<'T>> -> View<seq<'V>>` uses the given key function to check items. This means that if an item is added whose key is already present, the corresponding returned item is _not_ changed. So you should only use this when items are intended to be added or removed, but not changed.

```fsharp
type Person = { Id: int; Name: string: int }

let ann =   { Id = 0; Name = "Ann" }
let brian = { Id = 1; Name = "Brian" }
let bobby = { Id = 1; Name = "Bobby" }
let clara = { Id = 2; Name = "Clara" }
let dave =  { Id = 3; Name = "Dave" }

let varPeople = Var.Create [ann; brian; clara]

varPeople.View
|> View.MapSeqCachedBy (fun p -> p.Id) (fun p -> 
    Console.Log p.Id
    p [] [text (string p.Name)]
)
|> Doc.BindView Doc.Concat
|> Doc.RunAppend JS.Document.Body
// Prints 1, 2, 3
// Displays Ann, Brian, Clara

varPeople.Value <- [ann; brian; clara; dave]
// Prints 4
// Displays Ann, Brian, Clara, Dave
// Note: the existing <p> tags remain, they aren't recreated.

varPeople.Value <- [ann; bobby; clara; dave]
// Prints nothing
// Displays Ann, Brian, Clara, Dave
// The item with Id = 1 is already rendered as Brian,
// so it is not re-rendered as Bobby.
```

---

### View.MapSeqCachedViewBy

`View.MapSeqCachedViewBy : ('T -> 'K) -> ('K -> View<'T> -> 'V) -> View<seq<'V>>` covers the situation where items are identified by a key function and can be updated. Instead of passing the item's value to the mapping function, it passes a View of it, so you can react to the changes.

```fsharp
varPeople.View
|> View.MapSeqCachedViewBy (fun p -> p.Id) (fun pid vp -> 
    Console.Log pid
    p [] [textView (vp |> View.Map (fun p -> string p.Name))]
)
|> Doc.BindView Doc.Concat
|> Doc.RunAppend JS.Document.Body
// Prints 1, 2, 3
// Displays Ann, Brian, Clara

varPeople.Value <- [ann; brian; clara; dave]
// Prints 4
// Displays Ann, Brian, Clara, Dave
// Note: the existing <p> tags remain, they aren't recreated.

varPeople.Value <- [ann; bobby; clara; dave]
// Prints nothing
// Displays Ann, Bobby, Clara, Dave
// The item with Id = 1 is already rendered as Brian,
// so its <p> tag remains but its text content changes.
```

---

Each of these `View.MapSeqCached*` functions has a corresponding `Doc.BindSeqCached*`:

* `Doc.BindSeqCached : ('T -> #Doc) -> View<seq<'T>> -> Doc`
* `Doc.BindSeqCachedBy : ('T -> 'K) -> ('T -> #Doc) -> View<seq<'T>> -> Doc`
* `Doc.BindSeqCachedViewBy : ('T -> 'K) -> ('K -> View<'T> -> #Doc) -> View<seq<'T>> -> Doc`

These functions map each item of the sequence to a Doc and then concatenate them. They are basically equivalent to passing the result of the corresponding `View.MapSeqCached*` to `Doc.BindView Doc.Concat`, the same way the examples above did.

Finally, all of the above functions are also available as extension methods on the `View<seq<'T>>` type.

* `.MapSeqCached()` overloads correspond to `View.MapSeqCached*` functions, and
* `.DocSeqCached()` overloads correspond to `Doc.BindSeqCached*` functions.
