---
order: -60
expanded: true
label: ListModels
---
# ListModels

`ListModel<'K, 'T>` is a convenient type to store an observable collection of items of type `'T`. Items can be accessed using an identifier, or key, of type `'K`.

ListModels are to dictionaries as Vars are to refs: a type with similar capabilities, but with the additional capability to be reactively observed, and therefore to have your UI automatically change according to changes in the stored content.

---

## Creating ListModels

You can create ListModels with the following functions:

---

### ListModel.FromSeq

Creates a ListModel where items are their own key.

```fsharp
let myNames = ListModel.FromSeq ["John"; "Ana"]
```

---

### ListModel.Create

Creates a ListModel using a given function to determine the key of an item.

```fsharp
type Person = { Username: string; Name: string }

let myPeople =
    ListModel.Create (fun p -> p.Username)
        [
            { Username = "johnny87"; Name = "John" }
            { Username = "theana12"; Name = "Ana" }
        ]
```

(The examples on this page use the above `Person` type and `myPeople` model.)

---

## Modifying ListModels

Once you have a ListModel, you can modify its contents in a number of ways, see below.

---

### listModel.Add

Inserts an item into the model. If there is already an item with the same key, this item is replaced.

```fsharp
myPeople.Add { Username = "mynameissam"; Name = "Sam" }
// myPeople now contains John, Ana and Sam.

myPeople.Add { Username = "johnny87"; Name = "Johnny" }
// myPeople now contains Johnny, Ana and Sam.
```

---

### listModel.RemoveByKey

Removes the item from the model that has the given key. If there is no such item, then nothing happens.

```fsharp
myPeople.RemoveByKey "theana12"
// myPeople now contains John.

myPeople.RemoveByKey "chloe94"
// myPeople now contains John.
```

---

### listModel.Remove

Removes the item from the model that has the same key as the given item. It is effectively equivalent to `listModel.RemoveByKey(getKey x)`, where `getKey` is the key function passed to `ListModel.Create` and `x` is the argument to `Remove`.

```fsharp
myPeople.Remove { Username = "theana12"; Name = "Another Ana" }
// myPeople now contains John.
```

---

### listModel.Set

Sets the entire contents of the model, discarding the previous contents.

```fsharp
myPeople.Set [
    { Username = "chloe94"; Name = "Chloe" };
    { Username = "a13x"; Name = "Alex" }
]
// myPeople now contains Chloe, Alex.
```

---

### listModel.Clear

Removes all items from the model.

```fsharp
myPeople.Clear()
// myPeople now contains no items.
```

---

### listModel.UpdateBy

Updates the item with the given key. If the function returns None or the item is not found, nothing is done.

```fsharp
myPeople.UpdateBy (fun u -> Some { u with Name = "The Real Ana" }) "theana12"
// myPeople now contains John, The Real Ana.

myPeople.UpdateBy (fun u -> None) "johnny87"
// myPeople now contains John, The Real Ana.
```

---

### listModel.UpdateAll

Updates all the items of the model. If the function returns None, the corresponding item is unchanged.

```fsharp
myPeople.UpdateAll (fun u -> 
    if u.Username.Contains "ana" then
        Some { u with Name = "The Real Ana" }
    else
        None)
// myPeople now contains John, The Real Ana.
```

---

### listModel.Lens

Creates an `Var<'T>` that does not have its own separate storage, but is bound to the value for a given key.

```fsharp
let john : Var<Person> = myPeople.Lens "johnny87"
```

---

### listModel.LensInto

Creates an `Var<'T>` that does not have its own separate storage, but is bound to a part of the value for a given key.

```fsharp
let varJohnsName : Var<string> =
    myPeople.LensInto "johnny87" (fun p -> p.Name) (fun p n -> { p with Name = n })

// The following input field edits John's name directly in the listModel.
let editJohnsName = Doc.Input [] varJohnsName
```

---

## Reactively observing ListModels

The main purpose for using a ListModel is to be able to reactively observe it.

---

### listModel.View

Gives a `View<seq<'T>>` that reacts to changes to the model.

The following example creates an HTML list of people which is automatically updated based on the contents of the model.

```fsharp
open WebSharper.UI.Client
open WebSharper.UI.Html

let myPeopleList =
    myPeople.View
    |> Doc.BindView (fun people ->
        ul [] [
            people
            |> Seq.map (fun p -> li [] [text p.Name])
            |> Doc.Concat
        ]
    )
```

