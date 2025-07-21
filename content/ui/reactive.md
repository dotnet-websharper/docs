---
title: Reactive programming
---

WebSharper.UI's reactive layer helps represent user inputs and other time-varying values, and define how they depend on one another.

### Vars

Reactive values that are directly set by code or by user interaction are represented by values of type `Var<'T>`. Vars are similar to F# `ref<'T>` in that they store a value of type `'T` that you can get or set using the `Value` property. But they can additionally be reactively observed or two-way bound to HTML input elements.

The following are available from `WebSharper.UI.Client`:

* `Doc.InputType.Text` creates an `<input>` element with given attributes that is bound to a `Var<string>`.

    ```fsharp
    let varText = Var.Create "initial value"
    let myInput = Doc.InputType.Text [ attr.name "my-input" ] varText
    ```
    
    With the above code, once `myInput` has been inserted in the document, getting `varText.Value` will at any point reflect what the user has entered, and setting it will edit the input.

* `Doc.InputType.Int` and `Doc.InputType.Float` create an `<input type="number">` bound to a `Var<CheckedInput<_>>` of the corresponding type (`int` or `float`). `CheckedInput` provides access to the validity and actual user input, it is defined as follows:

    ```fsharp
    type CheckedInput<'T> =
        | Valid of value: 'T * inputText: string
        | Invalid of inputText: string
        | Blank of inputText: string
    ```

* `Doc.InputType.IntUnchecked` and `Doc.InputType.FloatUnchecked` create an `<input type="number">` bound to a `Var<_>` of the corresponding type (`int` or `float`). They do not check for the validity of the user's input, which can cause wonky interactions. We recommend using `Doc.InputType.Int` or `Doc.InputType.Float` instead.

* `Doc.InputType.TextArea` creates a `<textarea>` element bound to a `Var<string>`.

* `Doc.InputType.Password` creates an `<input type="password">` element bound to a `Var<string>`.

* `Doc.InputType.CheckBox` creates an `<input type="checkbox">` element bound to a `Var<bool>`.

* `Doc.InputType.CheckBoxGroup` also creates an `<input type="checkbox">`, but instead of associating it with a simple `Var<bool>`, it associates it with a specific `'T` in a `Var<list<'T>>`. If the box is checked, then the element is added to the list, otherwise it is removed.

    ```fsharp
    type Color = Red | Green | Blue
    
    // Initially, Green and Blue are checked.
    let varColor = Var.Create [ Blue; Green ]
    
    let mySelector =
        div [] [
            label [] [
                Doc.InputType.CheckBoxGroup [] Red varColor
                text " Select Red"
            ]
            label [] [
                Doc.InputType.CheckBoxGroup [] Green varColor
                text " Select Green"
            ]
            label [] [
                Doc.InputType.CheckBoxGroup [] Blue varColor
                text " Select Blue"
            ]
        ]
    ```
        
    Result:
    
    ```html
    <div>
      <label><input type="checkbox" /> Select Red</label>
      <label><input type="checkbox" checked /> Select Green</label>
      <label><input type="checkbox" checked /> Select Blue</label>
    </div>
    ```

    Plus varColor is bound to contain the list of ticked checkboxes.

* `Doc.InputType.Select` creates a dropdown `<select>` given a list of values to select from. The label of every `<option>` is determined by the given print function for the associated value.

    ```fsharp
    type Color = Red | Green | Blue

    // Initially, Green is checked.
    let varColor = Var.Create Green

    // Choose the text of the dropdown's options.
    let showColor (c: Color) =
        sprintf "%A" c

    let mySelector =
        Doc.InputType.Select [] showColor [ Red; Green; Blue ] varColor
    ```
        
    Result:
    
    ```html
    <select>
      <option>Red</option>
      <option>Green</option>
      <option>Blue</option>
    </select>
    ```

    Plus varColor is bound to contain the selected color.

* `Doc.InputType.SelectDyn` is similar to `Doc.InputType.Select`, but it takes a `View<list<'T>>` instead of a static list of values. This means that the dropdown can change dynamically based on the contents of the View.

