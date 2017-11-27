# UI.Next - Functional Reactive Programming and HTML

## Dataflow

<a name="var"></a>
### Var

Reactive variables lay at the foundation of the Dataflow layer. They can be created and manipulated just like regular `ref` cells. Additionally, variables can be lifted to the [View](#view) type to participate in the dataflow graph.

#### Members of the type `Var<'T>` and module `Var`

```fsharp
interface IRef<'T>
```

Var implements the [`IRef`](#iref) interface.

---

```fsharp
val Var.Create : 'T -> Var<'T>
```

Create a new Var with the given initial value.


---

```fsharp
val Var.CreateWaiting : unit -> Var<'T>
```

Create a new Var in a waiting state. In this state, explicitly getting the value (with the property `Value` or the function `Get`) return null, but the View is not set yet and will not provide a value to the dataflow until `Set` is called on this.

---

```fsharp
val Var.CreateLogged : string -> 'T -> Var<'T>
```

Create a new Var with the given initial value.
    
Stores it on window.UINVars object with the given key for debugging purposes.

---

<a name="var_view"></a>
```fsharp
member this.View : View<'T>
```

Get the [View](#view) that dynamically observes this. Equivalent to [`View.FromVar`](#view_fromvar).

---

```fsharp
val Var.Get : Var<'T> -> 'T
```

Get the current value. See also [`Value`](#var_value).

---

```fsharp
val Var.GetId : Var<'T> -> int
```

Gets the unique ID associated with the var.

---

```fsharp
val Var.Lens : IRef<'T> -> get:('T -> 'U) -> update:('T -> 'U -> 'T) -> IRef<'U>
member this.Lens : get:('T -> 'U) -> update:('T -> 'U -> 'T) -> IRef<'U>
```

Create a new IRef which lenses onto a part of this.

When getting the IRef's value, `get` is called on the value of this. When setting the IRef's value, `update` is called with the current value of this and the new value, and the result is set as the new value of this.

---

```fsharp
val Var.Set : Var<'T> -> 'T -> unit
```

Set the current value. See also [`Value`](#var_value).

---

```fsharp
val Var.SetFinal : Var<'T> -> 'T -> unit
```

Set the final value. After this, Set / Update is invalid.

This is rarely needed, but can help solve memory leaks when mutliple views are scheduled to wait on a variable that is never going to change again.

---

```fsharp
val Var.Update : Var<'T> -> ('T -> 'T) -> unit
```

Update the value based on the current value. Equivalent to the `Update` method of the `IRef` interface.

---

<a name="var_value"></a>
```fsharp
member this.Value : 'T with get, set
```

Get or set the current value. See also [`Get`](#var_get), [`Set`](#var_set).

<a name="view"></a>
### View

`View<'T>` represents a node in the Dataflow layer. Intuitively, it is a time-varying value computed from your model. At any point in time the view has a certain `'T`.

In pseudo-code below, `[[x]]` notation is used to denote value of the View `x` at every point in time, so that `[[x]]` = `[[y]]` means that the two views are observationally equivalent.

#### Members of the type `View<'T>` and module `View`

```fsharp
val View.Apply : View<'T -> 'U> -> View<'T> -> View<'U>
```

Generalize [`Map`](#view_map) to apply a time-varying function to a time-varying value, such that `[[View.Apply f v]] = [[f]] [[v]]`.

Together with [`Const`](#view_const), this permits a code pattern to create an equivalent of `View.MapN` for any number N of Views:

```fsharp
let ( <*> ) f x = View.Apply f x

View.Const (fun x y z -> (x, y, z)) <*> x <*> y <*> z
```

The above alias `(<*>)` is predefined in the module `WebSharper.UI.Next.Notation`.

---

```fsharp
val View.AsyncAwait : ('T -> bool) -> View<'T> -> Async<'T>
```

Returns as soon as the view's value matches the filter function.

---

<a name="view_bind"></a>
```fsharp
val View.Bind : ('T -> View<'U>) -> View<'T> -> View<'U>
member this.Bind : ('T -> View<'U>) -> View<'U>
```

Create a View which depends entirely on the current value of this, such that `[[View.Bind f v]] = [[ f [[v]] ]]`.

Dynamic composition via `View.Bind` and `View.Join` should be used with some care. Whenever static composition (such as `View.Map2` or `View.Apply`) can do the trick, it should be preferable. One concern here is efficiency, and another is state, identity and sharing (see Sharing for a discussion).

---

```fsharp
val View.BindInner : ('T -> View<'U>) -> View<'T> -> View<'U>
member this.BindInner : ('T -> View<'U>) -> View<'U>
```

Equivalent to `Bind`, but additionally, obsoletes the inner result whenever the input View changes. This can avoid leaks when a new View is created on every call to the binding function.

---

<a name="view_const"></a>
```fsharp
val View.Const : 'T -> View<'T>
```

Create a View that does not vary.

---

```fsharp
val View.ConstAsync : Async<'T> -> View<'T>
```

Create a view that awaits the given asynchronous task and gets its value, after which it does not vary.

---

<a name="view_fromvar"></a>
```fsharp
val View.FromVar : Var<'T> -> View<'T>
```

Get the [View](#view) that dynamically observes a Var. Equivalent to [`var.View`](#var_view).

---

```fsharp
val View.Get : ('T -> unit) -> View<'T> -> unit
```

Retrieve the current value of a View and pass it to the given callback. If the View is currently awaiting, the callback will be called as soon as the View receives a value. The callback is called at most once.

---

```fsharp
val View.GetAsync : View<'T> -> Async<'T>
```

Asynchronously retrieve the current value of a View. If the View is currently awaiting, the Async will return as soon as the View receives a value. Equivalent to:

```fsharp
Async.FromContinuations (fun (ok, _, _) -> View.Get ok v)
```

---

<a name="view_join"></a>
```fsharp
val View.Join : View<View<'T>> -> View<'T>
```

Flattens a higher-order view, such that `[[ View.Join v ]] = [[ [[v]] ]]`.

Dynamic composition via `View.Bind` and `View.Join` should be used with some care. Whenever static composition (such as `View.Map2` or `View.Apply`) can do the trick, it should be preferable. One concern here is efficiency, and another is state, identity and sharing (see Sharing for a discussion).

---

```fsharp
val View.JoinInner : View<View<'T>> -> View<'T>
```

Equivalent to `Join`, but additionally, obsoletes the inner result whenever the input View changes. This can avoid leaks when a new View is created on every call to the binding function.

---

<a name="view_map"></a>
```fsharp
val View.Map : ('T -> 'U) -> View<'T> -> View<'U>
member this.Map : ('T -> 'U) -> View<'U>
```

Create a View whose value is passed through a function from the value of this, such that `[[View.Map f v]] = f [[v]]`.

---

<a name="view_map2"></a>
```fsharp
val View.Map2 : ('T -> 'U -> 'V) -> View<'T> -> View<'U> -> View<'V>
```

Generalizes [`Map`](#view_map) by mapping over two Views, such that `[[View.Map f v1 v2]] = f [[v1]] [[v2]]`.

---

```fsharp
val View.Map2Unit : View<unit> -> View<unit> -> View<unit>
```

Optimized version of [`Map2`](#view_map2) for two unit views.

---

```fsharp
val View.Map3 : ('T -> 'U -> 'V -> 'W) -> View<'T> -> View<'U> -> View<'V> -> View<'W>
```

Generalizes [`Map`](#view_map) by mapping over two Views, such that `[[View.Map f v1 v2]] = f [[v1]] [[v2]]`.

---

<a name="view_mapasync"></a>
```fsharp
val View.MapAsync : ('T -> Async<'U>) -> View<'T> -> View<'U>
member this.MapAsync : ('T -> Async<'U>) -> View<'U>
```

Create a View whose value is passed through a function from the value of this, such that `[[View.MapAsync f v]]` eventually equals `f [[v]]` once `f` finishes.

An important property here is that this combinator saves work by abandoning requests. That is, if the input view changes faster than we can asynchronously convert it, the output view will not propagate change until it obtains a valid latest value. In such a system, intermediate results are thus discarded.

---

```fsharp
val View.MapAsync2 : ('T -> 'U -> Async<'V>) -> View<'T> -> View<'U> -> View<'V>
```

Generalizes [`MapAsync`](#view_mapasync) by mapping over two Views, such that `[[View.MapAsync2 f v1 v2]]` eventually equals `f [[v1]] [[v2]]` once `f` finishes. See [`MapAsync`](#view_mapasync) for considerations about asynchronicity.

---

```fsharp
val View.MapCached : ('T -> 'U) -> View<'T> -> View<'B>
    when 'T : equality
```

Equivalent to [`Map`](#view_map), except the function is not called again if the input value is equal to the previous one. Note that only the latest value is checked, not the whole history.

---

```fsharp
val View.MapCachedBy : ('T -> 'T -> bool) -> ('T -> 'U) -> View<'T> -> View<'B>
    when 'T : equality
```

Equivalent to [`Map`](#view_map), except the function is not called again if the input value is equal to the previous one according to the given equality function. Note that only the latest value is checked, not the whole history.

---

<a name="view_mapseqcached"></a>
```fsharp
val View.MapSeqCached : ('T -> 'U) -> View<'seqT> -> View<seq<'U>>
    when 'T : equality and 'seqT :> seq<'T>

type View<seq<'T>> with
    member view.MapSeqCached : ('T -> 'U) -> View<seq<'U>>
        when 'T : equality
```

Starts a process doing stateful conversion with “shallow” memoization. The process remembers inputs from the previous step, and re-uses outputs from the previous step when possible instead of calling the converter function.

Memory use is proportional to the longest sequence between the current and preview value taken by the View. Since only one step of history is retained, there is no memory leak.

Obsolete synonym: `View.Convert`.

---

```fsharp
val View.MapSeqCachedBy : ('T -> 'Key) -> ('T -> 'U) -> View<'seqT> -> View<seq<'U>>
    when 'Key : equality and 'seqT :> seq<'T>

type View<seq<'T>> with
    member view.MapSeqCachedBy : ('T -> 'Key) * ('T -> 'U) -> View<seq<'U>>
        when 'Key : equality
```

Equivalent to [`MapSeqCached`](#view_mapseqcached), except input items are compared by the given key function instead of equality on `'T`.

Obsolete synonym: `View.ConvertBy`.

---

<a name="view_mapseqcachedview"></a>
```fsharp
val View.MapSeqCachedView : (View<'T> -> 'U) -> View<'seqT> -> View<seq<'U>>
    when 'T : equality and 'seqT :> seq<'T>

type View<seq<'T>> with
    member view.MapSeqCachedView : (View<'T> -> 'U) -> View<seq<'U>>
        when 'T : equality
```

Equivalent to [`MapSeqCached`](#view_mapseqcached), except input items are passed as a View to the mapping function. At every step, changes to inputs identified as being the same object are propagated via that view.

Obsolete synonym: `View.ConvertBy`.

---

```fsharp
val View.MapSeqCachedViewBy : ('T -> 'Key) -> ('Key -> View<'T> -> 'U) -> View<'seqT> -> View<seq<'U>>
    when 'Key : equality and 'seqT :> seq<'T>

type View<seq<'T>> with
    member view.MapSeqCachedViewBy : ('T -> 'Key) * ('Key -> View<'T> -> 'U) -> View<seq<'U>>
        when 'Key : equality
```

Equivalent to [`MapSeqCachedView`](#view_mapseqcachedview), except input items are compared by the given key function instead of equality on `'T`.

Obsolete synonym: `View.ConvertViewBy`.

---

```fsharp
val View.RemovableSink : ('T -> unit) -> View<'T> -> (unit -> unit)
```

Equivalent to [`Sink`](#view_sink), but returns a function that, when called, stops the process.

---

```fsharp
val View.Sequence : seq<View<'T>> -> View<seq<'T>>
```

Collects the current values of a sequence of Views into a single View.

---

<a name="view_sink"></a>
```fsharp
val View.Sink : ('T -> unit) -> View<'T> -> unit
```

Starts a process that calls the given function repeatedly with the latest View value. This method is rarely needed, the most common way to use views is by constructing reactive documents of type `Doc`, and embedding them using `Doc.EmbedView`. `Sink` use requires a little care, the typical usage is to run it once per application. This is because the process created by `Sink` repeatedly blocks while waiting for the view to update. A memory leak can happen if the application repeatedly spawns `Sink` processes that never get collected because they await a Var that is never going to change (see Leaks for more information).

---

```fsharp
val View.SnapshotOn : 'U -> View<'T> -> View<'U> -> View<'U>
```

Given two views `a` and `b`, and a default value, provides a ‘snapshot’ of `b` whenever `a` updates. The value of `a` is unused. The initial value is an initial sample of `b`.

```fsharp
[[View.SnapshotOn init a b]] = init,                                   if [[a]] hasn't been updated yet
                             = [[b the last time [[a]] was updated]],  once [[a]] has been updated
```

This combinator is used as the base for the implementation of the [Submitter](#submitter), which is commonly used to include punctual events such as button clicks into the dataflow graph.

---

```fsharp
val View.UpdateWhile : 'T -> View<bool> -> View<'T> -> View<'T>
```

`View.UpdateWhile init cond v` creates a View whose initial value is `init`, which stays equal to `v` whenever `cond` is true, and keeps its value whenever `cond` is false. Using our notation:

```fsharp
[[View.UpdateWhile init cond v]] = init,                                     if [[cond]] has never been true yet
                                 = [[v]],                                    if [[cond]] is true
                                 = [[v the last time [[cond]] was true]],    if [[cond]] is false
```

<a name="iref"></a>
### IRef

`IRef` is an abstraction for gettable, settable and observable variables. The main implementation of this abstraction is [`Var`](#var).

#### Members of the interface `IRef<'T>`

```fsharp
member this.Get : unit -> 'T
```

Get the current value.

---

```fsharp
member this.Set : 'T -> unit
```

Set the current value.

---

```fsharp
member this.Value with get, set
```

Get or set the current value.

---

```fsharp
member this.Update : ('T -> 'T) -> unit
```

Update the value based on the current value.

---

```fsharp
member this.UpdateMaybe : ('T -> option<'T>) -> unit
```

Maybe update the value based on the current value; do nothing if the function returns `None`

---

```fsharp
member this.View : View<'T>
```

Get the [View](#view) that dynamically observes this.

---

```fsharp
member this.Id : string
```

Gets the unique ID associated with the var.

#### Extension methods

```fsharp
member this.Lens : get:('T -> 'U) -> update:('T -> 'U -> 'T) -> IRef<'U>
```

Create a new IRef which lenses onto a part of this.

When getting the IRef's value, `get` is called on the value of this. When setting the IRef's value, `update` is called with the current value of this and the new value, and the result is set as the new value of this.

<!-- TODO: Builder -->

<a name="submitter"></a>
### Submitter

`Submitter<'T>` is a special kind of input node in the Dataflow layer. Its purpose is to allow punctual events such as, typically, button clicks, to participate in the graph.

The purpose of a submitter is to provide a [View](#view) which gets its value from a given input view, but only gets updated when punctual events are [`Trigger`](#submitter_trigger)ed.

#### Members of the type `Submitter<'T>` and module `Submitter`

<a name="submitter_create"></a>
```fsharp
val Submitter.Create : View<'T> -> 'T -> Submitter<'T>
```

`Create v init` creates a submitter for the given input View. The initial value of the submitter’s output [`View`](#submitter_view) is `init`. Then, every time [`Trigger`](#submitter_trigger) is called, the value of the output View is updated to be the current value of input.

---

```fsharp
val Submitter.CreateDefault : View<'T> -> Submitter<'T>
```

`CreateDefault v` is equivalent to `Create v Unchecked.defaultof<'T>`.

---

```fsharp
val Submitter.CreateOption : View<'T> -> Submitter<option<'T>>
```

[Creates](#submitter_create) a Submitter with initial value `None`, and [`Trigger`](#submitter_trigger)ed values mapped to `Some`.

---

```fsharp
val Submitter.Input : Submitter<'T> -> View<'T>
member subm.Input : View<'T>
```

Get the input View of a Submitter, ie the View that was passed to [`Create`](#submitter_create).

---

<a name="submitter_trigger"></a>
```fsharp
val Submitter.Trigger : Submitter<'T> -> unit
member subm.Trigger : unit -> unit
```

Triggers the Submitter, causing its output [`View`](#submitter_view) to get the current value of its input View, and keep this value until the next call to `Trigger`.

---

<a name="submitter_view"></a>
```fsharp
val Submitter.View : Submitter<'T> -> View<'T>
member subm.View : View<'T>
```

Get the output View of the Submitter.

### Model

### ListModel
