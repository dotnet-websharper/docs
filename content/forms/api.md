---
title: Full forms API
---

Everything in the library is inside the namespace `WebSharper.Forms`.

## Types for form definitions

### Result type
The library defines its own result type for validation results. `Success` holds the valid value, while `Failure` holds a list of error messages.

```fsharp
type Result<'T> =
  | Success of 'T
  | Failure of list<ErrorMessage>
```

### ErrorMessage type
A type containing an error message associated with a specific form element identified by `Id` containing message `Text`.

It is automatically created by validation failures, see below. The `Id` property also do not need to be checked manually, 
instead, the `.Through` extension method can be used on a `View<Result<...>>` to filter for error messages arising from a given `Var` or `Form`.

### Form type
The central type for defining forms is `Form<'T, 'R>`. `'T` is the type of the value produced by the form, and `'R` is a signature that encodes how the form is structured to provide type-safety between the form definition and its rendering.

### Result module
This module provides utility functions for working with the `Result<'T>` type, enabling pattern matching, mapping, and binding operations typical for error-handling monads.

* `Result.IsSuccess: Result<'T> -> bool`: Check whether a result is successful.
* `Result.IsFailure: Result<'T> -> bool`: Check whether a result is failing.
* `Result.Map: ('T -> 'U) -> Result<'T> -> Result<'U>`: Pass a result through a function if it is successful.
* `Result.Apply: Result<'T -> 'U> -> Result<'T> -> Result<'U>`: Apply a function result to a value result if both are successful.
* `Result.Bind: ('T -> Result<'U>) -> Result<'T> -> Result<'U>`: Pass a result through a function if it is successful, also able to introduce new failure points.
* `Result.FailWith: string * ?string -> Result<'T>`: Create a failing result with a single error message, with an optional `Id` included, tying it to a `Var` or `Form`.

### Form.Many.ItemOperations type
Operations for managing individual items within a collection form.

* `.Delete: unit -> unit`: Delete the current item from the collection.
* `.MoveUp: Submitter<Result<bool>>`: Move the current item up one step in the collection.
* `.MoveDown: Submitter<Result<bool>>`: Move the current item down one step in the collection.

A `Submitter<'T>` is a type defined by WebSharper.UI that updates the value on an output view (`.View` property) when its `.Trigger()` method is called.
In this case the `Result<bool>` value will hold a `Failure []` state if the item is not movable in the given direction, and `Success true` otherwise.
This can be tied into a `Doc.ButtonValidate` helper to render a button that's enabled if the submitter is in valid state.

### Form.Many.Collection type
Represents a collection of forms for managing multiple items, including rendering and addition capabilities. Used in conjunction with `Form.Many` to handle dynamic list, which has an additional form part to create a new item before inserting.

* `.View: View<Result<seq<'T>>>`: A view on the resulting collection.
* `.Render: (Form.Many.ItemOperations -> 'V) -> Doc`: Render the item collection inside this form with the provided rendering function.
* `.Add: 'T -> unit`: Adds a new item to the collection.
* `.RenderAdder: 'Y -> Doc`: Render the form that creates new items to be inserted the collection.

### Form.Many.CollectionWithDefault type
A collection where new items are added with a default value, to be edited in their subforms after adding them.

* `.Add: unit -> unit`: Adds a new item with default value to the collection.

### Form.Dependent type
A type for forms that depend on previous values, enabling dynamic form structures where subsequent fields or validations depend on prior inputs. This supports conditional rendering and validation.

* `.View: View<Result<'TResult>>`: A view on the result of the dependent form.
* `.RenderPrimary: 'U -> Doc`: Render the primary part of a dependent form.
* `.RenderDependent: 'W -> Doc`: Render the dependent part of a dependent form.

