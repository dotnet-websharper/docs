---
order: -10
description: F#/C# to JSON
label: JSON format
---

# JSON format

This page describes how F# values (and C# objects) are converted to JSON by the [functions available](/json/README.md) in `WebSharper.Json`.

---

## Customization


If you would like to customize the default JSON representation, the following attributes are available (see examples in the sections below):

* `[<Name>]` to customize field names in F# [records](/json/format/#records) and union cases in [discriminated unions](/json/format/#unions) (DUs).

* `[<NamedUnionCases>]` to customize the name of the [discriminator field](/json/format/#explicit-discriminator) that determines which DU shape the value holds.

* `[<Constant>]` to represent DU cases as [constants/literals](/json/format/#constant-cases).

* `[<DateTimeFormat>]` (server-side only) to customize the format used for [`System.DateTime` values](/json/format/#datetimes).

* `[<System.Serializable>]` to mark [C#/F# classes](/json/format/#classes) to be JSON serializable.

---

## Base types

The following base types are handled:

* signed and unsigned integers: `uint8 (byte)`/`int8`, `uint16`/`int16`, `uint32`/`int32` \(`int`\), `uint64`/`int64`
* floats: `single`, `double (float)`
* decimals: `decimal`
* strings: `string`
* booleans: `bool`

| F\# | Output |
| :--- | :--- |
| `Content.Json 12y` | 12 |
| `Content.Json 12ul` | 12 |
| `Content.Json 12.34` | 12.34 |
| `Content.Json 12.34m` | 12.34 |
| `Content.Json """A string with some "content" inside"""` | "A string with some \"content\" inside" |
| `Content.Json true` | true |

---

## Collections

* Values of type `list<'T>`, `'T[]` and `Set<'T>` are represented as JSON arrays.
* Values of type `Map<string, 'T>` and `System.Collections.Generic.Dictionary<string, 'T>` are represented as flat JSON objects.
* Other `Map` and `Dictionary` values are represented as an array of key-value pairs.

| F\# | Output |
| :--- | :--- |
| `Content.Json [|"a string"; "another string"|]` | \["a string", "another string"\] |
| `Content.Json (Set ["a string"; "another string"])` | \["another string", "a string"\] |
| `Content.Json (Map [("somekey", 12); ("otherkey", 34)])` | {"somekey": 12, "otherkey": 34} |
| `Content.Json (Map [(1, 12); (3, 34)])` | \[\[1, 12\], \[3, 34\]\] |

---

## Tuples

Tuples \(including struct tuples\) are also represented as JSON arrays:

| F\# | Output |
| :--- | :--- |
| `Content.Json ("a string", "another string")` | \["a string", "another string"\] |
| `Content.Json (struct ("a string", "another string")` | \["another string", "a string"\] |

---

## Records

F\# records are represented as flat JSON objects. The attribute `[<Name "name">]` can be used to customize the field name:

{% tabs %}
{% tab title="F\#" %}

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

Content.Json { name={ FirstName="John"; LastName="Doe" }; age=42 }
```

{% endtab %}

{% tab title="Output" %}

```json
{"name": {"first-name": "John", "LastName": "Doe"}, "age": 42}
```

{% endtab %}
{% endtabs %}

---

## Unions

Union types intended for use in JSON serialization should optimally bear the attribute `NamedUnionCases` for producing fully readable JSON format. There are two ways to use it, specifying a field name to hold the union case name, or signaling that the case should be inferred from the field names. If no `NamedUnionCases` is present, a `"$"` field will be used for storing the case index.

---

### Explicit discriminator

With `[<NamedUnionCases "field">]`, the union value is represented as a JSON object with a field called `"field"`, whose value is the name of the union case, and as many other fields as the union case has arguments. You can use `[<Name "name">]` to customize the name of a union case.

{% tabs %}
{% tab title="F\#" %}

```fsharp
[<NamedUnionCases "kind">]
type Contact =
    | [<Name "address">] Address of street:string * zip:string * city:string
    | Email of email:string

Content.Json
    [
        Address("12 Random St.", "15243", "Unknownville")
        Email "john.doe@example.com"
    ]
```

{% endtab %}

{% tab title="Output" %}

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

Unnamed arguments receive the names `Item1`, `Item2`, etc.

Missing the `[<NamedUnionCases>]` attribute, the case names would be not stored in a readable form:

{% tabs %}
{% tab title="F\#" %}

```fsharp
type Contact =
    | Address of street:string * zip:string * city:string
    | Email of email:string

Content.Json
    [
        Address("12 Random St.", "15243", "Unknownville")
        Email "john.doe@example.com"
    ]
```

{% endtab %}

{% tab title="Output" %}

```json
[
    {"$": 0,
     "street": "12 Random St.",
     "zip": "15243",
     "city": "Unknownville"},
    {"$": 1,
     "email": "john.doe@example.com"}
]
```

{% endtab %}
{% endtabs %}

---

### Implicit discriminator

With an argumentless `[<NamedUnionCases>]`, no extra field is added to determine the union case; instead, it is inferred from the names of the fields present. This means that each case must have at least one mandatory field that no other case in the same type has, or a compile-time error will be thrown.

{% tabs %}
{% tab title="F\#" %}

```fsharp
[<NamedUnionCases>]
type Contact =
    | Address of street:string * zip:string * city:string
    | Email of email: string

Content.Json
    [
        Address("12 Random St.", "15243", "Unknownville")
        Email "john.doe@example.com"
    ]
```

{% endtab %}

{% tab title="Output" %}

```json
[
    {"street": "12 Random St.",
     "zip": "15243",
     "city": "Unknownville"},
    {"email": "john.doe@example.com"}
]
```

{% endtab %}
{% endtabs %}

---

### Record inside union

As a special case, if a union case has a single, unnamed record argument, then the fields of this record are used as the fields of the output object.

{% tabs %}
{% tab title="F\#" %}

```fsharp
type Address = { street: string; zip: string; city: string }

[<NamedUnionCases>]
type Contact =
    | Address of Address
    | Email of email:string

Content.Json
    [
        Address {
            street = "12 Random St."
            zip = "15243"
            city = "Unknownville"
        }
        Email "john.doe@example.com"
    ]
```

{% endtab %}

{% tab title="Output" %}

```json
[
    {"street": "12 Random St.",
     "zip": "15243",
     "city": "Unknownville"},
    {"email": "john.doe@example.com"}
]
```

{% endtab %}
{% endtabs %}

---

## Optional fields

Fields with type `option<'T>` are represented as a field that may or may not be there. This is the case both for unions and records.

{% tabs %}
{% tab title="F\#" %}

```fsharp
[<NamedUnionCases>]
type Contact =
    | Address of street:string * zip:string * city:string option
    | Email of email:string

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

```

{% endtab %}

{% tab title="Output" %}

```json
[
    {"fullName": "John Doe",
     "age": 42,
     "contact":{"street": "12 Random St.",
                "zip": "15243",
                "city": "Unknownville"}},
    {"fullName": "Jane Doe",
     "contact":{"street": "53 Alea St.",
                "zip": "51423"}}
]
```

{% endtab %}
{% endtabs %}

When parsing JSON, `null` is also accepted as a `None` value.

---

### Constant cases

Union cases annotated with the attribute `[<Constant "c">]` are represented as the corresponding constant, which can be a `string`, `int`, `float` or `bool`. It is recommended to only use this attribute on argument-less cases. If all cases of a union are annotated with `[<Constant>]`, then `[<NamedUnionCases>]` is not necessary.

{% tabs %}
{% tab title="F\#" %}

```fsharp
type Color =
    | [<Constant "blue">] Blue
    | [<Constant "red">] Red
    | [<Constant "green">] Green

Content.Json [Blue; Red; Green]
```

{% endtab %}

{% tab title="Output" %}

```json
["blue","red","green"]
```

{% endtab %}
{% endtabs %}

---

## Classes

In order to be serializable to/from JSON on the server-side, a class must be annotated with the `[<System.Serializable>]` attribute and must have a default constructor. On the client-side, these are not checked or required. Then, it is serialized based on its fields, similarly to [F\# records as mentioned above](/json/format.md#records). Here is an example in C\#:

{% tabs %}
{% tab title="C\#" %}

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

{% endtab %}

{% tab title="Query" %}

```csharp
Content.Json(User(Name("John", "Doe"), 36))
```

{% endtab %}

{% tab title="Output" %}

```json
{"name": {"first-name": "John", "lastName": "Doe"}, "age": 36}
```

{% endtab %}
{% endtabs %}

---

## DateTimes

Values of type `System.DateTime` are encoded using an ISO 8601 round-trip format string:

{% tabs %}
{% tab title="F\#" %}

```fsharp
Content.Json System.DateTime.UtcNow
```

{% endtab %}

{% tab title="Output" %}

```json
"2015-03-06T17:05:19.2077851Z"
```

{% endtab %}
{% endtabs %}

The format can be customized with the `[<DateTimeFormat>]` attribute. This attribute can be placed either on a record field of type `System.DateTime` , or `option<System.DateTime>`, or on a union case with an argument of one of these types.

{% tabs %}
{% tab title="F\#" %}

```fsharp
open System

type MyType =
    {
        [<DateTimeFormat "yyyy-MM-dd">] DateOnly:DateTime
    }

Content.Json { DateOnly = DateTime.UtcNow }
```

{% endtab %}

{% tab title="Output" %}

```json
{ DateOnly: "2015-03-24" }
```

{% endtab %}
{% endtabs %}

{% tabs %}
{% tab title="F\#" %}

```fsharp
[<NamedUnionCases>]
type MyTime =
    | [<DateTimeFormat("time", "HH.mm.ss")>] Time of time:DateTime

Content.Json (Time DateTime.UtcNow)
```

{% endtab %}

{% tab title="Output" %}

```json
{ time: "15.03.32" }
```

{% endtab %}
{% endtabs %}

Note, however, that `[<DateTimeFormat>]` is only available on the server side; this attribute is ignored by client-side serialization.
