# How to make form sequences?
## Method A: Dependent forms

A simple username-password form with a "confirm password" field, which is only visible if we already have a valid password input, using `WebSharper.Forms`:
```fsharp
open WebSharper.Forms
type RegData = {
    Username: string
    Password: string
}
let ExampleForm() =
    Form.Return (fun username password -> {UserName=username;Password=password})
    <*> Form.Yield ""
        |> Validation.IsNotEmpty "Please provide a username!"
    <*> Form.Do {
        // the magic where we get our "dependent form"
        let! currentPw = 
            Form.Yield "" // the Form.Dependent's first Var binding
            |> Validation.IsNotEmpty "Please provide a password!"
            |> Validation.IsMatch "insert-regex-pattern-here" "Error message"
        //multiple let! bindings result in multiple vars 
        return! Form.Yield "" // the field that we'll bind to our "password" Var
                |> Validation.Is ((=) currentPw) "The two passwords must be an exact match!"
    }
    |> Form.WithSubmit // otherwise validation checks will run on every input change
```

With this, you get an abstract form, for which you'll also have to define a render function using `Form.Render`, such as:

```fsharp
open WebSharper
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.Forms

let renderExampleForm() =
    ExampleForm()
    |> Form.Render (fun username password submitter -> // the submitter is added to the form with the Form.WithSubmit method
        Html.form [] [
            Client.Doc.InputType.Text [] username
            password.RenderPrimary(fun pw -> Client.Doc.InputType.Password [] pw)
                // the "primary" form defined by let! bindings in a Form.Do block
            password.RenderDependent(fun pwcheck -> Client.Doc.InputType.Password [] pwcheck
            
            ) // the dependent form's render function defined by the "return!" of a Form.Do builder
            Doc.ShowErrors submitter.View (fun errMsgs -> // WWebSharper.Forms helper function
                errMsgs 
                |> List.map (fun msg ->
                    Html.span [Html.attr.color "red"] [Html.text $"{msg.Id}: {msg.Text}"]) 
                |> Doc.Concat 
            )
            Doc.ButtonValidate "Submit" [] submitter // helper function defined in WebSharper.Forms, automatically turns the submit button on/off+binds the specific submitter to itself
        ]
    )
```
In this case, the `renderExampleForm` returns a regular doc, so you can embed it anywhere you want on your site.

Notes:
- Multiple `let!` bindings result multiple vars in the returned `Form.Dependent` object.
- If a validation fails on a `let!` binding, the returned Form won't be rendered by the `formName.RenderDependent` function.
