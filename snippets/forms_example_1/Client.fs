namespace forms_example_1

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.Forms
open WebSharper.UI.Html

// <FUMADOCS BEGIN>
[<JavaScript>]
module Client =

    type Species =
        | Cat | Dog | Piglet
        [<JavaScript>]
        override this.ToString() =
            match this with
            | Cat -> "cat"
            | Dog -> "dog"
            | Piglet -> "piglet"

    type Pet = { species: Species; name: string }
    type Person = { firstName: string; lastName: string; pets: seq<Pet> }

    let PetForm (init: Pet) =
        Form.Return (fun s n -> { species = s; name = n })
        <*> Form.Yield init.species
        <*> (Form.Yield init.name
            |> Validation.IsNotEmpty "Please enter your pet's name.")

    let PersonForm (init: Person) =
        Form.Return (fun first last pets ->
            { firstName = first; lastName = last; pets = pets })
        <*> (Form.Yield init.firstName
            |> Validation.IsNotEmpty "Please enter your first name.")
        <*> (Form.Yield init.lastName
            |> Validation.IsNotEmpty "Please enter your last name.")
        <*> Form.Many init.pets { species = Cat; name = "" } PetForm
        |> Form.WithSubmit

    let RenderPet species name =
        Doc.Concat [
            label [] [Doc.InputType.Radio [] Cat species; text (string Cat)]
            label [] [Doc.InputType.Radio [] Dog species; text (string Dog)]
            label [] [Doc.InputType.Radio [] Piglet species; text (string Piglet)]
            Doc.InputType.Text [] name
        ]

    let ShowErrorsFor v =
        v
        |> View.Map (function
            | Success _ -> Doc.Empty
            | Failure errors ->
                Doc.Concat [
                    for error in errors do
                        yield b [attr.style "color:red"] [text error.Text] :> _
                ]
        )
        |> Doc.EmbedView

    let RenderPerson (firstName: Var<string>)
                     (lastName: Var<string>)
                     (pets: Form.Many.CollectionWithDefault<Pet,_,_>)
                     (submit: Submitter<Result<_>>) =
        div [] [
            h2 [] [text "You"]
            div [] [
                label [] [text "First name: "; Doc.InputType.Text [] firstName]
                ShowErrorsFor (submit.View.Through firstName)
            ]
            div [] [
                label [] [text "Last name: "; Doc.InputType.Text [] lastName]
                ShowErrorsFor (submit.View.Through lastName)
            ]
            h2 [] [text "Your pets"]
            div [] [
                pets.Render (fun ops species name ->
                    div [] [
                        RenderPet species name
                        Doc.ButtonValidate "Move up" [] ops.MoveUp
                        Doc.ButtonValidate "Move down" [] ops.MoveDown
                        Doc.Button "Delete" [] ops.Delete
                        ShowErrorsFor (submit.View.Through name)
                    ])
                Doc.Button "Add a pet" [] pets.Add
            ]
            div [] [
                Doc.Button "Submit" [] submit.Trigger
            ]
        ]

    let Form =
        PersonForm {
            firstName = ""
            lastName = ""
            pets = [||] }
        |> Form.Run (fun p ->
            let message =
                "Welcome to you " + p.firstName + " " + p.lastName +
                (p.pets
                    |> Seq.map (fun pet ->
                        ", your " + string pet.species + " " + pet.name)
                    |> String.concat "") +
                "!"
            JS.Alert message)
        |> Form.Render RenderPerson

    [<SPAEntryPoint>]
    let Main () =
        Form
        |> Doc.RunById "main"
// <FUMADOCS END>
