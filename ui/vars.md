---
order: -30
expanded: true
label: Vars
---
# Reactive vars

Reactive values that are directly set by code or by user interaction are represented by values of type `Var<'T>`. Vars are similar to F# `ref<'T>` in that they store a value of type `'T` that you can get or set using the `Value` property. But they can additionally be reactively observed or two-way bound to HTML input elements.

The following UI helpers are from `WebSharper.UI.Client`, for more variants check the `Doc` module.

---

## Doc.Input

Creates an `<input>` element with given attributes that is bound to a `Var<string>`.

```fsharp
let varText = Var.Create "initial value"
let myInput = Doc.Input [attr.name "my-input"] varText
```

With the above code, once `myInput` has been inserted in the document, `varText.Value` yields what the user has entered, and setting it updates the value of the input control.

---

## Doc.IntInput and Doc.FloatInput

Creates an `<input type="number">` bound to a `Var<CheckedInput<_>>` of the corresponding type (`int` or `float`, respectively). `CheckedInput` provides access to the validity and actual user input, and it is defined as follows:

```fsharp
type CheckedInput<'T> =
    | Valid of value: 'T * inputText: string
    | Invalid of inputText: string
    | Blank of inputText: string
```

`CheckedInput.Invalid` is returned when the input can not be parsed into a number.

---

## Doc.IntInputUnchecked and Doc.FloatInputUnchecked

Creates an `<input type="number">` bound to a `Var<_>` of the corresponding type (`int` or `float`, respectively). These functions do not check for the validity of the user's input, so be sure to only rely on them in situations where this is acceptable. Otherwise, use `Doc.IntInput` or `Doc.FloatInput` instead.

---

## Doc.InputArea

Creates a `<textarea>` element bound to a `Var<string>`.

---

## Doc.PasswordBox

Creates an `<input type="password">` element bound to a `Var<string>`.

---

## Doc.CheckBox

Creates an `<input type="checkbox">` element bound to a `Var<bool>`.

---

## Doc.CheckBoxGroup

Creates an `<input type="checkbox">`, but instead of associating it with a simple `Var<bool>`, it associates it with a specific `'T` in a `Var<list<'T>>`. If the box is checked, then the element is added to the list, otherwise it is removed.

{% tabs %}
{% tab title="F\#" %}

```fsharp
type Color = Red | Green | Blue

// Initially, Green and Blue are checked.
let varColor = Var.Create [Blue; Green]

let mySelector =
    div [] [
        label [] [
            Doc.CheckBoxGroup [] Red varColor
            text " Select Red"
        ]
        label [] [
            Doc.CheckBoxGroup [] Green varColor
            text " Select Green"
        ]
        label [] [
            Doc.CheckBoxGroup [] Blue varColor
            text " Select Blue"
        ]
    ]
```

{% endtab %}

{% tab title="Result" %}

```html
<div>
  <label><input type="checkbox" /> Select Red</label>
  <label><input type="checkbox" checked /> Select Green</label>
  <label><input type="checkbox" checked /> Select Blue</label>
</div>
```

{% endtab %}
{% endtabs %}

`varColor` is bound to the list of ticked checkboxes.

---

## Doc.Select

Creates a `<select>` dropdown given a list of values to select from. The label of every `<option>` is determined by the given print function for the associated value.

{% tabs %}
{% tab title="F\#" %}

```fsharp
type Color = Red | Green | Blue

// Initially, Green is checked.
let varColor = Var.Create Green

// Choose the text of the dropdown's options.
let showColor (c: Color) =
    sprintf "%A" c

let mySelector =
    Doc.Select [] showColor [Red; Green; Blue] varColor
```

{% endtab %}

{% tab title="Result" %}

```html
<select>
  <option>Red</option>
  <option selected>Green</option>
  <option>Blue</option>
</select>
```

{% endtab %}
{% endtabs %}

`varColor` is bound to contain the selected color.

---

## Doc.Radio

Creates an `<input type="radio">` given a value, which sets the given `Var` to that value when it is selected.

{% tabs %}
{% tab title="F\#" %}

```fsharp
type Color = Red | Green | Blue

// Initially, Green is selected.
let varColor = Var.Create Green

let mySelector =
    div [] [
        label [] [
            Doc.Radio [] Red varColor
            text " Select Red"
        ]
        label [] [
            Doc.Radio [] Green varColor
            text " Select Green"
        ]
        label [] [
            Doc.Radio [] Blue varColor
            text " Select Blue"
        ]
    ]
```

{% endtab %}

{% tab title="Result" %}

```html
<div>
  <label><input type="radio" /> Select Red</label>
  <label><input type="radio" checked /> Select Green</label>
  <label><input type="radio" /> Select Blue</label>
</div>
```

{% endtab %}
{% endtabs %}

`varColor` is bound to contain the selected color.
