---
title: Working with vscode (Experimental)
---

A Visual Studio Code extension designed for **offline, privacy AI-assisted code reviews** tailored to **F#** and **WebSharper** development.
It integrates with a **local Large Language Model (LLM)** (e.g., Ollama running `qwen2.5-coder:7b-instruct`) to provide real-time suggestions and improvements for selected code, with strict respect for user-defined preferences.

The system never sends code to cloud services, ensuring **full code privacy**.

## **Features**

* **AI-Powered F# Code Review**:
  Reviews *only the selected code* while considering full file context.
* **Local LLM Integration**:
  Connects to `http://localhost:11434/api/generate` (Ollama API).
* **Custom AI Preferences**:
  Store and modify preferences (e.g., "no renames", "functional style").
* **Git Integration**:
  * View staged/unstaged changes.
  * Undo last AI suggestion.
* **Webview-based UI**:
  Interactive suggestion panel with Accept/Reject options.

## **Workflow**

1. **User selects F# code** in the editor.
    ![User Select Code](/ws-ai-plugin/select-code.png)

2. **Trigger the `Show Suggestion` command** from the Command Palette (`Ctrl + Shift + P` on Windows/Linux, `Cmd + Shift + P` on macOS). 

    ![Trigger Show Suggestion Command](/ws-ai-plugin/trigger-show-suggestion.png)

3. **Extension builds AI prompt** (`buildPrompt`) including:
   * Selected snippet
   * Full file content
   * User preferences
   * Sends prompt to local AI API (`queryAIStream`).
4. AI streams back suggestions to **Suggestion Webview**.
    ![Suggestion Webview Panel 1](/ws-ai-plugin/suggestion-webview-1.png)
    ![Suggestion Webview Panel 2](/ws-ai-plugin/suggestion-webview-2.png)

5. User can:

   * **Accept** (applies improved full file)

      Before Accept: 
      ![Before Accept Button](/ws-ai-plugin/select-code.png)

      After Accept:
      ![After Accept Button](/ws-ai-plugin/after-accept-button.png)
   
   * **Reject** (closes panel without changes).
6. Optionally **Undo Last Suggestion** (via Git checkout).

## **Security & Privacy**

* No external API calls except to **local AI server**.
* User code and preferences are **never sent to cloud services**.
* Preferences stored locally via VS Code `globalState`.

## **Technical Requirements**

* **VS Code 1.85+**
* **Node.js 18+**
* **Ollama** installed and running locally.
* Model: `qwen2.5-coder:7b-instruct` (configurable via `constants.ts`).

## **Example Usage**

```plaintext
Highlight a block of F# code.
Run "AI Code Review: Show Suggestion".
View suggested changes in side panel.
Accept or reject AI improvements.
Use "Undo Last Suggestion" if needed.
```