namespace FileInputExample

open WebSharper
open WebSharper.JavaScript

module Server =

    [<Rpc>]
    let DoSomething input =
        let R (s: string) = System.String(Array.rev(s.ToCharArray()))
        async {
            return R input
        }
    [<Rpc>]
    let ReturnFileInfo (fileBuffers:(string*byte array) array) =
        System.Console.WriteLine("Got a ReturnFileCount request!")
        let arrSum =
            fileBuffers
            |> Array.sumBy (snd >> Array.sumBy (uint))
        let fileCount =
            Array.length fileBuffers
        let retStr = $"Sent {fileCount} files on submit, totaling {arrSum} bytes!"
        System.Console.WriteLine(retStr)
        async {
            return retStr
        }