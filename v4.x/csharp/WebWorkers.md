# Using Web Workers with WebSharper

Web Workers are a browser feature that allows running client-side code in parallel. Every worker runs on a separate thread, and they communicate by posting messages to one another.

In WebSharper 4.4 and later, creating a Web Worker is very simple; much simpler, in fact, than in plain JavaScript.

## Creating a Worker

You can call the `WebSharper.JavaScript.Worker` constructor and pass it a function, which will be the entry point for the worker.

```fsharp
using WebSharper;
using WebSharper.JavaScript;

var myWorker = new Worker(self =>
{
    JavaScript.Console.Log("This was written from the worker!");
    self.PostMessage("This worker's job is done, it can be terminated.");
});
myWorker.Onmessage = e =>
{
    JavaScript.Console.Log(e.Data);
    myWorker.Terminate();
};
```

The above code will:

- create and start a new Web Worker.

- print the following from the created worker thread:

    ```
    This was written from the worker!
    ```

- send the following from the worker thread to the main thread, which then prints it:

    ```
    This worker's job is done, it can be terminated.
    ```

- terminate the worker thread.

<a name="behind-the-scenes"></a>
## What's happening behind the scenes

A lot of things are happening behind the scenes with this simple call. The WebSharper compiler:

- recognizes that you are calling the `Worker` constructor with a function argument, and sets this function (which we will call the entry point) aside.
- creates a separate compiled JavaScript file called `MyProject.worker.js` that only contains the entry point and any other functions, classes, etc that it requires. It passes as argument to the entry point the global scope of the worker.
- compiles the `Worker` constructor call into JavaScript as:

    ```javascript
    var myWorker = new Worker("MyProject.worker.js")
    ```

If you have several such `Worker` calls in the same project, subsequent ones will be compiled to JavaScript files called `MyProject.worker0.js`, `MyProject.worker1.js`, and so on.

## Message passing

Communication between a Worker and the main thread is done by message passing. The worker can listen for incoming messages from the main thread using `self.Onmessage = f` or `self.AddEventListener("message", f, false)`, and send messages to the main thread using `self.PostMessage(msg)`. Conversely, the main thread can listen for incoming messages from the worker using `worker.Onmessage = f` or `self.AddEventListener("message", f, false)`, and send messages to the worker using `worker.PostMessage(msg)`.

```fsharp
var echoWorker = new Worker(self =>
{
    // This is the code of the worker

    // Listen to messages from the main thread
    self.Onmessage = e =>
    {
        // Here we're assuming we'll only ever receive strings
        var msg = (string)e.Data;

        // Send a message to the main thread
        self.PostMessage("The worker said: " + msg);
    };
});

// This code is on the main thread

// Listen to messages from the worker
echoWorker.Onmessage = e =>
{
    // Again we're assuming we'll only ever receive strings
    var msg = (string)e.Data;

    // Print the received message to the console
    JavaScript.Console.Log(msg);
};

// This will send the string to the worker, receive it back,
// and print to the console: "The worker said: Hello world!"
echoWorker.PostMessage("Hello world!");

// The worker is still running, we can send more messages
// and they will be sent back and forth and printed.
echoWorker.PostMessage("Hi again!");
```

## Using external dependencies

It is possible to use external script dependencies in a Worker. These will be compiled to calls to `importScripts()` in the worker script. For example, here is a worker that uses [math.js](http://mathjs.org/) and the binding [WebSharper.MathJS](https://www.nuget.org/packages/websharper.mathjs) to perform mathematical operations:

```fsharp
using WebSharper.MathJS;

let myWorker = new Worker(self =>
{
    self.Onmessage = e =>
    {
        var msg = (string)e.Data;
        var res = Math.Derivative(msg, "x").ToString();
        JavaScript.Console.Log(res);
    };
});
// This will print "4 * x + 3":
myWorker.PostMessage("2x^2 + 3x + 4");
```

## Customizing the worker script

There are a few options to customize the generated script for a worker.

- You can customize the filename of the script by passing a string as first argument to the `Worker` constructor:

    ```fsharp
    // This script will be called "MyLibrary.worker.js":
    var simpleWorker = new Worker(self =>
    {
        JavaScript.Console.Log("I'm a worker");
    });

    // This script will be called "MyLibrary.say-hello.js":
    var helloWorker = new Worker("say-hello", self =>
    {
        JavaScript.Console.Log("I'm called 'say-hello'!");
    });
    ```

- You can decide to include the values, modules and types marked `[<JavaScriptExport>]` in the generated script by passing an additional `true` argument to the `Worker` constructor. This can be useful for example if you intend to also call the generated script manually from JavaScript.

    ```fsharp
    [<JavaScriptExport>]
    public void MyExportedFunction()
    {
        Console.Log("I'm not called directly from the worker entry point");
    }

    public Worker CreateWorker()
    {
        return new Worker("worker", true, self =>
        {
            Console.Log("This doesn't call MyExportedFunction, but it will be defined anyway");
        });
    }
    ```

Note that in both cases, you must use a literal string or boolean, rather than a more complex expression, because it must be recognized at compile time.

## Customizing the script location

[The behind-the-scenes section](#behind-the-scenes) states that the worker constructor call is translated into the following JavaScript:

```javascript
var myWorker = new Worker("MyProject.worker.js")
```

That is actually a simplification. In practice, WebSharper needs to figure out the path to the script: the file name is not sufficient. The full path is computed and depends on the type of final project that the code is used in:

- In a client-server `website` project, the default path is `/Scripts/WebSharper/<assemblyname>/<filename>`.
- In a single-page application (aka `bundle` or `bundleOnly` project), the default path is `/Content/<assemblyname>/<filename>`.
- In a generated static HTML site (aka `html` project), the default path is `/Scripts/<assemblyname>/<filename>`.

These correspond to the location where the compiler extracts the files, so in a standard setup, everything just works and there is nothing to do. But if you need it, this is customizable using the `scriptBaseUrl` [configuration setting](project-variables#scriptBaseUrl). It changes the base URL for the script, ie. the paths mentioned aboved minus the `<assemblyname>/<filename>` suffix. Note that this URL **must** end with a slash, and almost always needs to start with a slash too.

Here is an example `wsconfig.json` that deviates from the standard: the output directory into which the files are extracted is customized using `outputDir`, so we need to customize `scriptBaseUrl` too.

```json
{
    "project": "bundleOnly",
    "outputDir": "wwwroot/js",
    "scriptBaseUrl": "/js/"
}
```