### Form module
* `Form.Create: View<Result<'T>> -> ('R -> 'D) -> Form<'T, 'R -> 'D>`: Create a form from a view and a render builder. This is rarely used directly, forms are best created by composition.
* `Form.Render: 'R -> Form<'T, 'R -> #Doc> -> Doc`: Render a form with a render function.
* `Form.RenderMany: Many.Collection<'T, 'V, 'W, 'Y, 'Z> -> (Many.ItemOperations -> 'V) -> Doc`: Render the items of a collection with the provided rendering function.
* `Form.RenderManyAdder: Many.Collection<'T, 'V, 'W, 'Y, 'Z> -> 'Y -> Doc`: Render the form that inserts new items into a collection.
* `Form.RenderPrimary: Dependent<'TResult, 'U, 'W> -> 'U -> Doc`: Render the primary part of a dependent form.
* `Form.RenderDependent: Dependent<'TResult, 'U, 'W> -> 'W -> Doc`: Render the dependent part of a dependent form.
* `Form.GetView: Form<'T, 'R -> 'D> -> View<Result<'T>>`: Get the view of a form.
* `Form.Return: 'T -> Form<'T, 'D -> 'D>`: Create a form that always returns the same successful value.
* `Form.ReturnFailure: unit -> Form<'T, 'D -> 'D>`: Create a form that always fails.
* `Form.Yield: 'T -> Form<'T, (Var<'T> -> 'D) -> 'D>`: Create a form that returns a reactive value, initialized to a successful value `init`.
* `Form.YieldVar: Var<'T> -> Form<'T, (Var<'T> -> 'D) -> 'D>`: Create a form that returns a reactive value.
* `Form.YieldFailure: unit -> Form<'T, (Var<'T> -> 'D) -> 'D>`: Create a form that returns a reactive value, initialized to failure.
* `Form.YieldOption: option<'T> -> 'T -> Form<option<'T>, (Var<'T> -> 'D) -> 'D>`: Create a form that returns a reactive optional value, initialized to a successful value `init`. When the associated Var is `noneValue`, the result value is `None`; when it is any other value `x`, the result value is `Some x`.
* `Form.Apply: Form<'T -> 'U, 'R -> 'R1> -> Form<'T, 'R1 -> 'D> -> Form<'U, 'R -> 'D>`: Applies a function-producing form to a value-producing form in an applicative manner (aliased as `<*>`).
* `Form.WithSubmit: Form<'T, 'R -> Submitter<Result<'T>> -> 'D> -> Form<'T, 'R -> 'D>`: Add a submitter to a form: the returned form gets its value from the input form whenever the submitter is triggered.
* `Form.TransmitView: Form<'T, 'R -> View<Result<'T>> -> 'D> -> Form<'T, 'R -> 'D>`: Pass a view on the result of a form to its render function.
* `Form.TransmitViewMap: ('T -> 'U) -> Form<'T, 'R -> View<Result<'U>> -> 'D> -> Form<'T, 'R -> 'D>`: Pass a mapped view on the result of a form to its render function.
* `Form.TransmitViewMapResult: (Result<'T> -> 'U) -> Form<'T, 'R -> View<'U> -> 'D> -> Form<'T, 'R -> 'D>`: Pass a mapped view on the result (including validation errors) of a form to its render function.
* `Form.Map: ('T -> 'U) -> Form<'T, 'R -> 'D> -> Form<'U, 'R -> 'D>`: Map the result of a form.
* `Form.MapToResult: ('T -> Result<'U>) -> Form<'T, 'R -> 'D> -> Form<'U, 'R -> 'D>`: Map the result of a form, potentially adding adding validation errors.
* `Form.MapResult: (Result<'T> -> Result<'U>) -> Form<'T, 'R -> 'D> -> Form<'U, 'R -> 'D>`: Map the result of a form, with full validation result included.
* `Form.MapAsync: ('T -> Async<'U>) -> Form<'T, 'R -> 'D> -> Form<'U, 'R -> 'D>`: Map the result of a form asynchronously.
* `Form.MapToAsyncResult: ('T -> Async<Result<'U>>) -> Form<'T, 'R -> 'D> -> Form<'U, 'R -> 'D>`: Map the result of a form asynchronously, potentially adding adding validation errors.
* `Form.MapAsyncResult: (Result<'T> -> Async<Result<'U>>) -> Form<'T, 'R -> 'D> -> Form<'U, 'R -> 'D>` Map the result of a form asynchronously, with full validation result included.
* `Form.MapRenderArgs: 'R1 -> Form<'T, 'R1 -> 'R2> -> Form<'T, ('R2 -> 'D) -> 'D>`: Map the arguments passed to the render function of a form.
* `Form.FlushErrors: Form<'T, 'R -> 'D> -> Form<'T, 'R -> 'D>`: Map any failing result to a failure with no error messages.
* `Form.Run: ('T -> unit) -> Form<'T, 'R -> 'D> -> Form<'T, 'R -> 'D>`: Run a function on all successful results.
* `Form.RunResult: (Result<'T> -> unit) -> Form<'T, 'R -> 'D> -> Form<'T, 'R -> 'D>`: Run a function on all results.
* `Form.Dependent: Form<'TPrimary, 'U -> 'V> -> ('TPrimary -> Form<'TResult, 'W -> 'X>) -> Form<'TResult, (Dependent<'TResult, 'U, 'W> -> 'Y) -> 'Y>`: Create a dependent form where a `dependent` form depends on the value of a `primary` form.
* `Form.ManyForm: seq<'T> -> (Form<'T, 'Y -> 'Z>) -> ('T -> Form<'T, 'V -> 'W>) -> Form<seq<'T>, (Many.Collection<'T, 'V, 'W, 'Y, 'Z> -> 'x) -> 'x>`: Create a form that returns a collection of values, with an additional form used to insert new values in the collection.
* `Form.Many: seq<'T> -> 'T ->  ('T -> Form<'T, 'V -> 'W>) -> Form<seq<'T>, (Many.CollectionWithDefault<'T, 'V, 'W> -> 'x) -> 'x>`: Creates a form for managing a dynamic collection of items, with initial values, a default for additions, and a form builder for each item.

