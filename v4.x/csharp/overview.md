# Overview of WebSharper

WebSharper is a framework and toolset for developing web/mobile applications and web services 
entirely in C# or F# (or a mix of the two languages) with strongly-typed client-server 
communication and site navigation. 
It provides powerful server-side capabilities *and* a compiler to JavaScript with
a whole set of client-side functional abstractions.

Of course, it is possible to use WebSharper only for its server-side
functionality, or conversely, purely as a .NET-to-JavaScript compiler that comes
with powerful libraries. But by taking advantage of both aspects, you can also
benefit from the facilities provided by WebSharper to allow client-server
interaction with minimal boilerplate.

Here is an overview of WebSharper's capabilities.

## Sitelets: server-side functionality

WebSharper Sitelets is an API to parse HTTP requests and serve content in a strongly typed and functional way.
HTTP endpoints are represented by values of a user-defined
EndPoint type, which are parsed from requests based on their shape and some
attributes. With Sitelets, you can:

* Discriminate endpoints based on the HTTP method, URL path, query arguments,
  and request body (JSON or form body).
* Parse and generate JSON based on the shape and attributes of your data type,
  for easy REST APIs.
* Generate links from EndPoint values, practically eliminating the risk of
  internad dead links.
* Generate HTML content from template files or directly in C#/F# with a clean
  syntax.

Learn more about Sitelets [here](sitelets.md).

## JavaScript compiler and client-side abstractions

WebSharper can compile all your C# and/or F# source code to JavaScript.
Full interoperability is supported if you use both languages.
Unlike other .NET-to-JavaScript compilers, you can tell WebSharper what parts of your code
should or shouldn't be compiled to JavaScript (annotating types, methods or a whole assembly).
This allows you to keep together
related server-side and client-side functionality in a single cohesive code
base. Adding a new feature requiring client-server communication has never been
so safe and swift. 

* Take advantage of C# language constructs on the client side, such as
  [LINQ](Linq-CSharp.md).
* Use a functional and reactive programming style with
  [WebSharper.UI](ui.md) to let the data flow through your UI.
* Develop libraries with self-contained client and/or server functionality to reuse in multiple projects.
* Many JavaScript libraries has typed interfaces for WebSharper available on NuGet, 
  or write your own using a concise and easily readable F# DSL.

## Client-server interaction facilities

Including client-side controls inside server-rendered pages and interacting
between the client and the server has never been easier.

* Keep your code base consistent by using the same data types on the server and
  the client.
* Share code between tiers: JavaScript-compiled code is also compiled normally
  to .NET, so you can write a function once and use it directly both on the
  server and on the client.
  Same HTML combinators that work on the server also work in client-side code. 
* Include client-side generated content and event handlers directly inside your page without any indirection.
* Alternatively, you can also include WebSharper client controls inside ASPX pages.
* Use automated remoting: doing an AJAX request is a simple call to your server-side function inside a client-side `async` block.
* Create links and requests (including JSON content) on the client based on the same router and
  serializer that the server uses.
  Or use the router to set up client-side routing, and you can generate links on the server that 
  will be handled by the client reactively.
* Communicate between the server and the client using
  WebSockets, with automatically serialized
  messages.

## Extra features

* Source mapping.
* Rosyn analyzer included, showing WebSharper-specific translation errors as you code.
* Metaprogramming: translate calls to specific methods with your custom logic or 
easily include JavaScript code generated at compile-time.

## Contributing
WebSharper is open-source with [Apache 3.0 license](https://github.com/intellifactory/websharper/blob/master/LICENSE.md), on [GitHub](https://github.com/intellifactory/websharper/).
The source of these documentation pages are found in the [websharper.docs](https://github.com/intellifactory/websharper.docs/) repository.
Issue reports and pull requests are welcome to both code and documentation.