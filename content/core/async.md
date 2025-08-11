---
title: Asynchronous workflows
---

WebSharper supports [F# asynchronous workflows][asyncs] on the client,
implementing them with JavaScript callbacks.

As in F#, a workflow of type `async<'T>` represents a program that can
be invoked to either succeed with a result `'T` asynchronously or fail
with an exception.  The limitations on the client are:

* All parallelism is cooperative as the JavaScript runtime is single-threaded. 
  You have to yield control inside a
  workflow to let other workflows execute.
  Yielding of control happens implicitly every time whenever F# inserts
  a call to an `AsyncBuilder` call.
  This includes where you use a `let` or `let!`, every iteration of a `for` or `while` loop, 
  or between consecutive statements on the top level of an async block.

* There is no way to use `Async.RunSynchronously`.

* Cancellation is supported, in standard .NET/F# ways. If a cancellation occurs
  when waiting on an aynchronous remote call, the response from the server gets
  discarded without converting the JSON back to an object graph.

[asyncs]: http://msdn.microsoft.com/en-us/library/dd233250.aspx

# Task-based asynchronicity in C#

WebSharper supports [C# Task-based Asynchronous Pattern][asyncs] on the client,
implementing them with JavaScript callbacks.

As in C#, an object of type `Task` or `Task<T>` represents a waiting, ongoing
or finished operation. The limitations on the client are:

* All parallelism is cooperative as the JavaScript runtime is single-threaded. 
  You have to yield control inside a
  task to let other workflows execute.
  The current supported way is to use `await Task.Delay(0)` to yield control
  periodically when you are defining a CPU-heavy computation for the client-side.
  `Task.Yield` is currently unavailable but planned.
  For example: 
  
```
	public async void LotsOfHelloWorld(int n)
	{
		for(int i = 0; i < n; i++)
		{
			Console.WriteLine("Hello world!"); // prints to JavaScript console
			if (i % 1000 == 0) await Task.Delay(0); // yield control regularly  
		}
	}
```

* Parallelization is possible with `WhenAny` and `WaitAll` methods.
	
* There is no way to use `Async.RunSynchronously`. Also, no `Task` static methods
  are available that are using a specific `TaskScheduler`.

* Currently `await` by the `IAsyncResult` interface is not supported, but planned.
  
[asyncs]: https://msdn.microsoft.com/en-us/library/mt674882.aspx