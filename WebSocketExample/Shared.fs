namespace WebSocketExample

open WebSharper

[<JavaScript>]
module Shared =

    [<NamedUnionCases>]
    type ClientToServer =
    | ExampleRequest of a:int*b:int

    [<NamedUnionCases "type">]
    type ServerToClient =
    | [<Name "int">] ExampleResponse of int
    | [<Name "string">] ErrorResponse of string