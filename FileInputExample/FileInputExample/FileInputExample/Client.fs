namespace FileInputExample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Templating
open WebSharper.UI.Notation

[<JavaScript>]
module Templates =
    type MainTemplate = Template<"index.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>

[<JavaScript>]
module FormStuff =
    open WebSharper.Forms
    type Species =
    | [<Constant "dog">] Dog
    | [<Constant "cat">] Cat
    with
        override this.ToString() =
            match this with
            | Dog -> "dog"
            | Cat -> "cat"
    type Pet = {species:Species;name:string;alive:bool}
                with static member Init () = {species=Dog;name="Fifi";alive=true}

    type RegData = {
        UserName:string
        Password:string
        
    }


    let RegForm() =
        let init = {UserName="";Password=""}
        Form.Return (fun uname pw -> {UserName=uname;Password=pw})
        <*> (Form.Yield init.UserName
            |> Validation.IsNotEmpty "Please provide a username!")
        <*> (Form.Do {
            let! pass = Form.Yield init.Password 
            return! Form.Yield "" 
                    |> Validation.IsNotEmpty "Please provide a password!"
                    |> Validation.Is ((=) pass) "The two passwords must match!"
        })
        |> Form.WithSubmit

    let RenderRegForm() =
        RegForm()
        |> Form.Render (fun username password submitter ->
            Html.form [] [

                Client.Doc.InputType.Text [] username
                password.RenderPrimary(fun vi -> //Client.Doc.InputType.Password [] vi
                    Doc.ShowErrors password.View (fun a -> Html.div [] [])
                )
                password.RenderDependent(fun a -> Client.Doc.InputType.Password [] a)
                Doc.ShowErrors submitter.View (fun errMsgs -> 
                    errMsgs 
                    |> List.map (fun msg ->
                        Html.span [Html.attr.color "red"] [Html.text $"{msg.Id}: {msg.Text}"]) 
                    |> Doc.Concat 
                )
                Doc.ButtonValidate "Submit" [] submitter
            ]
            
        )
    
    let PetForm () =
        let init = Pet.Init()
        Form.Return (fun s n isAlive -> { species = s; name = n; alive=isAlive })
        <*> Form.Yield init.species
        <*> (Form.Yield init.name
            |> Validation.IsNotEmpty "Please enter the pet's name.")
        <*> Form.Do {
            
            let! isAlive = Form.Yield true
            return! Form.Yield isAlive
        }
        |> Form.WithSubmit

    let PetListForm() =
        Form.ManyForm Seq.empty (PetForm()) Form.Yield
        |> Validation.Is(not << Seq.isEmpty) "Have at least one pet sadboi"
        |> Form.WithSubmit
        |> Form.Run (fun pets ->
            ()
        )

    let valami() =
        PetListForm()
        |> Form.Render (fun pets submitter ->
            Html.div [] [
                pets.RenderAdder (fun a b dependent subm ->
                    Doc.Concat [
                        dependent.RenderPrimary (Client.Doc.InputType.CheckBox [])
                        dependent.RenderDependent (Client.Doc.InputType.CheckBox [])
                    ]
                    
                )
            ]
        )
    let renderMainForm() =

        PetListForm()
        |> Form.Render (fun pets submitter ->
            Html.div [] [
                pets.RenderAdder (fun a b c d -> 
                    Html.div [] [
                        c.RenderPrimary (Client.Doc.InputType.CheckBox [])
                        c.RenderDependent (fun (bVal:Var<bool>) ->
                            bVal.View
                            |> View.Map(function | true -> "success" | _ -> "fail")
                            |> Html.textView
                            
                        )
                    ])
                
            ]
        )
[<JavaScript>]
module Client =
    open WebSharper.UI
    open WebSharper.Forms
    let Main () =
        let tsk = task {
            return "szoveg"
        }
        let rvReversed = Var.Create "" 
        let datevar = 
            let date = Date(Date.Now())
            (sprintf "%04i-%02i-%02i" (date.GetFullYear()) (date.GetMonth()) (date.GetDay()))
            |> Var.Create
        let dateInput = Client.Doc.InputType.Date [] datevar
        let colorvar = Var.Create("#0000ff")
        Templates.MainTemplate.MainForm()
            .DateVar(datevar)
            .ColorVar(colorvar)
            .FormProbaSlot(FormStuff.RenderRegForm())
            .OnFormSubmit(fun e ->
                Console.Log "Submit clicked!"
                let files = e.Vars.FilePicker.Value
                async {
                    let! res = Server.DoSomething e.Vars.TextToReverse.Value
                    rvReversed := res
                    let! arrays =
                        files
                        |> Array.map(fun file -> promise {
                                let! buffer = file.ArrayBuffer()
                                let uint8arr = Uint8Array buffer
                                return file.Name, Array.init uint8arr.Length uint8arr.Get
                            })
                        |> (Promise.All >> Promise.AsAsync)

                    let! fileRes = Server.ReturnFileInfo arrays
                    Console.Log fileRes

                }
                |> Async.StartImmediate

            )
            // .OnFileFormSubmit(fun e -> )
            .Reversed(rvReversed.View)
            .Doc()