---

### listModel.ViewState

Is equivalent to `View`, except that it returns a `View<ListModelState<'T>>`. Here are the differences:

* `ViewState` provides better performance.
* `ListModelState<'T>` implements `seq<'T>`, but it additionally provides indexing and length of the sequence.
* However, a `ViewState` is only valid until the next change to the model.

As a summary, it is generally better to use `ViewState`. You only need to choose `View` if you need to store the resulting `seq` separately.

---

### listModel.Map

Reactively maps a function on each item. It is similar to [the `View.MapSeqCached` family of functions](/ui/views/#mapping-views-on-sequences): it is optimized so that the mapping function is not called again on every item when the content changes, but only on changed items. There are two variants:

#### Map(f: 'T -> 'V)

Assumes that the item with a given key does not change. It is equivalent to `View.MapSeqCachedBy` using the ListModel's key function.

```fsharp
let myDoc =
    myPeople.Map(fun p ->
        Console.Log p.Username
        p [] [text p.Name]
    )
    |> Doc.BindView Doc.Concat
    |> Doc.RunAppend JS.Document.Body
// Logs johnny87, theana12
// Displays John, Ana

// We add an item with a key that doesn't exist yet,
// so the mapping function is called for it and the result is added.
myPeople.Add { Username = "mynameissam"; Name = "Sam" }
// Logs mynameissam
// Displays John, Ana, Sam

// We change the value for an existing key,
// so this change is ignored by Map.
myPeople.Add { Username = "johnny87"; Name = "Johnny" }
// Logs nothing, since no key has been added
// Displays John, Ana, Sam (unchanged)
```

---

#### Map(f: 'K -> View<'T> -> 'V)

Additionally observes changes to individual items that are updated. It is equivalent to `View.MapSeqCachedViewBy` using the ListModel's key function.

```fsharp
myPeople.Map(fun k vp ->
    Console.Log k
    p [] [text (vp.V.Name)]
)
|> Doc.BindView Doc.Concat
|> Doc.RunAppend JS.Document.Body
// Logs johnny87, theana12
// Displays John, Ana

// We add an item with a key that doesn't exist yet,
// so the mapping function is called for it and the result is added.
myPeople.Add { Username = "mynameissam"; Name = "Sam" }
// Logs mynameissam
// Displays John, Ana, Sam

// We change the value for an existing key,
// so the mapping function is not called again
// but the View's value is updated.
myPeople.Add { Username = "johnny87"; Name = "Johnny" }
// Here we changed the value for an existing key
// Logs nothing, since no key has been added
// Displays Johnny, Ana, Sam (changed!)
```

Note that in both cases, only the current state is kept in memory: if you remove an item and insert it again, the function will be called again.

---

### listModel.MapLens

Is similar to the second `Map` method above, except that it passes an `Var<'T>` instead of a `View<'T>`. This makes it possible to edit list items within the mapping function.

```fsharp
let myDoc =
    myPeople.MapLens(fun k vp ->
        label [] [
            text (vp.V.Username + ": ")
            Doc.InputV [] vp.V.Name
        ]
    )
    |> Doc.BindView Doc.Concat
```

---

### listModel.Doc

Is similar to `Map`, but the function must return a `Doc` and the resulting Docs are concatenated. It is similar to [the `Doc.BindSeqCached` family of functions](/ui/views/#mapping-views-on-sequences).

---

### listModel.DocLens

Like `MapLens` but concatenating the resulting Docs.

---

### listModel.TryFindByKeyAsView

Gives a View on the item that has the given key, or `None` if it is absent.

```fsharp
let showJohn =
    myPeople.TryFindByKeyAsView("johnny87")
    |> Doc.BindView (function
        | None -> text "He is not here."
        | Some u -> text (sprintf "He is here, and his name is %s." u.Name)
    )
```

---

### listModel.FindByKeyAsView

Is equivalent to `TryFindByKeyAsView`, except that when there is no item with the given key, an exception is thrown.

---

### listModel.ContainsKeyAsView

Gives a View on whether there is an item with the given key. It is equivalent to (but more optimized than):

```fsharp
View.Map Option.isSome (listModel.TryFindByKeyAsView(k))
```
