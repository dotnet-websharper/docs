# UI.Next - Functional Reactive Programming and HTML

## Dataflow

<a name="var"></a>
### Var

Reactive variables lay at the foundation of the Dataflow layer. They can be created and manipulated just like regular `ref` cells. Additionally, variables can be lifted to the [View](#view) type to participate in the dataflow graph.

---

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

```
val Var.Lens : IRef<'T> -> get:('T -> 'V) -> update:('T -> 'V -> 'T) -> IRef<'V>
member this.Lens : get:('T -> 'V) -> update:('T -> 'V -> 'T) -> IRef<'V>
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

#### `type View<'T>`


<a name="view_bind"></a>
```fsharp
val View.Bind : ('T -> View<'U>) -> View<'T> -> View<'U>
member this.Bind : ('T -> View<'U>) -> View<'U>
```

Create a View which depends entirely on the current value of this, such that `[[View.Bind f v]] = [[ f [[v]] ]]`.

Dynamic composition via `View.Bind` and `View.Join` should be used with some care. Whenever static composition (such as `View.Map2`) can do the trick, it should be preferable. One concern here is efficiency, and another is state, identity and sharing (see Sharing for a discussion).

---

```fsharp
val View.BindInner : ('T -> View<'U>) -> View<'T> -> View<'U>
member this.BindInner : ('T -> View<'U>) -> View<'U>
```

Equivalent to `Bind`, but additionally, obsoletes the inner result whenever this changes. This can avoid leaks when a new `View` is created on every call to the binding function.

---

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

```fsharp
val View.RemovableSink : ('T -> unit) -> View<'T> -> (unit -> unit)
```

Equivalent to [`Sink`](#view_sink), but returns a function that, when called, stops the process.

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

<a name="iref"></a>
### IRef
