/// This script updates the README.md file based on 
/// docs.config and tutorials.config
///
/// How to run:
///
///   fsharpi genReadme.fsx [version]

#r "System.Xml.Linq"
open System.IO
open System.Xml.Linq

let version =
    match fsi.CommandLineArgs with
    | [| _ |] -> "websharper41" // latest version by default
    | [| _; v |] -> v
    | _ -> failwith "Usage: fsharpi genReadme.fsx [version]"

let getPath x = Path.Combine(__SOURCE_DIRECTORY__, version, x)

let xn x = XName.Get(x)

let rawLink = sprintf "https://raw.githubusercontent.com/intellifactory/websharper.docs/master/%s/" version

let res =
    [|
        yield! 
            File.ReadLines(getPath "README.md") |> Seq.takeWhile (fun l ->
                not (l.StartsWith "##")
            )
        yield "## Documentation"
        let docsCfg = XDocument.Load(getPath "docs.config")
        for section in docsCfg.Root.Elements() do
            yield ""
            yield "### " + section.Attribute(xn"title").Value
            for page in section.Elements() do
            yield sprintf "* [%s](%s)" (page.Attribute(xn"title").Value) (page.Attribute(xn"src").Value.Replace(rawLink, ""))
        yield ""
        yield "## Tutorials"
        let tutorialsCfg = XDocument.Load(getPath "tutorials.config")
        for section in tutorialsCfg.Root.Elements() do
            yield ""
            yield "### " + section.Attribute(xn"title").Value
            for page in section.Elements() do
            yield sprintf "* [%s](%s)" (page.Attribute(xn"title").Value) (page.Attribute(xn"src").Value.Replace(rawLink, ""))
    |]
File.WriteAllLines(getPath "README.md", res)
