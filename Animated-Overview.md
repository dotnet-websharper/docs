# Overview of WebSharper

WebSharper is a framework and toolset for developing web/mobile applications and web services 
entirely in C# or F# (or a mix of the two languages) with strongly-typed client-server 
communication and site navigation. 
It provides powerful server-side capabilities *and* a compiler to JavaScript with
a whole set of client-side functional abstractions.

## Functional, composable web server

<style>
	.feature-image {
        max-height:250px;
        width: 380px;
	}
</style>
<section class="block-buzz has-text-centered">
    <div class="columns">
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Safe HTML templating</p>
            <img src="/images/cs-templating.gif" class="feature-image" />
            <p>Serve HTML content from template files, with strongly typed access to fill in holes.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Compose HTML easily</p>
            <img src="/images/html-syntax.png" class="feature-image" />
            <p>Generate HTML from C#/F# with a clean syntax.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Inlined client code</p>
            <img src="/images/todo.png" class="feature-image" />
            <p>Have event handlers and client-generated content in C#/F# right inside your server-side code, auto-translated to JavaScript.</p>
        </div>
    </div>
</section><section class="block-buzz has-text-centered">      
    <div class="columns">
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Declarative routing</p>
            <img src="/images/routing.png" class="feature-image" />
            <p>Parse requests and write links using C#/F# types automatically.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Customizable JSON format</p>
            <img src="/images/json-format.png" class="feature-image" />
            <p>Parse and generate JSON based on the shape and attributes of your data type.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile" class="feature-image" >
            <p class="title">Generate static sites</p>
            <img src="/images/todo.png" class="feature-image" />
            <p>Output HTML+JS into a folder, all set up to work without WebSharper's server runtime.</p>
        </div>
    </div>
</section>

## JavaScript compiler and utilities

<section class="block-buzz has-text-centered">      
    <div class="columns">
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Use the power of C# and/or F#</p>
            <img src="/images/linq.png" class="feature-image" />
            <p>Source-to-source translation with support for latest and greatest language features of both C# and F#.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Interact with JavaScript</p>
            <img src="/images/cs-javascript.gif" class="feature-image" />
            <p>JavaScript inlines are checked compile-time for correctness.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Metaprogramming</p>
            <img src="/images/todo.png" class="feature-image" />
            <p>Customize translation with macros: every call to annotated methods can be modified by your custom class.</p>
        </div>
    </div>
</section>
<section class="block-buzz has-text-centered">      
    <div class="columns">
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Use JavaScript libraries</p>
            <img src="/images/googlemaps.png" class="feature-image" />
            <p>Many JavaScript libraries has typed interfaces for WebSharper available on NuGet.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Reactive UI</p>
            <img src="/images/todo.png" class="feature-image" />
            <p>Let the data flow through your web pages.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Powerful abstractions</p>
            <img src="/images/formlet.png" class="feature-image" />
            <p>Write full web forms in a couple lines of code.</p>
        </div>
    </div>
</section>

## Seamless client-server interaction

<section class="block-buzz has-text-centered">      
    <div class="columns">
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Share code</p>
            <img src="/images/shared-code.png" class="feature-image" />
            <p>HTML templating and combinators, parsing or creating links, JSON serialization and your custom code all have compatible behavior.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Remoting with ease</p>
            <img src="/images/cs-analyzer.gif" class="feature-image" />
            <p>Doing an AJAX request is just a plain asynchronous call to your server-side function.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">WebSocket support</p>
            <img src="/images/todo.png" class="feature-image" />
            <p>Using a customizable JSON format for your messages.</p>
        </div>
    </div>
</section>

## Contributing
WebSharper is open-source with [Apache 3.0 license](https://github.com/intellifactory/websharper/blob/master/LICENSE.md), on [GitHub](https://github.com/intellifactory/websharper/).
The source of these documentation pages are found in the [websharper.docs](https://github.com/intellifactory/websharper.docs/) repository.
Issue reports and pull requests are welcome to both code and documentation.