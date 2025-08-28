namespace SystemDateTime

open System
open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let private format = "yyyy-MM-dd"

    [<SPAEntryPoint>]
    let Main () =
        let current = Var.Create DateTime.Now
        let nDays   = Var.Create 3

        let viewTomorrow   = current.View |> View.Map (fun date -> date.AddDays(1.))
        let viewStartMonth = current.View |> View.Map (fun date -> DateTime(date.Year, date.Month, 1))

        let setNow () = current.Set <| DateTime.Now
        let addDay () = current.Set <| current.Value.AddDays(1.)
        let addWeek () = current.Set <| current.Value.AddDays(7.)
        let toStartMo () = 
            let dateTime = current.Value in current.Set <| DateTime(dateTime.Year, dateTime.Month, 1)

        let addNDays () = current.Set <| current.Value.AddDays(nDays.Value)

        IndexTemplate.Main()
            .NowText(current.V.ToString(format))
            .TomorrowText(viewTomorrow.V.ToString(format))
            .StartOfMonthText(viewStartMonth.V.ToString(format))
            .NDays(nDays)
            .SetNow(fun _ -> setNow())
            .AddDay(fun _ -> addDay())
            .AddWeek(fun _ -> addWeek())
            .ToStartOfMonth(fun _ -> toStartMo())
            .AddNDays(fun _ -> addNDays())
            .Doc()
        |> Doc.RunById "main"