### Operators

* `<*>`: An alias for `Form.Apply`.
* `<*?>`: Passes the full result of the value-carrying form to the function-carrying form.

### Validation module
* `Validation.Is: ('T -> bool) -> string -> Form<'T, 'R -> 'D> -> Form<'T, 'R -> 'D>`: If the form value passes the predicate, it is passed on; else, `Failwith msg` is passed on.
* `Validation.IsNotEmpty: string -> Form<string, 'R -> 'D> -> Form<string, 'R -> 'D>`: If the form value is not an empty string, it is passed on; else, `Failwith msg` is passed on.
* `Validation.IsMatch: string -> string -> Form<string, 'R -> 'D> -> Form<string, 'R -> 'D>`: If the form value matches the given regexp, it is passed on; else, `Failwith msg` is passed on.
* `Validatiaon.MapValidCheckedInput: string -> Form<CheckedInput<'T>, 'R -> 'D> -> Form<'T, 'R -> 'D>`: Creates a validation based on a WebSharper.UI `CheckedInput` status.

### Computation expression syntax

You can also build forms with multiple fields and dependent form functionality with a computation expression. Use the `Form.Builder.Do` builder, `let!` bindings for sub-forms and `return` for the combined value.
Example:

```fsharp
let PersonForm (init: Person) =
    Form.Builder.Do {
        let! first = Form.Yield init.firstName
        let! last = Form.Yield init.lastName
        let! pets = Form.Many init.pets { species = Cat; name = "" } PetForm
        return { firstName = first; lastName = last; pets = pets }
    }
```

## UI helpers for form rendering

### Attr module
* `Attr.SubmitterValidate: Submitter<Result<'T>> -> Attr`: Add a click handler that triggers a submitter, and disable the element when the submitter's input is a failure.

### Doc module
* `Doc.ButtonValidate: string -> seq<Attr> -> Submitter<Result<'T>> -> Elt`: Create a button that triggers a submitter when clicked, and is disabled when the submitter's input is a failure.
* `Doc.ShowErrors: View<Result<'T>> -> (list<ErrorMessage> -> Doc) -> Doc`: Renders a document only when the view is in failure state, passing the error messages to the provided function.
* `Doc.ShowSuccess: View<Result<'T>> -> ('T -> Doc) -> Doc`: Renders a document only when the view is in success state, passing the value to the provided function.

### Extensions for Views
* `.Through: View<Result<'T>> * Var<'U> -> View<Result<'T>>`: Filters the error messages in a result view to only those associated with the given `Var`, for localized error display.
* `.Through: View<Result<'T>> * Form<'U, 'R> -> View<Result<'T>>`: Filters the error messages in a result view to only those associated with the given `Form`, for localized error display.
* `.ShowErrors: View<Result<'T>> * (list<ErrorMessage> -> Doc) -> Doc`: When the input View is a failure, show the given `Doc`; otherwise, show an empty `Doc`.
* `.ShowSuccess: View<Result<'T>> * ('T -> Doc) > Doc`: When the input View is a success, show the given `Doc`; otherwise, show an empty `Doc`.
