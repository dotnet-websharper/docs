#r "System.Xml.Linq"

open System
open System.IO
open System.Text.RegularExpressions
open System.Collections.Generic

open System.Xml.Linq

[<AutoOpen>]
module Xml =

    let (|Root|_|) (xml: XDocument) = Some xml.Root

    let (|Node|_|) (s: string) (xml: XElement) =
        if xml.Name.LocalName.ToLower() = s.ToLower() then
            Some <| (xml, xml.Elements() |> List.ofSeq)
        else
            None

    let (|Attribute|_|) (s: string) (xml: XElement) =
        let attrs = xml.Attributes()
        attrs
        |> Seq.tryFind (fun att -> if att.Name.LocalName.ToLower() = s.ToLower() then true else false)
        |> function
            | None ->
                None
            | Some attr ->
                Some <| (attr.Value, xml)

    let (|AttributeOption|) s xml =
        match (|Attribute|_|) s xml with
        | None -> None, xml
        | Some (a, xml) -> Some a, xml

    let (|List|_|) (s: string) (xml: XElement list) =
        match List.tryFind (fun (elem: XElement) -> elem.Name.LocalName.ToLower() <> s.ToLower()) xml with
        | None ->
            Some xml
        | Some _ ->
            None

module DocChecker =
    let linkPattern =
        Regex(@"\((\w+)[.]md\)", RegexOptions.Multiline)

    type Link =
        {
            Source : string
            Target : string
        }

        override link.ToString() =
            String.Format("{0} -> {1}", link.Source, link.Target)

    let getLinks (file: FileInfo) =
        let t = File.ReadAllText(file.FullName)
        Set [|
            for m in linkPattern.Matches(t) ->
                m.Groups.[1].Value
        |]
        |> Set.map (fun info ->
            {
                Source = file.Name
                Target = info
            })

    //let manualPage =
    //    FileInfo(Path.Combine(__SOURCE_DIRECTORY__, "WebSharper.md"))

    //let tocLinks =
    //    getLinks manualPage
    //    |> Set.map (fun link -> link.Target)

    let d = DirectoryInfo(__SOURCE_DIRECTORY__)

    type Problem =
        | Missing of Link
        | Orphan of Link

        override p.ToString() =
            match p with
            | Missing k -> "missing : " + string k
            | Orphan k -> "orphan  : " + string k

    let doesExist name =
        let p = Path.Combine(__SOURCE_DIRECTORY__, name + ".md")
        File.Exists(p)

    //let problems =
    //    [|
    //        for file in d.EnumerateFiles("*.md") do
    //            let links = getLinks file
    //            yield!
    //                links
    //                |> Seq.choose (fun link ->
    //                    if not (doesExist link.Target) then
    //                        Some (Missing link)
    //                    elif not (tocLinks.Contains(link.Target)) && link.Target <> "WebSharper" then
    //                        Some (Orphan link)
    //                    else
    //                        None)
    //    |]

    //if problems.Length > 0 then
    //    printfn "Problems:"
    //    Seq.iter (printfn "  %O") problems

let configFiles =
    Directory.GetFiles(__SOURCE_DIRECTORY__, "*.config", SearchOption.AllDirectories)

let mdFiles =
    Directory.GetFiles(__SOURCE_DIRECTORY__, "*.md", SearchOption.AllDirectories)

let usedMDFiles = HashSet<string>()

let errorsInDocFile name content =
    Seq.empty

let errorsInDocFileLink config (link: string) =
    if link.StartsWith "http" then
        use wc = new System.Net.WebClient()
        try 
            errorsInDocFile link (wc.DownloadString link)
        with _ ->
            [ sprintf "Could not reach %s" link ] :> _
    elif link.StartsWith "local:" then
        let relPath = link.Substring(6)
        let path = Path.Combine(Path.GetDirectoryName(config), relPath)
        if File.Exists path then
            errorsInDocFile path (File.ReadAllText path)
        else
            [ sprintf "Linked file not found in config %s: %s" config link ] :> _  
    else
        [ sprintf "Unrecognized link in config %s: %s" config link ] :> _  

let errorsInConfigFile path =
    try
        match File.ReadAllText(path) |> XDocument.Parse with
        | Root(Node "folder" (_, List "section" sections)) ->
            sections |> Seq.collect (fun section ->
                match section with
                | Node "section" (Attribute "title" (stitle, _), List "page" pages) ->
                    pages |> Seq.collect (fun page ->
                        match page with
                        | Attribute "key" (key, Attribute "title" (title, Attribute "src" (src, _))) ->
                            errorsInDocFileLink path src
                        | _ ->
                            [ sprintf "Wrongly formatted page entry in config %s, section %s: %O" path stitle page ] :> _
                    )
                | _ -> [ sprintf "Wrongly formatted section entry in config %s: %O" path section ] :> _
            )
        | _ ->
            [ sprintf "Wrongly formatted config root: %s" path ] :> _
    with e ->
        [ sprintf "Error while parsing config xml %s: %s" path e.Message ] :> _
 
let allErrors = 
    configFiles
    |> Seq.collect errorsInConfigFile
    |> Array.ofSeq