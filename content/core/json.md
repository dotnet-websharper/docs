---
title: JSON API
---

WebSharper provides a convenient and readable JSON serialization format for F# types as well as C# classes. The structure of the JSON is inferred from the type, and can be customized using attributes. This format is usable both from the server and the client side.

## Using JSON on the server

WebSharper sitelets provide facilities to both parse JSON from HTTP requests and write it to HTTP responses.

* Parsing: [using the `[<Json>]` attribute](sitelets#customizing-siteletinfer).
* Writing: [using Content.Json](content#contentjson).

The `WebSharper.Json` type provides the following static methods:
* `Serialize : 'T -> string` serializes a value to string.
* `Deserialize : string -> 'T` deserializes a value from a string.

## Using JSON on the client

There is a `JSON` module available in `WebSharper.JavaScript` that provides access to underlying JavaScript functionality: 
* `Parse : string -> obj` uses JavaScript's `JSON.parse` to convert a string to a value (no attribute-based transformations).
* `Stringify :obj -> string` uses JavaScript's `JSON.stringify` to convert a value to a string (no attribute-based transformations).

Typed JSON serialization is also available on the client.
The `WebSharper.Json` type also provides the following static methods that only work on the client:

* `Encode : 'T -> obj` converts a value to a JavaScript object that is not yet stringified, such that `JSON.Stringify (Json.Encode x) = Json.Serialize x`.
* `Decode : obj -> 'T` converts a parsed JavaScript object to a value, such that `Json.Decode (JSON.Parse s) = Json.Deserialize s`.

## Automatic JSON use

WebSharper automatically uses JSON de/serialization for the following cases:
* Remote method calls
* Creating a `Web.Control` (including using the `client` helper) and passing it server-side values
* Sitelet endpoints with a field with `[<Json>]` attribute
* Returning a `Content.Json` value from a sitelet endpoint

## Supported types

JSON serializers are automatically derived for the following types,
where any generic parameters are also JSON-serializable types:

* `unit`
* `bool`
* numeric types: `byte`, `sbyte`, `int16`, `uint16`, `int`, `uint32`, `int64`, `uint64`, `single`, `float`
* `decimal` (on the client side, requires the `WebSharper.MathJS` package)
* `string`
* `char`
* `System.DateTime`
* `System.DateTimeOffset`
* `System.TimeSpan`
* `System.Guid`
* `System.Numerics.BigInteger`
* one-dimensional arrays: `'T []`
* tuples: `'T1 * 'T2 * ... * 'Tn`
* `System.Nullable<'T>`
* unions (including `option` and `list`)
* records
* enums
* `Map<'K, 'V>`
* `Set<'T>`
* `System.Collections.Generic.List<'T>`
* `System.Collections.Generic.Queue<'T>`
* `System.Collections.Generic.Stack<'T>`
* `System.Collections.Generic.LinkedList<'T>`
* `System.Collections.Generic.Dictionary<'K, 'V>`
* classes with a parameterless default constructor
* structs with a constructor that initializes all fields from parameters

For records, unions, classes and structs to be JSON-serializable, all their
fields must also be JSON-serializable.

## Extensibility

Adding custom support for other types is currently supported only on the client side.
Add static methods to your type or proxy `EncodeJson: 'T -> obj` and `DecodeJson: obj -> 'T`.
These should convert between a JSON-compatible plain JavaScript object and your type.

## Format

### Base types

The following base types are handled:

* Integers: `int8`, `int16`, `int32` (aka `int`), `int64`

```fsharp
Content.Json 12y

// Output: 12
```

* Unsigned integers: `uint8` (aka `byte`), `uint16`, `uint32`, `uint64`

```fsharp
Content.Json 12ul

// Output: 12
```

* Floats: `single`, `double` (aka `float`)

```fsharp
Content.Json 12.34

// Output: 12.34
```

* Decimals: `decimal`

```fsharp
Content.Json 12.34m

// Output: "12.34"
```

* Strings: `string` and `char`

```fsharp
Content.Json """A string with some "content" inside"""

// Output: "A string with some \"content\" inside"
```

* Booleans: `bool`

```fsharp
Content.Json true

// Output: true
```

### Collections

Values of type `list<'T>`, `'T[]`, `ResizeArray<'T>`, `Set<'T>`, `Queue<'T>`, and `Stack<'T>` are represented as JSON arrays:

```fsharp
Content.Json [|"a string"; "another string"|]

// Output: ["a string", "another string"]

Content.Json (Set ["a string"; "another string"])

// Output: ["another string", "a string"]
```

Values of type `Map<string, 'T>` and `System.Collections.Generic.Dictionary<string, 'T>` are represented as flat JSON objects:

```fsharp
Content.Json (Map [("somekey", 12); ("otherkey", 34)])

// Output: {"somekey": 12, "otherkey": 34}
```

Other `Map` and `Dictionary` values are represented as an array of key-value pairs:

```fsharp
Content.Json (Map [(1, 12); (3, 34)])

// Output: [[1, 12], [3, 34]]
```

### Tuples

Tuples (including struct tuples) are also represented as JSON arrays:

```fsharp
Content.Json ("a string", "another string")

// Output: ["a string", "another string"]

Content.Json (struct ("a string", "another string")

// Output: ["another string", "a string"]
```

<a name="fs-records"></a>
### F# Records

F# records are represented as flat JSON objects. The attribute `[<Name "name">]` can be used to customize the field name:

```fsharp
type Name =
    {
        [<Name "first-name">] FirstName: string
        LastName: string
    }

type User =
    {
        name: Name
        age: int
    }

Content.Json {name = {FirstName = "John"; LastName = "Doe"}; age = 42}

// Output: {"name": {"first-name": "John", "LastName": "Doe"}, "age": 42}
```

### F# Unions

Union types intended for use in JSON serialization should optimally bear the attribute `NamedUnionCases` for producing fully readable JSON format.
There are two ways to use it, specifying a field name to hold the union case name or signaling that the case should be inferred from the field names.
If no `NamedUnionCases` is present, a `"$"` field will be used for storing the case index.

#### Explicit discriminator

With `[<NamedUnionCases "field">]`, the union value is represented as a JSON object with a field called `"field"`, whose value is the name of the union case, and as many other fields as the union case has arguments. You can use `[<Name "name">]` to customize the name of a union case.

```fsharp
[<NamedUnionCases "kind">]
type Contact =
    | [<Name "address">]
        Address of street: string * zip: string * city: string
    | Email of email: string

Content.Json
    [
        Address("12 Random St.", "15243", "Unknownville")
        Email "john.doe@example.com"
    ]

// Output: [
//           {"kind": "address",
//            "street": "12 Random St.",
//            "zip": "15243",
//            "city": "Unknownville"},
//           {"kind": "Email",
//            "email": "john.doe@example.com"}
//         ]
```

Unnamed fields receive the names `Item1`, `Item2`, etc.

Missing the attribute, the case name would be not stored in a readable form:

```fsharp
type Contact =
    | Address of street: string * zip: string * city: string
    | Email of email: string

Content.Json
    [
        Address("12 Random St.", "15243", "Unknownville")
        Email "john.doe@example.com"
    ]

// Output: [
//           {"$": 0,
//            "street": "12 Random St.",
//            "zip": "15243",
//            "city": "Unknownville"},
//           {"$": 1,
//            "email": "john.doe@example.com"}
//         ]
```

#### Implicit discriminator

With an argumentless `[<NamedUnionCases>]`, no extra field is added to determine the union case; instead, it is inferred from the names of the fields present. This means that each case must have at least one mandatory field that no other case in the same type has, or a compile-time error will be thrown.

```fsharp
[<NamedUnionCases>]
type Contact =
    | Address of street: string * zip: string * city: string
    | Email of email: string

Content.Json
    [
        Address("12 Random St.", "15243", "Unknownville")
        Email "john.doe@example.com"
    ]

// Output: [
//           {"street": "12 Random St.",
//            "zip": "15243",
//            "city": "Unknownville"},
//           {"email": "john.doe@example.com"}
//         ]
``` 

#### Record inside union

As a special case, if a union case has a single, unnamed argument which is a record, then the fields of this record are used as the fields of the output object.

```fsharp
type Address = { street: string; zip: string; city: string }

[<NamedUnionCases>]
type Contact =
    | Address of Address
    | Email of email: string

Content.Json
    [
        Address {
            street = "12 Random St."
            zip = "15243"
            city = "Unknownville"
        }
        Email "john.doe@example.com"
    ]

// Output: [
//           {"street": "12 Random St.",
//            "zip": "15243",
//            "city": "Unknownville"},
//           {"email": "john.doe@example.com"}
//         ]
```

### Optional fields

Fields with type `option<'T>` are represented as a field that may or may not be there. This is the case both for unions and records.

```fsharp
[<NamedUnionCases>]
type Contact =
    | Address of street: string * zip: string * city: string option
    | Email of email: string

type User =
    {
        fullName: string
        age: int option
        contact: Contact
    }

Content.Json
    [
        {
            fullName = "John Doe"
            age = Some 42
            contact = Address("12 Random St.", "15243", Some "Unknownville")
        }
        {
            fullName = "Jane Doe"
            age = None
            contact = Address("53 Alea St.", "51423", None)
        }
    ]

// Output: [
//           {"fullName": "John Doe",
//            "age": 42,
//            "contact":{"street": "12 Random St.",
//                       "zip": "15243",
//                       "city": "Unknownville"}},
//           {"fullName": "Jane Doe",
//            "contact":{"street": "53 Alea St.",
//                       "zip": "51423"}}
//         ]
```

When parsing JSON, `null` is also accepted as a `None` value.

#### Constant cases

Union cases annotated with the attribute `[<Constant c>]` are represented as the corresponding constant, which can be a string, int, float or bool. It is recommended to only use this attribute on argument-less cases. If all cases of a union are annotated with `[<Constant>]`, then `[<NamedUnionCases>]` is not necessary.

```fsharp
type Color =
    | [<Constant "blue">] Blue
	| [<Constant "red">] Red
	| [<Constant "green">] Green

Content.Json [Blue; Red; Green]

// Output: ["blue","red","green"]
```

### Classes

In order to be serializable to/from JSON on the server side, a class must be annotated with the `[<System.Serializable>]` attribute and must have a default constructor.
On the client side, these are not checked or required.
Then, it is serialized based on its fields, similarly to F# records [as mentioned above](#f-records).
Here is an example in C#:

```csharp
[Serializable]
public class User
{
    Name name;
    int age;
    
    public User() { }

    public User(Name name, int age)
    {
        this.name = name;
        this.age = age;
    }
}

[Serializable]
public class Name
{
    [Name("first-name")] string firstName;
    string lastName;
    
    public Name() { }

    public Name(string firstName, string lastName)
    {
        this.firstName = firstName;
        this.lastName = lastName;
    }
}
```

Then from F#:

```fsharp
Content.Json (User(Name("John", "Doe"), 36));

// Output: {"name": {"first-name": "John", "lastName": "Doe"}, "age": 36}
```

### DateTimes

Values of type `System.DateTime` are encoded using an ISO 8601 round-trip format string:

```fsharp
Content.Json System.DateTime.UtcNow

// Output: "2015-03-06T17:05:19.2077851Z"
```

The format can be customized with the attribute `[<DateTimeFormat>]`. 
This attribute can be placed either on a record field of type `System.DateTime` or `option<System.DateTime>`, or on a union case with an argument of one of these types.

```fsharp
type Action =
    {
        [<DateTimeFormat "yyyy-MM-dd">] dateOnly: System.DateTime
    }

Content.Json { dateOnly = System.DateTime.UtcNow }

// Output: { dateOnly: "2015-03-24" }

[<NamedUnionCases>]
type Action =
    | [<DateTimeFormat("time", "HH.mm.ss")>] A of time: System.DateTime

Content.Json (A (time = System.DateTime.UtcNow))

// Output: { time: "15.03.32" }
```

Note however that `[<DateTimeFormat>]` is only available on the server side; this attribute is ignored by client-side serialization. Do not use it if you want to use your data type for remoting.
