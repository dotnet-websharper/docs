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
module Client =
    open DependentForms

    open WebSharper.Sitelets
    let Main () =

        let router = Router.Infer<ClientEndpoint>()
        let routerInstance = Router.InstallHash Login router
        let rvReversed = Var.Create "" 
        let datevar = 
            let date = (Date.Now >> Date)()
            (sprintf "%04i-%02i-%02i" (date.GetFullYear()) (date.GetMonth()) (date.GetDay()))
            |> Var.Create
        let dateInput = Client.Doc.InputType.Date [] datevar
        let colorvar = Var.Create("#0000ff")

        Templates.MainTemplate.MainForm()
            .DateVar(datevar)
            .ColorVar(colorvar)
            .FormProbaSlot(routerInstance.View 
                |> Client.Doc.BindView (function
                | Home | Login -> AuthForms.renderLoginForm (Cart >> routerInstance.Set)
                | SignUp -> AuthForms.renderSignUpForm (Cart >> routerInstance.Set)
                | Cart authInfo -> ShoppingCart.renderOrdersForm (fun () -> ())
                )
            )
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