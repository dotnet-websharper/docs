---
title: Working with VS Code
description: Offline, privacy-first AI code reviews for F# and WebSharper in VS Code.
---

![Demo](/ws-ai-plugin/small-file-suggestion.gif)

**WS Code Review** is a Visual Studio Code extension for **offline, privacy-first** AI code reviews tailored to **F#** and **WebSharper**.

* Runs entirely on your machine via **Ollama**.
* Uses a local coder model (default: **`qwen2.5-coder:7b-instruct`**).
* **No code is sent to cloud services.**

> **Links**
> * Marketplace: https://marketplace.visualstudio.com/items?itemName=souvanxay.ws-code-review  
> * GitHub + README: https://github.com/Got17/ai-code-review-vscode

## Key features

* **Selection-based reviews** with **streamed markdown** and a **diff preview**.

* **Apply mode**:
  * Normal files: applies an improved **full file** — primarily the selected region changes, but related code may also be adjusted if required (e.g., imports, types, call sites).

  * Large files (≥ **600** lines): applies changes to the **selected region** only.

* **Model switcher** (in webview + command). Default model is `qwen2.5-coder:7b-instruct`.

* **RAG pill** (ON/OFF) in the top bar of the webview to enrich reviews with bundled references.

* **Shadow Git snapshots** (optional): accepted suggestions are snapshot-committed to a private repo in extension storage—your real repo is untouched.

* **AI preferences**: store style/constraints (e.g., "functional style", "no renames") and show them in the webview.

## Quick start

1. **Install** from the Marketplace (link above).

Here’s the updated **Quick start → Step 2** you can paste in:

2. Install and start **Ollama**:
    * Download and install Ollama for your OS (from the [official site](https://ollama.com/download)).

    * Pull the **coder** model:
    ```bash
    ollama pull qwen2.5-coder:7b-instruct
    ```

    * Run the **Ollama Server**
    ```bash
    ollama serve
    ```

    By default, Ollama serves at `http://localhost:11434`, open it in your browser to verify (you should see "Ollama is running").

> Tip: If you prefer another local model, pull it first (e.g., `ollama pull <model:tag>`) and run `WS Code Review: Change Ollama Model` on Command Palette to switch before running `Show Suggestion`.


3. Open an **`.fs`** file, **select code**, then run:

    * **WS Code Review: Show Suggestion**
    * Shortcut: **Ctrl+Alt+R** (Windows/Linux) or **Ctrl+Cmd+R** (macOS)

4. Review the streamed suggestion and **diff** → **Accept** or **Reject**.
5. (Optional) Click the **RAG pill** (ON/OFF) and press **Refresh** to re-run.
6. (Optional) Enable **Shadow Git** in settings to keep private snapshot history.

## Commands

* **WS Code Review: Show Suggestion** — run review on the current selection.
* **WS Code Review: Change Ollama Model** — pick a local model or enter a tag.
* **WS Code Review: Set / Show / Clear AI Preferences** — manage reviewer hints.
* **WS Code Review: Show Shadow Git History (Current File)** — browse snapshot commits.
* **WS Code Review: Clear Shadow Git History** — purge the private snapshot repo.

> See the GitHub **README** for the full command list, options, and troubleshooting.

## Settings (high level)

* `wsCodeReview.rag.enable` — toggle retrieval-augmented context (also via the RAG pill).
* `wsCodeReview.git.enable` — enable private Shadow Git snapshotting on **Accept**.

> Endpoint and model are configurable via **Change Ollama Model**.
> Default endpoint: `http://localhost:11434/api/generate`
> Default model: `qwen2.5-coder:7b-instruct`

## Privacy

This extension communicates **only** with a **local** Ollama server.
No source code or preferences are sent to external/cloud services.
Shadow Git history is stored in the extension’s private storage and can be cleared at any time.