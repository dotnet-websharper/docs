---
title: Asynchronous workflows
---

WebSharper supports [F# asynchronous workflows][https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/async-expressions] on the client,
implementing them with JavaScript callbacks.

As in F#, a workflow of type `Async<'T>` represents a program that can
be invoked to either succeed with a result `'T` asynchronously or fail
with an exception.  The limitations on the client are:

* All parallelism is cooperative as the JavaScript runtime is single-threaded. 
  You have to yield control inside a
  workflow to let other workflows execute.
  Yielding of control happens implicitly every time whenever F# inserts
  a call to an `AsyncBuilder` call.
  This includes where you use a `let` or `let!`, every iteration of a `for` or `while` loop, 
  or between consecutive statements on the top level of an async block.

* `Async.RunSynchronously` is not supported, as it would block the JavaScript event loop.

* Cancellation is supported, in standard .NET/F# ways. If a cancellation occurs
  when waiting on an aynchronous remote call, the response from the server gets
  discarded without converting the JSON back to an object graph.

# Task-based asynchronous workflows

Also supported are the `Task` and `Task<T>` types from .NET, which represent a waiting, ongoing
or finished operation.

Similar limitations apply as with `Async<'T>`. Use the `task` builder to create task workflows.

# Task-based asynchronicity in C#

WebSharper supports [C# Task-based Asynchronous Pattern][asyncs] on the client,
implementing them with JavaScript callbacks.

* Parallelization is possible with `Task.WhenAny` and `Task.WaitAll` methods.

* `.RunSynchronously()` and `.Wait()` is not supported, as it would block the JavaScript event loop.

* Use the `Async.StartAsTask` and `Async.AwaitTask` to convert between `Async<'T>` and `Task<'T>`, same as in .NET.
