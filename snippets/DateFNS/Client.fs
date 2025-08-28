namespace DateFNS

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.DateFNS

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let private format = "yyyy-MM-dd"

    [<SPAEntryPoint>]
    let Main () =
        let current = Var.Create (Date())
        let nDays   = Var.Create 3

        let viewTomorrow   = current.View |> View.Map (fun date -> DateFNS.AddDays(date, 1))
        let viewStartMonth = current.View |> View.Map (fun d -> DateFNS.StartOfMonth(d))

        let setNow () = current.Set <| Date()
        let addDay () = current.Set <| DateFNS.AddDays(current.Value, 1)
        let addWeek () = current.Set <| DateFNS.AddWeeks(current.Value, 1)
        let toStartMo () = current.Set <| DateFNS.StartOfMonth(current.Value)
        let addNDays () = current.Set <| DateFNS.AddDays(current.Value, nDays.Value)

        IndexTemplate.Main()
            .NowText(
                current.View |> View.Map (fun date -> DateFNS.Format(date, format))
            )
            .TomorrowText(
                viewTomorrow |> View.Map (fun date -> DateFNS.Format(date, format))
            )
            .StartOfMonthText(
                viewStartMonth |> View.Map (fun date -> DateFNS.Format(date, format))
            )
            .NDays(nDays)
            .SetNow(fun _ -> setNow())
            .AddDay(fun _ -> addDay())
            .AddWeek(fun _ -> addWeek())
            .ToStartOfMonth(fun _ -> toStartMo())
            .AddNDays(fun _ -> addNDays())
            .Doc()
        |> Doc.RunById "main"
