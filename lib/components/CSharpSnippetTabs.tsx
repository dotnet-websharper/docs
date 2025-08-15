import { Tab, Tabs } from "fumadocs-ui/components/tabs";
import {CustomSyntaxHighlighterWithCopy} from 'lib/components/CustomSyntaxHighlighter';
import fs from "node:fs";
import { extractBetweenMarkers } from "./shared";

const basePath = process.env.GHREPO !== undefined ? "/" + process.env.GHREPO : "";

export function CSharpSnippetTabs({ snippet, liveSnippetHeight = "600", highlightLines = "", defTab = "" }: CSharpSnippetTabsProps) {
    const csCode = extractBetweenMarkers(fs.readFileSync(`snippets/${snippet}/Client.cs`, 'utf-8'));
    const htmlCode = extractBetweenMarkers(fs.readFileSync(`snippets/${snippet}/wwwroot/index.html`, 'utf-8'));
    let defIndex = 0
    switch (defTab) {
        case "csharp":
            defIndex = 0;
            break;
        case "html":
            defIndex = 1;
            break;
        case "preview":
            defIndex = 2;
            break;
    }
    return (
        <Tabs items={["C#", "index.html", "Result"]} defaultIndex={defIndex}>
            <Tab value="C#" className="not-prose text-sm">
                <CustomSyntaxHighlighterWithCopy 
                    id={`snippet_${snippet}_cs`} 
                    code={csCode} language="csharp" 
                    highlightLines={highlightLines}
                /> 
            </Tab>
            <Tab value="index.html" className="not-prose text-sm">
                <CustomSyntaxHighlighterWithCopy id={`snippet_${snippet}_cs`} code={htmlCode} language="html" highlightLines=""></CustomSyntaxHighlighterWithCopy>
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
                      marginTop: "1rem",
                    }}
                />
            </Tab>      
        </Tabs>
    );
}

interface CSharpSnippetTabsProps {
    snippet: string; // e.g. "forms_example_1"
    liveSnippetHeight?: string;
    highlightLines?: string;
    defTab?: string;
}
