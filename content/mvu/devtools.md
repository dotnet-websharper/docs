---
title: Redux DevTools integration
---

WebSharper.Mvu integrates seamlessly with [Redux DevTools](https://github.com/reduxjs/redux-devtools).
This browser extension allows you to inspect the successive messages and states of your model, and even to replay old states and see the effect on your view.

## Installation

See the [Installation readme](https://github.com/reduxjs/redux-devtools/tree/main/extension#installation) for a link to the extension for your browser.

## Code integration

Redux DevTools integration is applied using a single call to `App.WithReduxDevTools`:

```fsharp
let Main() =
    App.Create initialModel update render
    |> App.WithReduxDevTools
    |> App.Run
```

This integration uses [WebSharper.Json](../core/json) serialization to communicate with RemoteDev.
The tool expects message values to be objects with a `"type"` field on messages. Therefore, you should use as `Message` type a discriminated union annotated like follows:

```fsharp
[<NamedUnionCases "type">]
type Message =
    | Message1 of id: int * value: string
    | // other message types...
```

Given the above, the value:

```fsharp
Message1 (42, "Hello world!")
```

will be sent to RemoteDev as:

```json
{ "type": "Message1", "id": 42, "value": "Hello world!" }
```

It will show up in the DevTools extension as labeled as a "Message1" action.

### Options

To customize the integration, you can pass an options value to `App.WithReduxDevToolsOptions`. For example, to set the name of your application in the DevTools, you can do:

```fsharp
let Main() =
    App.Create initialModel update render
    |> App.WithReduxDevToolsOptions (ReduxDevTools.Options(name = "MyApp"))
    |> App.Run
```

### Supported features

Not all features of Redux DevTools are supported. The following features are currently implemented:

* Time travel: you can use the "Jump" button on actions to go back to a previous state.
* Reset: you can reset the state to the initial model.
* Revert: you can revert to the last committed state.
* Commit: you can commit the current state to jump back to with revert and clean up the action history.

Other features such as skipping and reordering actions are not yet implemented.