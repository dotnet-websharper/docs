# Adding file uploads to a form
## UI

### Inside the template's HTML file
If you're using a regular button (RPC):
```html
    <input type="file" name="input-name" ws-var="FileInput">Label</input>
    <button ws-onclick="OnFileSend">Submit</button>
```

If you're using forms:
```html
    <form ws-onsubmit="OnFileubmit" method="post" action="/your-post-endpoint">
        <input type="file" name="input-name" ws-var="FileInput">Label</input>
        <input type="submit">Submit</button>
    </form>
```
You can also add the `multiple` attribute to the file input if you want to, in the resulting code it's handled with a `JavaScript.File array` in both cases.

### Client-side code (RPC)

#### With template:
```fsharp
    open WebSharper.JavaScript

    MyTemplate()
        .OnFileSend(fun e ->
            let files: File array = e.Vars.FileInput.Value
            //...
        )
```

#### Without template:
```fsharp
    open WebSharper.UI.Client
    open WebSharper.JavaScript

    let files : Var<File array> = Var.Create Array.empty
    
    let onClickAttr = Html.on.click (fun el evt -> doSomethingWith files.Value)

    (*Rest of the content*) [] [
        (*...*))
        Doc.InputType.File [
            onClickAttr
            Html.attr.multiple "" // "multiple" attr if you want to use that
        ] files
    ]
```

#### Conversion to Rpc-compatible format
```fsharp
// example inside a handler, using only the first file to keep it a bit more readable
let file = e.Vars.FileInput.Value[0]
promise {
    let! arrayBuffer = file.ArrayBuffer()
    let uintArr = Uint8Array arrayBuffer
    // in case of multiple files, just send an array of the byteArrs below
    let fileName, byteArr = 
        file.Name, Array.init uintArr.Length (fun i -> uintArr.Get(i)) // or just (uintArr.Get)
        
        // alternative way, watch out for F#'s inclusive "to" range
        file.Name, [|for i=0 to (uintArr.Length-1) do uintArr.Get(i)|]

    Server.ExampleSaveFile fileName byteArr
}


```

### Client-side code (Using FormData)
You can also pass files using a FormData object, which is either sent by a(n) 
```html
<input type="submit" action="/your-post-endpoint" method="post" enctype="multipart/form-data"/>
``` 
element, or you can create one yourself as described below and send it by hand. 

#### FormData object from code:
```fsharp
let fd = FormData()
files.Iter(fun (name, data) -> 
    fd.Append((*insert name of file input here*)"input-name", data, name)
)
```

#### FormData from template:
No custom handling logic is required from a template.

#### FormData object from onsubmit handler

## Sitelets

### How do I handle received file data?

### Server-side code (RPC)

Using the logic specified in the client-side code, the server is receiving a `string`, containing the file's name, and a `byte array` or containing the file data itself.
```fsharp
open WebSharper
open System.IO

module Server = 
    [<Rpc>]
    let ExampleSaveFile (fileName:string) (fileContent:byte array) =
        task {
            let uploadPath = Path.Combine("specified-folder-path",fileName)
            do! File.WriteAllBytesAsync(uploadPath, fileContent)

        } |> Async.AwaitTask // or you can just return the Task itself

```

### Server-side code (FormData)

Here, you'll have to define `your-post-endpoint` on your server, such as
```fsharp
open WebSharper

type EndPoint =
| [<EndPoint "/home">] Home
| [<EndPoint "POST /your-post-endpoint">] YourPostEndPoint // <---
```
Then, you'll also have to define the handling logic, which looks like this in an app that uses `Application.MultiPage`:
```fsharp
open WebSharper
open System.IO
open System.Threading.Tasks

Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            | EndPoint.Home -> HomePage ctx
            | EndPoint.YourPostEndPoint ->
                // here, your files are 'WebSharper.Sitelets.Http.IPostedFile's
                ctx.Request.Files
                |> Seq.map (fun file ->
                    task {
                        use inputReader = new StreamReader(file.InputStream) 
                        use outputWriter = new StreamWriter(Path.Combine("specified-folder-path",file.FileName))
                        return! outputWriter.WriteAsync(inputReader.ReadToEnd())
                    }
                )
                |> Seq.cast<Task>
                |> Array.ofSeq
                |> Task.WaitAll
                
                (*rest of the handler*)
                HomePage ctx // replace with anything
        )
```

# Adding date inputs to a form
## UI
### Templating

Note: The ws-var is resolved to a `yyyy-mm-dd` string here
```fsharp
<input
    name="my-date"
    type="date"
    ws-var="DateVar"
    />
```

### Combinator

```fsharp
open WebSharper.UI.Client

let dateVar = Var.Create "2000-01-01"
let colorInput = Doc.InputType.Date [(*html attributes*)] dateVar
```


# Adding color inputs to a form
## UI
### Templating

Note: The ws-var is resolved to a `#1122ff` hex string here
```fsharp
<input
    name="my-color"
    type="color"
    ws-var="ColorVar"
    />
```

### Combinator

```fsharp
open WebSharper.UI.Client

let colorVar = Var.Create "#ffffff"
let colorInput = Doc.InputType.Color [(*html attributes*)] colorVar
```

