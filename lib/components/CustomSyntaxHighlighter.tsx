import React, { JSX } from "react";
import {highlight} from "fumadocs-core/highlight"
import type { ShikiTransformer } from "shiki"
import type { Element } from "hast"
import { createRanges } from "./shared";
import { CopyCodeComponent } from "./copyCodeComponent";

export function CustomSyntaxHighlighter({ code, language, highlightLines }: Props) {
    const ranges = createRanges(highlightLines);
    const highlightTransformer : ShikiTransformer =
      {
        line(node: Element, line: number) {
          if (ranges.includes(line)) {
            this.addClassToHast(node, 'highlight')
          }
        }
      }

    const options =
      {
        lang: language,
        transformers: [
          highlightTransformer
        ]
      }
    return (highlight(code, options));
}

export function CustomSyntaxHighlighterWithCopy(p: Props) : JSX.Element {
  const c = 
    <div className="copyBlockHolder" id={p.id}>
      <button className="copy">
        <span className="ready"></span>
        <span className="success"></span>
      </button>
      <CustomSyntaxHighlighter id={p.id} code={p.code} language={p.language} highlightLines={p.highlightLines}></CustomSyntaxHighlighter>
      <CopyCodeComponent id={p.id}></CopyCodeComponent>
    </div>;
  return c;
}

interface Props {
  id: string;
  code: string;
  language: string;
  highlightLines: string;
}
