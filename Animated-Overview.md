# Overview of WebSharper

WebSharper is a framework and toolset for developing web/mobile applications and web services 
entirely in C# or F# (or a mix of the two languages) with strongly-typed client-server 
communication and site navigation. 
It provides powerful server-side capabilities *and* a compiler to JavaScript with
a whole set of client-side functional abstractions.

## Functional, composable web server

<div class="container">
    <div class="row text-center">      
		<div class="col-md-4">
            <h3>Declarative routing in C#</h3>
            <img src="/images/googlemaps.png" />
            <p>Parse requests (including JSON or form content) and write links safely created from a C# class hierarchy automatically.</p>
        </div>
        <div class="col-md-4">
            <h3>Declarative routing in F#</h3>
            <img src="/images/googlemaps.png" />
            <p>Same automatic routing is available with F# records and unions.</p>
        </div>
        <div class="col-md-4">
            <h3>Declarative JSON format</h3>
            <img src="/images/googlemaps.png" style="max-height:250px; width: 380px;" />
            <p>Parse and generate JSON based on the shape and attributes of your data type.</p>
        </div>
    </div>
    <div class="row text-center">
        <div class="col-md-4">
            <h3>Safe HTML templating</h3>
            <img src="/images/cs-templating.gif" style="max-height:250px; width: 380px;" />
            <p>Serve HTML content from template files, with strongly typed access to fill in holes.</p>
        </div>
        <div class="col-md-4">
            <h3>Compose HTML easily</h3>
            <img src="/images/formlet.png" style="max-height:250px; width: 380px;" />
            <p>Generate HTML from C#/F# with a clean syntax.</p>
        </div>
    </div>
</div>

## JavaScript compiler and utilities

<div class="container">
    <div class="row text-center">      
		<div class="col-md-4">
            <h3>Use the power of C#</h3>
            <img src="/images/cs-javascript.png" />
            <p>Powered by Roslyn, you get latest language features.</p>
        </div>
        <div class="col-md-4">
            <h3>Use the expressiveness of F#</h3>
            <img src="/images/googlemaps.png" />
            <p>Use F# language features like pattern matching and type providers on the client side.</p>
        </div>
        <div class="col-md-4">
            <h3>Interact with JavaScript</h3>
            <img src="/images/googlemaps.png" style="max-height:250px; width: 380px;" />
            <p>JavaScript inlines are checked compile-time for correctness.</p>
        </div>
    </div>
    <div class="row text-center">      
		<div class="col-md-4">
            <h3>Use JavaScript libraries</h3>
            <img src="/images/googlemaps.png" />
            <p>Many JavaScript libraries has typed interfaces for WebSharper available on NuGet.</p>
        </div>
        <div class="col-md-4">
            <h3>Reactive UI</h3>
            <img src="/images/googlemaps.png" />
            <p>Let the data flow through your web pages.</p>
        </div>
        <div class="col-md-4">
            <h3>Powerful abstractions</h3>
            <img src="/images/formlet.png" style="max-height:250px; width: 380px;" />
            <p>Write full web forms in a couple lines of code.</p>
        </div>
    </div>
</div>

## Seamless client-server interaction

<div class="container">
    <div class="row text-center">      
		<div class="col-md-4">
            <h3>Share code</h3>
            <img src="/images/cs-javascript.png" />
            <p>HTML templating and combinators, parsing or creating links, JSON serialization and your custom code all have compatible behavior.</p>
        </div>
        <div class="col-md-4">
            <h3>Remoting with ease</h3>
            <img src="/images/googlemaps.png" />
            <p>Doing an AJAX request is just a call to your server-side function.</p>
        </div>
        <div class="col-md-4">
            <h3>Inlined client code</h3>
            <img src="/images/googlemaps.png" style="max-height:250px; width: 380px;" />
            <p>Include client-side generated content and event handlers directly inside your page without any indirection.</p>
        </div>
    </div>
</div>

## Contributing
WebSharper is open-source with [Apache 3.0 license](https://github.com/intellifactory/websharper/blob/master/LICENSE.md), on [GitHub](https://github.com/intellifactory/websharper/).
The source of these documentation pages are found in the [websharper.docs](https://github.com/intellifactory/websharper.docs/) repository.
Issue reports and pull requests are welcome to both code and documentation.