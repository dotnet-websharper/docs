---
order: -50
label: Lenses
---
# Lenses

## Vars and lensing

The `Var<'T>` type is actually an abstract class, this makes it possible to create instances with an implementation different from `Var.Create`. The main example of this are **lenses**.

In WebSharper.UI, a lens is a `Var` without its own storage cell that "focuses" on a sub-part of an existing `Var`. For example, given the following:

```fsharp
type Person = { FirstName : string; LastName : string }
let varPerson = Var.Create { FirstName = "John"; LastName = "Doe" }
```

You might want to create a form that allows entering the first and last name separately. For this, you need two `Var<string>`s that directly observe and alter the `FirstName` and `LastName` fields of the value stored in `varPerson`. This is exactly what a lens does.

To create a lens, you need to pass a getter and a setter function. The getter is called when the lens needs to know its current value, and extracts it from the parent `Var`'s current value. The setter is called when setting the value of the lens; it receives the current value of the parent `Var` and the new value of the lens, and returns the new value of the parent `Var`.

```fsharp
let varFirstName =
    varPerson.Lens (fun p -> p.FirstName)
                   (fun p n -> { p with FirstName = n })
let varLastName =
    varPerson.Lens (fun p -> p.LastName)
                   (fun p n -> { p with LastName = n })

let myForm =
    div [] [
        Doc.Input [attr.placeholder "First Name"] varFirstName
        Doc.Input [attr.placeholder "Last Name"] varLastName
    ]
```

---

### Automatic lenses

In the specific case of records, you can use `LensAuto` to create lenses more concisely. This method only takes the getter, and is able to generate the corresponding setter during compilation.

```fsharp
let varFirstName = varPerson.LensAuto (fun p -> p.FirstName)

// The above is equivalent to:
let varFirstName = varPerson.Lens (fun p -> p.FirstName)
                                  (fun p n -> { p with FirstName = n })
```

You can be even more concise when using the `Doc.Input` family of functions, thanks to the V shorthand.

---

## The V shorthand

Mapping reactive values from their model to a value that you want to display can be greatly simplified using the V shorthand. This shorthand revolves around passing calls to the property `view.V` to a number of supporting functions.

---

### Views and V

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
    | Some p -> div [] [text varPerson.V.FirstName]
)
let myDoc = vMyDoc.V

// The above is equivalent to:
let vMyDoc =
    varPerson.View |> View.Map (fun p ->
        match p with
        | None -> Doc.Empty
        | Some p -> div [] [text p.FirstName]
    )
let myDoc = Doc.EmbedView vMyDoc
```

---

### Vars and V

Vars also have a `.V` property. When used with one of the above supporting functions, it is equivalent to `.View.V`.

```fsharp
let varPerson = Var.Create { FirstName = "John"; LastName = "Doe" }

let vFirstName = V(varPerson.V.FirstName)

// The above is equivalent to:
let vFirstName = V(varPerson.View.V.FirstName)

// Which is also equivalent to:
let vFirstName = varPerson.View |> View.Map (fun p -> p.FirstName)
```

Additionally, `var.V` can be used as a shorthand for [lenses](#lens). `.V` is a shorthand for `.LensAuto` when passed to the following supporting functions:

* `Lens` simply creates a lensed Var.

    ```fsharp
    type Person = { FirstName : string; LastName : string }
    let varPerson = Var.Create { FirstName = "John"; LastName = "Doe" }

    let myForm =
        div [] [
            Doc.Input [attr.placeholder "First Name"] (Lens varPerson.V.FirstName)
            Doc.Input [attr.placeholder "Last Name"] (Lens varPerson.V.LastName)
        ]
    ```

* `Doc.InputV`
* `Doc.InputAreaV`
* `Doc.PasswordBoxV`
* `Doc.IntInputV`, `Doc.IntInputUncheckedV`
* `Doc.FloatInputV`, `Doc.FloatInputUncheckedV`

```fsharp
type Person = { FirstName : string; LastName : string }
let varPerson = Var.Create { FirstName = "John"; LastName = "Doe" }

let myForm =
    div [] [
        Doc.InputV [attr.placeholder "First Name"] varPerson.V.FirstName
        Doc.InputV [attr.placeholder "Last Name"] varPerson.V.LastName
    ]

// The above is equivalent to:
let myForm =
    div [] [
        Doc.Input [attr.placeholder "First Name"]
            (varPerson.LensAuto (fun p -> p.FirstName))
        Doc.Input [attr.placeholder "Last Name"]
            (varPerson.LensAuto (fun p -> p.LastName))
    ]

// Which is equivalent to:
let myForm =
    div [] [
        Doc.Input [attr.placeholder "First Name"] 
            (varPerson.Lens (fun p -> p.FirstName) (fun p n -> { p with FirstName = n }))
        Doc.Input [attr.placeholder "Last Name"]
            (varPerson.Lens (fun p -> p.LastName) (fun p n -> { p with LastName = n }))
    ]
```