* `Doc.InputType.SelectOptional` and `Doc.InputType.SelectDynOptional` allow for selecting no values.

* `Doc.InputType.SelectMultiple` and `Doc.InputType.SelectMultipleDyn` allow for selecting multiple values.

* `Doc.InputType.Radio` creates an `<input type="radio">` given a value, which sets the given `Var` to that value when it is selected.

    ```fsharp
    type Color = Red | Green | Blue
    
    // Initially, Green is selected.
    let varColor = Var.Create Green
    
    let mySelector =
        div [] [
            label [] [
                Doc.InputType.Radio [] Red varColor
                text " Select Red"
            ]
            label [] [
                Doc.InputType.Radio [] Green varColor
                text " Select Green"
            ]
            label [] [
                Doc.InputType.Radio [] Blue varColor
                text " Select Blue"
            ]
        ]
    ```
        
    Result:
    
    ```html
    <div>
      <label><input type="radio" /> Select Red</label>
      <label><input type="radio" checked /> Select Green</label>
      <label><input type="radio" /> Select Blue</label>
    </div>
    ```

    Plus varColor is bound to contain the selected color.

More variants available in the `Doc.InputType` module are: `Color`, `Date`, `DateTimeLocal`, `Email`, `File`, `Month`, `Range`, `Search`, `Tel`, `Time`, `Url`, `Week`, .
These all correspond to the HTML `<input>` element of the same type.

### Views

The full power of WebSharper.UI's reactive layer comes with `View`s. A `View<'T>` is a time-varying value computed from Vars and from other Views. At any point in time the view has a certain value of type `'T`.

