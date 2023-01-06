---
order: -50
description: Construct JSON on the server or the client.
label: WebSharper.Json
expanded: false
icon: quote
---

# WebSharper JSON

==- Where is it?

* NuGet package: `WebSharper`
* DLL: `WebSharper.Main.dll`
* Namespace: `WebSharper.Json`

===

WebSharper.Json provides a convenient and developer-friendly JSON serialization format for F\# types \(and C\# classes\) for both client-side and server-side scenarios. The JSON structure is automatically inferred from the types involved and can be further customized using attributes. See the [JSON format](/json/format.md) page for more details.

---

## Using JSON on the server

The `WebSharper.Json` module provides F# value de/serialization for server-side use via `Serialize` and `Deserialize`:

### `Serialize`

`Serialize : 'T -> string` serializes a value to string.

{% tabs %}
{% tab title="F\#" %}

```fsharp
open WebSharper

[<NamedUnionCases "kind">]
type Contact =
    | [<Name "address">] Address of street:string * zip:string * city:string
    | Email of email:string

Json.Serialize
    [
        Address("12 Random St.", "15243", "Unknownville")
        Email "john.doe@example.com"
    ]
```

{% endtab %}

{% tab title="Result" %}

```json
[
    {"kind": "address",
     "street": "12 Random St.",
     "zip": "15243",
     "city": "Unknownville"},
    {"kind": "Email",
     "email": "john.doe@example.com"}
]
```

{% endtab %}
{% endtabs %}

---

### `Deserialize`

`Deserialize<'T> : string -> 'T` deserializes a value from a string. You can pass the desired output type as a type parameter.

{% tabs %}
{% tab title="F\#" %}

```fsharp
"""[
    {"kind": "address",
     "street": "12 Random St.",
     "zip": "15243",
     "city": "Unknownville"},
    {"kind": "Email",
     "email": "john.doe@example.com"}
]"""
|> Json.Deserialize<Contact list>
```

{% endtab %}

{% tab title="Result (F#)" %}

```fsharp
val it : Contact list =
    [Address ("12 Random St.", "15243", "Unknownville");
     Email "john.doe@example.com"]
```

{% endtab %}
{% endtabs %}

---

### JSON in sitelets

WebSharper [sitelets](/sitelets/README.md) provide facilities both to parse JSON payloads from HTTP requests and to return JSON as HTTP responses, greatly reducing the effort needed to implement microservices and REST-based services, among others. Most typically, JSON is sent as a POST request to an endpoint and returned as response from another.

This can be accomplished by using the `[<Json>]` attribute on parts of an endpoint type to enable receiving JSON and parsing it into F#/C# types, and using `Content.Json` to return a JSON response in the opposite direction.

The following code demonstrates both:

```fsharp #14,26
module Site

open WebSharper
open WebSharper.Sitelets

type Person =
    {
        FirstName: string
        LastName: string
        Age: int
    }

type EndPoint =
    | [<EndPoint "POST /receive"; Json "data">] Receive of data:Person
    | [<EndPoint "GET /send">] Send

[<Website>]
let Main = Sitelet.Infer (fun ctx -> function
    | Receive data ->
        sprintf "data received: %A" data |> Content.Text
    | Send ->
        let john =
            {
                FirstName = "John"; LastName = "Smith"; Age = 32
            }
        Content.Json john
)
```

---

## Using JSON on the client

JSON serialization is also available on the client. `WebSharper.Json` provides the following functions:

### `Parse`

`Parse : string -> obj` uses JavaScript's `JSON.parse` to convert a string to a value (no attribute-based transformations).

---

### `Stringify`

`Stringify : obj -> string` uses JavaScript's `JSON.stringify` to convert a value to a string (no attribute-based transformations).

---

### `Encode`

`Encode : 'T -> obj` converts a value to a JavaScript object, such that `Json.Stringify (Json.Encode x) = Json.Serialize x`.

---

### `Decode`

`Decode : obj -> 'T` converts a JavaScript object to a value, such that `Json.Decode (Json.Parse s) = Json.Deserialize s`.

---

### `Activate`

`Activate : obj -> 'T` parses/activates a JSON object returned by the server. This function is used internally to activate values returned from RPC functions.
