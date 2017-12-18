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
            <p class="title">Declarative routing in C#</p>
            <img src="/images/googlemaps.png" class="feature-image" />
            <p>Parse requests (including JSON or form content) and write links safely created from a C# class hierarchy automatically.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile" class="feature-image" >
            <p class="title">Declarative routing in F#</p>
            <img src="/images/googlemaps.png" class="feature-image" />
            <p>Same automatic routing is available with F# records and unions.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Declarative JSON format</p>
            <img src="/images/googlemaps.png" class="feature-image" />
            <p>Parse and generate JSON based on the shape and attributes of your data type.</p>
        </div>
    </div>
</section>
<section class="block-buzz has-text-centered">
    <div class="columns">
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Safe HTML templating</p>
            <img src="/images/cs-templating.gif" class="feature-image" />
            <p>Serve HTML content from template files, with strongly typed access to fill in holes.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Compose HTML easily</p>
            <img src="/images/formlet.png" class="feature-image" />
            <p>Generate HTML from C#/F# with a clean syntax.</p>
        </div>
    </div>
</section>

## JavaScript compiler and utilities

<section class="block-buzz has-text-centered">      
    <div class="columns">
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Use the power of C#</p>
            <img src="/images/cs-javascript.png" class="feature-image" />
            <p>Powered by Roslyn, you get latest language features.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Use the expressiveness of F#</p>
            <img src="/images/googlemaps.png" class="feature-image" />
            <p>Use F# language features like pattern matching and type providers on the client side.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Interact with JavaScript</p>
            <img src="/images/googlemaps.png" class="feature-image" />
            <p>JavaScript inlines are checked compile-time for correctness.</p>
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
            <img src="/images/googlemaps.png" class="feature-image" />
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
            <img src="/images/cs-javascript.png" class="feature-image" />
            <p>HTML templating and combinators, parsing or creating links, JSON serialization and your custom code all have compatible behavior.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Remoting with ease</p>
            <img src="/images/googlemaps.png" class="feature-image" />
            <p>Doing an AJAX request is just a call to your server-side function.</p>
        </div>
        <div class="column is-4-desktop is-6-mobile">
            <p class="title">Inlined client code</p>
            <img src="/images/googlemaps.png" class="feature-image" />
            <p>Include client-side generated content and event handlers directly inside your page without any indirection.</p>
        </div>
    </div>
</section>

## Contributing
WebSharper is open-source with [Apache 3.0 license](https://github.com/intellifactory/websharper/blob/master/LICENSE.md), on [GitHub](https://github.com/intellifactory/websharper/).
The source of these documentation pages are found in the [websharper.docs](https://github.com/intellifactory/websharper.docs/) repository.
Issue reports and pull requests are welcome to both code and documentation.