One thing important to note is that the value of a View is not computed unless it is needed. For example, if you use `View.Map`, the function passed to it will only be called if the result is needed. It will only be run while the resulting View is included in the document using [one of these methods](#inserting-views-in-the-doc). This means that you generally don't have to worry about expensive computations being performed unnecessarily. However it also means that you should avoid relying on side-effects performed in functions like `View.Map`.

In pseudo-code below, `[[x]]` notation is used to denote the value of the View `x` at every point in time, so that `[[x]]` = `[[y]]` means that the two views `x` and `y` are observationally equivalent.

Note that several of the functions below can be used more concisely using [the V shorthand](#the-v-shorthand).

#### Creating and combining Views

The first and main way to get a View is using the `View` property of `Var<'T>`. This retrieves a View that tracks the current value of the Var.

You can create Views using the following functions and combinators from the `View` module:

* `View.Const` creates a View whose value is always the same.

    ```fsharp
    let v = View.Const 42

    // [[v]] = 42
    ```

* `View.ConstAnyc` is similar to `Const`, but is initialized asynchronously. Until the async returns, the resulting View is uninitialized.

* <a name="view-map"></a>`View.Map` takes an existing View and maps its value through a function.

    ```fsharp
    let v1 : View<string> = // ...
    let v2 = View.Map (fun s -> String.length s) v1

    // [[v2]] = String.length [[v1]]
    ```

* `View.Map2` takes two existing Views and map their value through a function.

    ```fsharp
    let v1 : View<int> = // ...
    let v2 : View<int> = // ...
    let v3 = View.Map2 (fun x y -> x + y) v1 v2

    // [[v3]] = [[v1]] + [[v2]]
    ```

    Similarly, `View.Map3` takes three existing Views and map their value through a function.

* `View.MapAsync` is similar to `View.Map` but maps through an asynchronous function.

    An important property here is that this combinator saves work by abandoning requests. That is, if the input view changes faster than we can asynchronously convert it, the output view will not propagate change until it obtains a valid latest value. In such a system, intermediate results are thus discarded.
    
    Similarly, `View.MapAsync2` maps two existing Views through an asynchronous function.

* `View.Apply` takes a View of a function and a View of its argument type, and combines them to create a View of its return type.

    While Views of functions may seem like a rare occurrence, they are actually useful together with `View.Const` in a pattern that can lift a function of any number N of arguments into an equivalent of `View.MapN`.

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

<a name="view-doc"></a>
#### Inserting Views in the Doc

Once you have created a View to represent your dynamic content, here are the various ways to include it in a Doc:

* `textView` is a reactive counterpart to `text`, which creates a text node from a `View<string>`.

    ```fsharp
    let varTxt = Var.Create ""
    let vLength =
        varTxt.View
        |> View.Map String.length
        |> View.Map (fun l -> sprintf "You entered %i characters." l)
    div [] [
        Doc.InputType.Text [] varTxt
        textView vLength
    ]
    ```

* `Doc.BindView` maps a View into a dynamic Doc.

    ```fsharp
    let varTxt = Var.Create ""
    let vWords =
        varTxt.View
        |> View.Map (fun s -> s.Split(' '))
        |> Doc.BindView (fun words ->
            words
            |> Array.map (fun w -> li [] [text w] :> Doc)
            |> Doc.Concat
        )
    div [] [
        Doc.InputType.Text [] varTxt
        text "You entered the following words:"
        ul [] [ vWords ]
    ]
    ```

* `Doc.EmbedView` unwraps a `View<Doc>` into a Doc. It is equivalent to `Doc.BindView id`.

* `attr.*Dyn` is a reactive equivalent to the corresponding `attr.*`, creating an attribute from a `View<string>`.

    For example, the following sets the background of the input element based on the user input value:

    ```fsharp
    let varTxt = Var.Create ""
    let vStyle =
        varTxt.View
        |> View.Map (fun s -> "background-color: " + s)
    Doc.InputType.Text [ attr.styleDyn vStyle ] varTxt
    ```

* `attr.*DynPred` is similar to `attr.*Dyn`, but it takes an extra `View<bool>`. When this View is true, the attribute is set (and dynamically updated as with `attr.*Dyn`), and when it is false, the attribute is removed.

    ```fsharp
    let varTxt = Var.Create ""
    let varCheck = Var.Create true
    let vStyle =
        varTxt.View
        |> View.Map (fun s -> "background-color: " + s)
    div [] [
        Doc.InputType.Text [ attr.styleDynPred vStyle varCheck.View ] varTxt
        Doc.CheckBox [] varCheck
    ]
    ```

<a name="view-seqcached"></a>
#### Mapping Views on sequences

Applications often deal with varying collections of data. This means using a View of a sequence: a value of type `View<seq<T>>`, `View<list<T>>` or `View<T[]>`. In this situation, it can be sub-optimal to use `Map` or `Doc` to render it: the whole sequence will be re-computed even when a single item has changed.

The `SeqCached` family of functions fixes this issue. These functions map a View of a sequence to either a new `View<seq<U>>` (functions `View.MapSeqCached*` and method `.MapSeqCached()`) or to a `Doc` (functions `Doc.BindSeqCached` and method `.DocSeqCached()`) but avoid re-mapping items that haven't changed.

There are different versions of these functions, which differ in how they decide that an item "hasn't changed".

* `View.MapSeqCached : ('T -> 'V) -> View<seq<'T>> -> View<seq<'V>>` uses standard F# equality to check items.

    ```fsharp
    let varNums = Var.Create [ 1; 2; 3 ]

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
    
    varNums.Value <- [ 1; 2; 3; 4 ]
    // Prints 4
    // Displays 1, 2, 3, 4
    // Note: the existing <p> tags remain, they aren't recreated.
    
    varNums.Value <- [ 3; 2 ]
    // Prints nothing
    // Displays 3, 2
    ```

* `View.MapSeqCachedBy : ('T -> 'K) -> ('T -> 'V) -> View<seq<'T>> -> View<seq<'V>>` uses the given key function to check items. This means that if an item is added whose key is already present, the corresponding returned item is _not_ changed. So you should only use this when items are intended to be added or removed, but not changed.

    ```fsharp
    type Person = { Id: int; Name: string: int }

    let ann = { Id = 0; Name = "Ann" }
    let brian = { Id = 1; Name = "Brian" }
    let bobby = { Id = 1; Name = "Bobby" }
    let clara = { Id = 2; Name = "Clara" }
    let dave = { Id = 3; Name = "Dave" }

    let varPeople =
        Var.Create [ ann; brian; clara ]

    varPeople.View
    |> View.MapSeqCachedBy (fun p -> p.Id) (fun p -> 
        Console.Log p.Id
        p [] [ text (string p.Name) ]
    )
    |> Doc.BindView Doc.Concat
    |> Doc.RunAppend JS.Document.Body
    // Prints 1, 2, 3
    // Displays Ann, Brian, Clara
    
    varPeople.Value <- [ ann; brian; clara; dave ]
    // Prints 4
    // Displays Ann, Brian, Clara, Dave
    // Note: the existing <p> tags remain, they aren't recreated.
    
    varPeople.Value <- [ ann; bobby; clara; dave ]
    // Prints nothing
    // Displays Ann, Brian, Clara, Dave
    // The item with Id = 1 is already rendered as Brian,
    // so it is not re-rendered as Bobby.
    ```

* `View.MapSeqCachedViewBy : ('T -> 'K) -> ('K -> View<'T> -> 'V) -> View<seq<'V>> covers the situation where items are identified by a key function and can be updated. Instead of passing the item's value to the mapping function, it passes a View of it, so you can react to the changes.

    ```fsharp
    varPeople.View
    |> View.MapSeqCachedViewBy (fun p -> p.Id) (fun pid vp -> 
        Console.Log pid
        p [] [ textView (vp |> View.Map (fun p -> string p.Name)) ]
    )
    |> Doc.BindView Doc.Concat
    |> Doc.RunAppend JS.Document.Body
    // Prints 1, 2, 3
    // Displays Ann, Brian, Clara
    
    varPeople.Value <- [ ann; brian; clara; dave ]
    // Prints 4
    // Displays Ann, Brian, Clara, Dave
    // Note: the existing <p> tags remain, they aren't recreated.
    
    varPeople.Value <- [ ann; bobby; clara; dave ]
    // Prints nothing
    // Displays Ann, Bobby, Clara, Dave
    // The item with Id = 1 is already rendered as Brian,
    // so its <p> tag remains but its text content changes.
    ```

Each of these `View.MapSeqCached*` functions has a corresponding `Doc.BindSeqCached*`:

* `Doc.BindSeqCached : ('T -> #Doc) -> View<seq<'T>> -> Doc`
* `Doc.BindSeqCachedBy : ('T -> 'K) -> ('T -> #Doc) -> View<seq<'T>> -> Doc`
* `Doc.BindSeqCachedViewBy : ('T -> 'K) -> ('K -> View<'T> -> #Doc) -> View<seq<'T>> -> Doc`

These functions map each item of the sequence to a Doc and then concatenates them. They are basically equivalent to passing the result of the corresponding `View.MapSeqCached*` to `Doc.BindView Doc.Concat`, like we did in the examples above.

Finally, all of the above functions are also available as extension methods on the `View<seq<'T>>` type. `.MapSeqCached()` overloads correspond to `View.MapSeqCached*` functions, and `.DocSeqCached()` overloads correspond to `Doc.BindSeqCached*` functions.

<a name="lens"></a>
### Vars and lensing

The `Var<'T>` type is actually an abstract class, this makes it possible to create instances with an implementation different from `Var.Create`. The main example of this are **lenses**.

In WebSharper.UI, a lens is a `Var` without its own storage cell that "focuses" on a sub-part of an existing `Var`. For example, given the following:

```fsharp
type Person = { FirstName : string; LastName : string }
let varPerson = Var.Create { FirstName = "John"; LastName = "Doe" }
```

You might want to create a form that allows entering the first and last name separately. For this, you need two `Var<string>`s that directly observe and alter the `FirstName` and `LastName` fields of the value stored in `varPerson`. This is exactly what a lens does.

To create a lens, you need to pass a getter and a setter function. The getter is called when the lens needs to know its current value, and extracts it from the parent `Var`'s current value. The setter is called when setting the value of the lens; it receives the current value of the parent `Var` and the new value of the lens, and returns the new value of the parent `Var`.

```fsharp
let varFirstName = varPerson.Lens (fun p -> p.FirstName)
                                  (fun p n -> { p with FirstName = n })
let varLastName = varPerson.Lens (fun p -> p.LastName)
                                 (fun p n -> { p with LastName = n })
let myForm =
    div [] [
        Doc.InputType.Text [ attr.placeholder "First Name" ] varFirstName
        Doc.InputType.Text [ attr.placeholder "Last Name" ] varLastName
    ]
```

#### Automatic lenses

In the specific case of records, you can use `LensAuto` to create lenses more concisely. This method only takes the getter, and is able to generate the corresponding setter during compilation.

```fsharp
let varFirstName = varPerson.LensAuto (fun p -> p.FirstName)

// The above is equivalent to:
let varFirstName = varPerson.Lens (fun p -> p.FirstName)
                                  (fun p n -> { p with FirstName = n })
```

You can be even more concise when using `Doc.InputType.Text` and family thanks to [the V shorthand](#the-v-shorthand).

<a name="v"></a>
### The V Shorthand

Mapping reactive values from their model to a value that you want to display can be greatly simplified using the V shorthand. This shorthand revolves around passing calls to the property `view.V` to a number of supporting functions.

#### Views and V

When an expression containing a call to `view.V` is passed as argument to one of the supporting functions, it is converted to a call to `View.Map` on this view, and the resulting expression is used in a way relevant to the supporting function.

The simplest supporting function is called `V`, and it simply returns the view expression.

```fsharp
type Person = { FirstName: string; LastName: string }

let vPerson : View<Person> = // ...

let vFirstName = V(vPerson.V.FirstName)

// The above is equivalent to:
let vFirstName = vPerson |> View.Map (fun p -> p.FirstName)
```

You can use arbitrarily complex expressions:

```fsharp
let vFullName = V(vPerson.V.FirstName + " " + vPerson.V.LastName)

// The above is equivalent to:
let vFirstName = vPerson |> View.Map (fun p -> p.FirstName + " " + p.LastName)
```

Other supporting functions use the resulting View in different ways:

* `text` passes the resulting View to `textView`.

    ```fsharp
    let showName : Doc = text (vPerson.V.FirstName + " " + vPerson.V.LastName)

    // The above is equivalent to:
    let showName = 
        textView (
            vPerson
            |> View.Map (fun p -> p.V.FirstName + " " + p.V.LastName)
        )
    ```

* `attr.*` attribute creation functions pass the resulting View to the corresponding `attr.*Dyn`.

    ```fsharp
    type ImgData = { Src: string; Height: int }
    
    let myImgData = Var.Create { Src = "/my-img.png"; Height = 200 }
    
    let myImg =
        img [
            attr.src (myImgData.V.Src)
            attr.height (string myImgData.V.Height)
        ] []

    // The above is equivalent to:
    let myImg =
        img [
            attr.srcDyn (myImgData.View |> View.Map (fun i -> i.Src))
            attr.heightDyn (myImgData.View |> View.Map (fun i -> string i.Height))
        ] []
    ```

* `Attr.Style` passes the resulting View to `Attr.DynamicStyle`.

    ```fsharp
    type MyStyle = { BgColor: string; Width: int }
    
    let myStyle = Var.Create { BgColor = "orangered"; Width = 400 }
    
    let myElt =
        div [
            Attr.Style "background-color" myStyle.V.BgColor
            Attr.Style "width" (sprintf "%ipx" myStyle.V.Width)
        ] [ text "This is my elt" ]

    // The above is equivalent to:
    let myElt =
        div [
            Attr.DynamicStyle "background-color"
                (myStyle |> View.Map (fun s -> s.BgColor))
            Attr.DynamicStyle "width"
                (myStyle |> View.Map (fun s -> sprintf "%ipx" s.Width))
        ] [ text "This is my elt" ]
    ```

Calling `.V` outside of one of the above supporting functions is a compile error. There is one exception: if `view` is a `View<Doc>`, then `view.V` is equivalent to `Doc.EmbedView view`.

```fsharp
let varPerson = Var.Create (Some { FirstName = "John"; LastName = "Doe" })

let vMyDoc = V(
    match varPerson.V with
    | None -> Doc.Empty
    | Some p -> div [] [ text varPerson.V.FirstName ]
)
let myDoc = vMyDoc.V

// The above is equivalent to:
let vMyDoc =
    varPerson.View |> View.Map (fun p ->
        match p with
        | None -> Doc.Empty
        | Some p -> div [] [ text p.FirstName ]
    )
let myDoc = Doc.EmbedView vMyDoc
```

#### Vars and V

Vars also have a `.V` property. When used with one of the above supporting functions, it is equivalent to `.View.V`.

```fsharp
let varPerson = Var.Create { FirstName = "John"; LastName = "Doe" }

let vFirstName = V(varPerson.V.FirstName)

// The above is equivalent to:
let vFirstName = V(varPerson.View.V.FirstName)

// Which is also equivalent to:
let vFirstName = varPerson.View |> View.Map (fun p -> p.FirstName)
```

Additionally, `var.V` can be used as a shorthand for [lenses](#vars-and-lensing). `.V` is a shorthand for `.LensAuto` when passed to the following supporting functions:

* `Lens` simply creates a lensed Var.

    ```fsharp
    type Person = { FirstName : string; LastName : string }
    let varPerson = Var.Create { FirstName = "John"; LastName = "Doe" }

    let myForm =
        div [] [
            Doc.InputType.Text [ attr.placeholder "First Name" ] (Lens varPerson.V.FirstName)
            Doc.InputType.Text [ attr.placeholder "Last Name" ] (Lens varPerson.V.LastName)
        ]
    ```

* `Doc.InputType.TextV`, `Doc.InputType.TextAreaV`, `Doc.InputType.PasswordV`
* `Doc.InputType.IntV`, `Doc.InputType.IntUncheckedV`
* `Doc.InputType.FloatV`, `Doc.InputType.FloatUncheckedV`

```fsharp
type Person = { FirstName : string; LastName : string }
let varPerson = Var.Create { FirstName = "John"; LastName = "Doe" }

let myForm =
    div [] [
        Doc.InputType.TextV [ attr.placeholder "First Name" ] varPerson.V.FirstName
        Doc.InputType.TextV [ attr.placeholder "Last Name" ] varPerson.V.LastName
    ]

// The above is equivalent to:
let myForm =
    div [] [
        Doc.InputType.Text [ attr.placeholder "First Name" ]
            (varPerson.LensAuto (fun p -> p.FirstName))
        Doc.InputType.Text [ attr.placeholder "Last Name" ]
            (varPerson.LensAuto (fun p -> p.LastName))
    ]

// Which is equivalent to:
let myForm =
    div [] [
        Doc.InputType.Text [ attr.placeholder "First Name" ] 
            (varPerson.Lens (fun p -> p.FirstName) (fun p n -> { p with FirstName = n }))
        Doc.InputType.Text [ attr.placeholder "Last Name" ]
            (varPerson.Lens (fun p -> p.LastName) (fun p n -> { p with LastName = n }))
    ]
```

### ListModels

`ListModel<'K, 'T>` is a convenient type to store an observable collection of items of type `'T`. Items can be accessed using an identifier, or key, of type `'K`.

ListModels are to dictionaries as Vars are to refs: a type with similar capabilities, but with the additional capability to be reactively observed, and therefore to have your UI automatically change according to changes in the stored content.

#### Creating ListModels

You can create ListModels with the following functions:

* `ListModel.FromSeq` creates a ListModel where items are their own key.

    ```fsharp
    let myNameColl = ListModel.FromSeq [ "John"; "Ana" ]
    ```

* `ListModel.Create` creates a ListModel using a given function to determine the key of an item.

    ```fsharp
    type Person = { Username: string; Name: string }
    
    let myPeopleColl =
        ListModel.Create (fun p -> p.Username)
            [ { Username = "johnny87"; Name = "John" };
              { Username = "theana12"; Name = "Ana" } ]
    ```

Every following example will assume the above `Person` type and `myPeopleColl` model.

#### Modifying ListModels

Once you have a ListModel, you can modify its contents like so:

* `listModel.Add` inserts an item into the model. If there is already an item with the same key, this item is replaced.

    ```fsharp
    myPeopleColl.Add({ Username = "mynameissam"; Name = "Sam" })
    // myPeopleColl now contains John, Ana and Sam.
    
    myPeopleColl.Add({ Username = "johnny87"; Name = "Johnny" })
    // myPeopleColl now contains Johnny, Ana and Sam.
    ```

* `listModel.RemoveByKey` removes the item from the model that has the given key. If there is no such item, then nothing happens.

    ```fsharp
    myPeopleColl.RemoveByKey("theana12")
    // myPeopleColl now contains John.
    
    myPeopleColl.RemoveByKey("chloe94")
    // myPeopleColl now contains John.
    ```

* `listModel.Remove` removes the item from the model that has the same key as the given item. It is effectively equivalent to `listModel.RemoveByKey(getKey x)`, where `getKey` is the key function passed to `ListModel.Create` and `x` is the argument to `Remove`.

    ```fsharp
    myPeopleColl.Remove({ Username = "theana12"; Name = "Another Ana" })
    // myPeopleColl now contains John.
    ```

* `listModel.Set` sets the entire contents of the model, discarding the previous contents.

    ```fsharp
    myPeopleColl.Set([ { Username = "chloe94"; Name = "Chloe" };
                       { Username = "a13x"; Name = "Alex" } ])
    // myPeopleColl now contains Chloe, Alex.
    ```

* `listModel.Clear` removes all items from the model.

    ```fsharp
    myPeopleColl.Clear()
    // myPeopleColl now contains no items.
    ```

* `listModel.UpdateBy` updates the item with the given key. If the function returns None or the item is not found, nothing is done.

    ```fsharp
    myPeople.UpdateBy (fun u -> Some { u with Name = "The Real Ana" }) "theana12"
    // myPeopleColl now contains John, The Real Ana.
    
    myPeople.UpdateBy (fun u -> None) "johnny87"
    // myPeopleColl now contains John, The Real Ana.
    ```

* `listModel.UpdateAll` updates all the items of the model. If the function returns None, the corresponding item is unchanged.

    ```fsharp
    myPeople.UpdateAll (fun u -> 
        if u.Username.Contains "ana" then
            Some { u with Name = "The Real Ana" }
        else
            None)
    // myPeopleColl now contains John, The Real Ana.
    ```

* `listModel.Lens` creates an `Var<'T>` that does not have its own separate storage, but is bound to the value for a given key.

    ```fsharp
    let john : Var<Person> = myPeople.Lens "johnny87"
    ```

* `listModel.LensInto` creates an `Var<'T>` that does not have its own separate storage, but is bound to a part of the value for a given key. See [lenses](#vars-and-lensing) for more information.

    ```fsharp
    let varJohnsName : Var<string> =
        myPeople.LensInto "johnny87" (fun p -> p.Name) (fun p n -> { p with Name = n })

    // The following input field edits John's name directly in the listModel.
    let editJohnsName = Doc.InputType.Text [] varJohnsName
    ```

#### Reactively observing ListModels

The main purpose for using a ListModel is to be able to reactively observe it. Here are the ways to do so:

* `listModel.View` gives a `View<seq<'T>>` that reacts to changes to the model. The following example creates an HTML list of people which is automatically updated based on the contents of the model.

    ```fsharp
    let myPeopleList =
        myPeopleColl.View
        |> Doc.BindView (fun people ->
            ul [] [
                people
                |> Seq.map (fun p -> li [] [ text p.Name ] :> Doc)
                |> Doc.Concat
            ] :> Doc
        )
    ```

* `listModel.ViewState` is equivalent to `View`, except that it returns a `View<ListModelState<'T>>`. Here are the differences:

    * `ViewState` provides better performance.
    * `ListModelState<'T>` implements `seq<'T>`, but it additionally provides indexing and length of the sequence.
    * However, a `ViewState` is only valid until the next change to the model.
    
    As a summary, it is generally better to use `ViewState`. You only need to choose `View` if you need to store the resulting `seq` separately.

* `listModel.Map` reactively maps a function on each item. It is similar to [the `View.MapSeqCached` family of functions](#mapping-views-on-sequences): it is optimized so that the mapping function is not called again on every item when the content changes, but only on changed items. There are two variants:

    * `Map(f: 'T -> 'V)` assumes that the item with a given key does not change. It is equivalent to `View.MapSeqCachedBy` using the ListModel's key function.
    
        ```fsharp
        let myDoc =
            myPeopleColl.Map(fun p ->
                Console.Log p.Username
                p [] [ text p.Name ]
            )
            |> Doc.BindView Doc.Concat
            |> Doc.RunAppend JS.Document.Body
        // Logs johnny87, theana12
        // Displays John, Ana
        
        // We add an item with a key that doesn't exist yet,
        // so the mapping function is called for it and the result is added.
        myPeopleColl.Add({ Username = "mynameissam"; Name = "Sam" })
        // Logs mynameissam
        // Displays John, Ana, Sam
        
        // We change the value for an existing key,
        // so this change is ignored by Map.
        myPeopleColl.Add({ Username = "johnny87"; Name = "Johnny" })
        // Logs nothing, since no key has been added
        // Displays John, Ana, Sam (unchanged)
        ```

    * `Map(f: 'K -> View<'T> -> 'V)` additionally observes changes to individual items that are updated. It is equivalent to `View.MapSeqCachedViewBy` using the ListModel's key function.
    
        ```fsharp
        myPeopleColl.Map(fun k vp ->
            Console.Log k
            p [] [ text (vp.V.Name) ]
        )
        |> Doc.BindView Doc.Concat
        |> Doc.RunAppend JS.Document.Body
        // Logs johnny87, theana12
        // Displays John, Ana
        
        // We add an item with a key that doesn't exist yet,
        // so the mapping function is called for it and the result is added.
        myPeopleColl.Add({ Username = "mynameissam"; Name = "Sam" })
        // Logs mynameissam
        // Displays John, Ana, Sam

        // We change the value for an existing key,
        // so the mapping function is not called again
        // but the View's value is updated.
        myPeopleColl.Add({ Username = "johnny87"; Name = "Johnny" })
        // Here we changed the value for an existing key
        // Logs nothing, since no key has been added
        // Displays Johnny, Ana, Sam (changed!)
        ```

    Note that in both cases, only the current state is kept in memory: if you remove an item and insert it again, the function will be called again.

* `listModel.MapLens` is similar to the second `Map` method above, except that it passes an `Var<'T>` instead of a `View<'T>`. This makes it possible to edit list items within the mapping function.

    ```fsharp
        let myDoc =
            myPeopleColl.MapLens(fun k vp ->
                label [] [
                    text (vp.V.Username + ": ")
                    Doc.InputV [] vp.V.Name
                ]
            )
            |> Doc.BindView Doc.Concat
    ```

* `listModel.Doc` is similar to `Map`, but the function must return a `Doc` and the resulting Docs are concatenated. It is similar to [the `Doc.BindSeqCached` family of functions](#mapping-views-on-sequences).

* `listModel.DocLens`, similarly, is like `MapLens` but concatenating the resulting Docs.

* `listModel.TryFindByKeyAsView` gives a View on the item that has the given key, or `None` if it is absent.

    ```fsharp
    let showJohn =
        myPeopleColl.TryFindByKeyAsView("johnny87")
        |> Doc.BindView (function
            | None -> text "He is not here."
            | Some u -> text (sprintf "He is here, and his name is %s." u.Name)
        )
    ```

* `listModel.FindByKeyAsView` is equivalent to `TryFindByKeyAsView`, except that when there is no item with the given key, an exception is thrown.

* `listModel.ContainsKeyAsView` gives a View on whether there is an item with the given key. It is equivalent to (but more optimized than):

    ```fsharp
    View.Map Option.isSome (listModel.TryFindByKeyAsView(k))
    ```
