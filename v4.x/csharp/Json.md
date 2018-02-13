# JSON API

WebSharper provides a convenient and readable JSON serialization format for C# classes as well as F# types. The structure of the JSON is inferred from the type, and can be customized using attributes. This format is usable both from the server and the client side.

## Using JSON on the server

WebSharper Sitelets provide facilities to both parse JSON from HTTP requests and write it to HTTP responses.

* Parsing: [using the `[Json]` attribute](Sitelets.md#json-request).
* Writing: [using Content.Json](Sitelets.md#json-response).

The `WebSharper.TypedJson` class provides the following static methods:
* `string Serialize(T)` serializes a value to string.
* `string Deserialize(T)` deserializes a value from a string.

## Using JSON on the client

JSON serialization is also available on the client.

The `WebSharper.Json` class provides static methods for converting between a string and a plain JavaScript object (no custom de/serialization).

* `obj Parse(string)` uses JavaScript's `JSON.parse` to convert a string to a value.
* `string Stringify(obj)` uses JavaScript's `JSON.stringify` to convert a value to a string.

The `Serialize`/`Deserialize` methods of `WebSharper.TypedJson` are also working in on the client, 
and there are two additional static methods that are only working on the client:

* `obj Encode(T)` converts a value to a JavaScript object, such that `Json.Stringify(TypedJson.Encode(x)) == TypedJson.Serialize(x)`.
* `T Decode(obj)` converts a JavaScript object to a value, such that `TypedJson.Decode(Json.Parse(s)) == TypedJson.Deserialize(s)`.

## Format

### Base types

The following base types are handled:

* Integers: `int8`, `int16`, `int`, `int64`

```csharp
Content.Json(12)

// Output: 12
```

* Unsigned integers: `byte`, `uint16`, `uint32`, `uint64`

```csharp
Content.Json((byte)12)

// Output: 12
```

* Floats: `single`, `double`

```csharp
Content.Json(12.34)

// Output: 12.34
```

* Decimals: `decimal`

```csharp
Content.Json((decimal)12.34)

// Output: 12.34
```

* Strings: `string`

```csharp
Content.Json("A string with some \"content\" inside")

// Output: "A string with some \"content\" inside"
```

* Booleans: `bool`

```csharp
Content.Json(true)

// Output: true
```

### Collections

Values of type `System.Collections.Generic.List<T>` and `T[]` are represented as JSON arrays:

```csharp
Content.Json(new[] { "a string", "another string" })

// Output: ["a string", "another string"]

Content.Json(new List<string> { "a string", "another string" })

// Output: ["a string", "another string"]
```

Values of type `System.Collections.Generic.Dictionary<string, 'T>` are represented as flat JSON objects:

```csharp
Content.Json(new Dictionary<string, int> { { "somekey", 12 }, { "otherkey", 34 } })

// Output: {"somekey": 12, "otherkey": 34}
```

Other`Dictionary` values are represented as an array of key-value pairs:

```csharp
Content.Json(new Dictionary<string, int> { { 1, 12 }, { 3, 34 } })

// Output: [[1, 12], [3, 34]]
```

### Classes

In order to be serializable to/from JSON on the server-side, a class must be annotated with the `[<System.Serializable>]` attribute and must have a default constructor.
On the client-side, these are not checked or required.
Then, it is serialized based on all of its fields which are not marked `NonSerialized`.

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

Content.Json(new User(new Name("John", "Doe"), 36));

// Output: {"name": {"first-name": "John", "lastName": "Doe"}, "age": 36}
```

### DateTimes

Values of type `System.DateTime` are encoded using an ISO 8601 round-trip format string:

```csharp
Content.Json(System.DateTime.UtcNow)

// Output: "2015-03-06T17:05:19.2077851Z"
```

The format can be customized with the attribute `[<DateTimeFormat>]`.

```csharp
public class Action
{
    [DateTimeFormat("yyyy-MM-dd")] public DateTime dateOnly;
}

Content.Json(new Action() { dateOnly = System.DateTime.UtcNow })

// Output: { dateOnly: "2015-03-24" }

Note however that `[<DateTimeFormat>]` is only available on the server side; this attribute is ignored by client-side serialization.
