import { Tab, Tabs } from "fumadocs-ui/components/tabs";
import {CustomSyntaxHighlighterWithCopy} from 'lib/components/CustomSyntaxHighlighter';
import fs from "node:fs";
import { extractBetweenMarkers } from "./shared";

const basePath = process.env.GHREPO !== undefined ? "/" + process.env.GHREPO : "";

export function CSharpSnippetSimple({ snippet, liveSnippetHeight = "600", highlightLines = "", defTab = "" }: Props) {
  const csCode = extractBetweenMarkers(fs.readFileSync(`snippets/${snippet}/Client.cs`, 'utf-8'));  
  let defIndex = 0
  switch (defTab) {
      case "csharp":
          defIndex = 0;
          break;
      case "preview":
          defIndex = 1;
          break;
  }
  return (
    <Tabs items={["F#", "Result"]} defaultIndex={defIndex}>
      <Tab value="F#" className="not-prose">
        <CustomSyntaxHighlighterWithCopy id={`snippet_${snippet}_cs`} code={csCode} language="csharp" highlightLines={highlightLines}></CustomSyntaxHighlighterWithCopy>
      </Tab>
      <Tab value="Result">
        <div style={{ display: "flex", justifyContent: "flex-end", marginBottom: "0.5rem" }}>
          <a
            href={`${basePath}/snippets/${snippet}/index.html`}
            target="_blank"
            rel="noopener noreferrer"
            style={{
              fontSize: "0.875rem",
              padding: "0.25rem 0.5rem",
              border: "1px solid #ccc",
              borderRadius: "4px",
              textDecoration: "none",
              backgroundColor: "#f8f8f8",
              color: "#333",
            }}
          >
            Open in new tab ↗
          </a>
        </div>
        <iframe
          src={`${basePath}/snippets/${snippet}/index.html`}
          style={{
            width: "100%",
            height: `${liveSnippetHeight}px`,
            border: "1px solid #ccc",
            borderRadius: "6px",
          }}
        />
      </Tab>
    </Tabs>
  );
}

interface Props {
  snippet: string;
  liveSnippetHeight?: string;
  highlightLines?: string;
  defTab?: string;
}
