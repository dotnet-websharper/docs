namespace forms_example_2

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

    type Subject =
        | Math | English | History
        with override this.ToString() = $"%A{this}"

    type TeacherOrStudent =
        | Teacher of Subject 
        | Student of year: int

    let IsTeacherForm() = 
        Form.Yield true

    let TeacherForm() =
        Form.Yield Math

    let StudentForm() =
        Form.Yield (CheckedInput.Make 1)
        |> Validation.Is (function Valid _ -> true | _ -> false) "Please enter a valid number."
        |> Validation.Is (function Valid (y, _) when y >= 1 && y <= 12 -> true | _ -> false) "Please enter a valid school year (1-12)."
        |> Form.Map (function Valid (y, _) -> y | _ -> 0) //  by this time, we have already filtered out non-valid values

    type TeacherOrStudentInput =
        | TeacherInput of Var<Subject>
        | StudentInput of Var<CheckedInput<int>>

    let TeacherOrStudentForm() =
        Form.Dependent (IsTeacherForm()) (fun isTeacher ->
            if isTeacher then
                TeacherForm()
                |> Form.MapRenderArgs TeacherInput
                |> Form.Map Teacher
            else
                StudentForm()
                |> Form.MapRenderArgs StudentInput
                |> Form.Map Student
        )
        |> Form.WithSubmit

    let ShowErrorMessage v =
        v |> Doc.BindView (function
            | Success _ -> Doc.Empty
            | Failure msgs ->
                div [attr.style "color:red"] [for msg in msgs -> p [] [text msg.Text] ]
        )

    let Form =
        TeacherOrStudentForm()
        |> Form.Render (fun dep submit ->            
            div [] [
                // render the primary IsTeacherForm
                dep.RenderPrimary (fun rvIsTeacher ->
                    p [] [
                        label [] [Doc.InputType.Radio [] true rvIsTeacher; text "I am a teacher"]
                        label [] [Doc.InputType.Radio [] false rvIsTeacher; text "I am a student"]
                    ]
                )
                // render the dependent TeacherForm or StudentForm
                dep.RenderDependent (fun rvTeacherOrStudent ->
                    match rvTeacherOrStudent with
                    | TeacherInput rvSubject ->
                        Doc.InputType.Select [] string [Math; English; History] rvSubject
                    | StudentInput rvYear ->
                        Doc.Concat [
                            Doc.InputType.Int [ attr.min "1"; attr.max "12" ] rvYear
                            ShowErrorMessage (submit.View.Through rvYear)
                        ]
                )
                p [] [Doc.Button "Submit" [] submit.Trigger]
                submit.View |> View.Map (function
                    | Success (Teacher subject) ->
                        div [] [text ("You registered as a teacher of " + string subject + ".")]
                    | Success (Student year) ->
                        div [] [text ("You registered as a student in year " + string year + ".")]
                    | _ -> Doc.Empty
                )
                |> Doc.EmbedView
            ]
        )

    [<SPAEntryPoint>]
    let Main () =
        Form
        |> Doc.RunById "main"
// <FUMADOCS END>
