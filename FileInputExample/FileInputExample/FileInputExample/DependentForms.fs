namespace FileInputExample

open WebSharper
open WebSharper.Forms
open WebSharper.UI

[<JavaScript>]
module DependentForms =

    type AuthInfo = {
        Username:string
        Password:string
    }
        with 
            static member Create username password = {Username=username;Password=password}
            static member Default = AuthInfo.Create "" ""

    type ClientEndpoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/login">] Login
    | [<EndPoint "/sign-up"; Json "userData">] SignUp
    | [<EndPoint "/";Json "userData">] Cart of userName:string
    
    module AuthForms =

            let authFormBase() =
                Form.Return (AuthInfo.Create)
            let userNameBase() =
                (Form.Yield AuthInfo.Default.Username
                    |> Validation.IsNotEmpty "Please provide a username")
            let passwordBase() =
                Form.Yield AuthInfo.Default.Password
                // |> Validation.Is (insert someRegexCheck here) "errorMsg"
                |> Validation.IsNotEmpty "Password cannot be empty!"
            let passwordWithConfirm() =
                Form.Do {
                    let! pass = passwordBase()
                    return! (Form.Yield AuthInfo.Default.Password
                            |> Validation.Is ((=) pass) "The two passwords must match!")
                }
            let loginForm() =
                authFormBase()
                <*> userNameBase()
                <*> passwordBase()
                |> Form.WithSubmit

            let ValamiForm() =
                Form.Do {
                    let! user = userNameBase()
                    let! pass = passwordWithConfirm()
                    return {Username=user;Password=pass}
                }
                |> Form.WithSubmit
            let signUpForm() =
                authFormBase()
                <*> userNameBase()
                <*> passwordWithConfirm()
                |> Form.WithSubmit
            
            let private renderErrors errors =
                    Doc.ShowErrors errors (fun errs -> 
                        errs 
                        |> List.map (fun err -> Html.div [] [Html.text err.Text]) 
                        |> Doc.Concat)
            let private renderPasswordDependent (passwordForm: Form.Dependent<string,Var<string> -> Doc,Var<string> -> Doc>) =
                let inline renderFun view = Html.div [] [
                    Client.Doc.InputType.Password [] view
                    passwordForm.View |> renderErrors
                ]
                Doc.Concat [
                    passwordForm.RenderPrimary renderFun
                    passwordForm.RenderDependent renderFun
                ]

            let renderSubmit (submitter:Submitter<Result<_>>) =
                Doc.Concat [
                    let showView = submitter.Input.Map(function | Success _ -> false | Failure _ -> true)
                    // Client.Doc.Button "Submit" [Html.attr.disabledBool showView] submitter.Trigger
                    Doc.ButtonValidate "Submit" [] submitter
                    submitter.View |> renderErrors

                ]
            let renderAuthForm (authForm:_ -> Form<AuthInfo,_>) passwordRender onSubmit =
                authForm()
                |> Form.Run (fun authSuccess -> onSubmit authSuccess.Username)
                |> Form.Render (fun username password submitter ->
                    Html.form [] [
                        Client.Doc.InputType.Text [] username
                        passwordRender password
                        renderSubmit submitter
                    ]
                )
            let renderLoginForm =
                renderAuthForm loginForm (Client.Doc.InputType.Password [])
            let renderSignUpForm =
                renderAuthForm signUpForm renderPasswordDependent

            
        module ShoppingCart =
            type CartItem = {
                name:string
                quantity:int
            }
                with static member Init() = {name="";quantity=1}

            type CheckoutFormData = {
                name:string
                address:string
                email:string
            }
            let items = [
                {name="Lepin Star Demolisher";quantity=1}
                {name="Fight Robot CN";quantity=3}
                {name="Protein bar";quantity=5}
            ]

            let itemsToOrder = items |> ListModel.Create (fun item -> item.name)
            
            let itemForm (init:CartItem) =
                Form.Return (fun name quantity -> {name=name;quantity=quantity})
                <*> (Form.Yield init.name
                    |> Validation.IsNotEmpty "Name cannot be empty")
                <*> (Form.Yield init.quantity
                    |> Validation.Is (fun x -> x > 0) "Must order at least one of item!")

            let cartForm it =
                Form.Many it (CartItem.Init()) itemForm

            let orderForm it =
                Form.Return (fun name address email items -> {name=name;address=address;email=email})
                <*> (Form.Yield ""
                    |> Validation.IsNotEmpty "Please enter a name")
                <*> (Form.Yield ""
                    |> Validation.IsNotEmpty "Address pls")
                <*> (Form.Yield ""
                    |> Validation.IsNotEmpty "Email pls")
                <*> cartForm it
                |> Form.WithSubmit

            let renderCartForm it =
                cartForm it
                |> Form.Render (fun collection -> 
                    collection.Render (fun ops itemName quantity -> 
                        Html.div [] [
                            Html.div [] [Html.textView itemName.View]
                            Html.div [] [Html.textView (quantity.View.Map(string))]
                        ]
                    ))
                    
            let renderCartItems (collection: Form.Many.CollectionWithDefault<CartItem,(Var<string>->Var<int>->_),_>) =
                collection.Render (fun ops itemName quantity -> 
                        Html.div [] [
                            Html.div [] [Html.textView itemName.View]
                            Html.div [] [Html.textView (quantity.View.Map string)]
                        ]
                    )
            let renderOrdersForm onSubmit =
                orderForm items
                |> Form.Run (fun i -> onSubmit())
                |> Form.Render (fun name address email coll submitter -> 
                    Html.div [] [
                        Html.h2 [] [Html.text "Shopping cart"]
                        renderCartItems coll

                        Html.form [Html.attr.``class`` "d-flex flex-column"] [
                            Html.label [Html.attr.``for`` "name"] [Html.text "Name"]
                            Client.Doc.InputType.Text [Html.attr.name "name"] name
                            Html.label [Html.attr.``for`` "address"] [Html.text "Address"]
                            Client.Doc.InputType.Text [Html.attr.name "address"] address
                            Html.label [Html.attr.``for`` "email"] [Html.text "Email"]
                            Client.Doc.InputType.Email [Html.attr.name "email"] email
                            Doc.ButtonValidate "Place order" [] submitter
                        ]
                    ]
                    
                    
                    
                )