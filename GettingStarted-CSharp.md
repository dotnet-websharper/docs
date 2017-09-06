# Getting started with WebSharper for C# #

This article will walk you through getting a WebSharper
example up and running.

## Creating a project

### With Visual Studio

After completing [the installation](Install.md), open Visual Studio and create a new project ("Website"):

    File > New Project > Templates > Visual C# >
    WebSharper > UI.Next Client-Server Web Application
	
This project defines a simple website, with both server and
client-side components.  To try it out, simply "Run" (F5) - you should
now see the code in action:

### With MonoDevelop / Xamarin Studio

After completing [the installation](Install-XS.md), open MonoDevelop / Xamarin Studio and create a new project ("Website"):

    File > New > Solution... >
    WebSharper > UI.Next Client-Server Web Application

This project defines a simple website, with both server and
client-side components.  To try it out, simply "Run" (F5) - you should
now see the code in action:

## The project files

Let us look at what the code does.

### Client class

This is the most interesting class (see `Client.cs`). Having it
marked `[JavaScript]` makes WebSharper cross-compile all code in
this module to JavaScript and run it in the browser.

The `Main` method is what is invoked as the client-side point. It
generates some DOM elements dynamically and these get inserted in the
page where the server definition places it.

The most interesting part is the remote server query. `rvInput` is a
`Var`, a reactive variable. We initiate it with an empty string. As you
can see a couple of lines later, we will use this `Var` to hold the
value of a text input.

The most important part of **UI.Next** is the `View` type. A `View` is
a time-varying value that can be mapped and inserted into the DOM, for
example. Next, we create a `Submitter` of `rvInput.View`. As `rvInput.View`
would change with any keystroke in the input field, we will use this
submitter to only act when the `Send` button is clicked. The submitter
has it's own `View`, too, which will reflect `rvInput.View`'s value every
time `submit.Trigger` is called.

Now comes the exciting part: we create our final `View`, `vReversed`, which
will have the data that the server sends back. We take a function that gets the
submitted nullable `string` value, returns a `Task<string>` (as the query will
be delegated to a different thread), and map that function over the submitter's
`View`. In this function, if no value has been submitted yet, the we will simply
return a Task that gives an empty string. On the other hand, if the submitter has 
a value, we call a server-side function just as simply as returning the empty
string was! And with that, we get our `View` which always contains the last
response from the server.

The rest of the code takes care of binding these reactive elements into the
DOM.

### Remoting class

This module (`Remoting.cs`) defines the `DoSomething` function that is
executed on the server but is also available on the client. Execution
happens by serializing arguments (empty in this case) and return value and passing them
over HTTP.  Not all types are supported, WebSharper will warn you
about potential problems at compile time.

### Server class

The main module (`Server.cs`) defines the page structure of your
website, which is now a single fallback function that accepts any sub-url, and passes
this argument to the client-side (initializing the text box).

As you are starting out, you may just consider this boilerplate and
focus on programming the client-side logic. For diving deeper,
reference documentation is available by topic in the manual.
