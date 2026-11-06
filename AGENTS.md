# Agent guidelines

## Testing

- For non-code changes (menu items, copy/text, links, small styling tweaks, asset
  swaps), make the edit and stop. Do **not** run the production build, restart the
  dev server, or do route smoke tests.
- Run the full verification (production build, dev-server check, route checks) only
  when the change could plausibly break something — e.g. routing, layouts, build
  config, dependencies, or actual code.

## Repo notes

`docs.websharper.com` — Fumadocs + Next.js (App Router) on the `fumadocs` branch,
deployed as a static export to GitHub Pages.

### Commands

- **Dev**: `npm run dev` — *not* `next dev`. It runs `npm run prep`
  (esbuild `source.config.ts` → `.source/source.config.mjs`), installs and builds
  the sample solution (`snippets/`: `npm install`, `dotnet build
  snippets/snippets.sln -c Release`) and then starts `customServer.mjs`, which
  listens on `PORT` (default **3000**), is hardcoded to dev mode, and serves
  `/snippets/*` straight from `snippets/*/wwwroot/*`. Skip any of that and the
  snippet tabs 404.
- **Prod**: `npm run build` (`next build`) with `output: 'export'` in
  `next.config.mjs` → static files in `out/`. `basePath` comes from the
  `GHREPO` env var (currently commented out, so the site is served from the
  domain root; `lib/components/*SnippetTabs.tsx` read the same env var for their
  asset paths).
- **After a production build, restart the dev server.** Both write `.next`, so a
  build during a dev session leaves the dev server serving stale/broken output;
  when in doubt `rm -rf .next` and start `npm run dev` again.
- Docs are served from the **site root** (`lib/source.ts` sets `baseUrl: '/'`):
  `/core/sitelets`, `/ui/...`, not `/docs/core/...`.

### Layout

| Path | What it is |
| --- | --- |
| `content/**/*.mdx` | the documentation pages, grouped by area (`core/`, `ui/`, `forms/`, `csharp/`, `mvu/`, `metaprogramming/`, `cli/`, `contributing/`) with `meta.json` ordering |
| `content/index.mdx` | the designed landing page (`DocsHero` + install command + cards) |
| `app/(docs)/[[...slug]]/page.tsx`, `app/(docs)/layout.tsx` | the docs route + shared chrome |
| `app/layout.tsx`, `app/layout.config.tsx`, `app/global.css` | root layout, Fumadocs theme config, brand tokens (`--color-brand-primary: #36b9ff`, DM Sans) |
| `lib/components/menuBar.tsx`, `docsChrome.tsx` | the full-width top bar and the docs chrome wrapper (the stock `DocsLayout` hides its navbar on desktop and forces `--fd-nav-height: 0`) |
| `lib/components/*Snippet*.tsx`, `CustomSyntaxHighlighter.tsx`, `installCommand.tsx`, `asciinemaPlayer.tsx` | the snippet/terminal widgets |

### Snippets

Snippet tabs (`CSharpSnippetTabs`, `FSharpSnippetTabs`, `CSharpSnippetSimple`, …)
are server components: they read `snippets/<id>/Client.cs`, `Client.fs`,
`wwwroot/index.html` and friends with `fs.readFileSync` at render time and
extract marked regions (`extractBetweenMarkers` in `lib/components/shared.ts`).
So a snippet only renders if it exists under `snippets/` **and** the sample
solution was built (`npm run dev` does both).

### Conventions

- Copy: "full-stack" (never "fullstack"); write "vscode"; package/namespace
  references, FQNs and API names in headings are backticked.
- Brand colours come from `app/global.css` theme tokens, not hardcoded hexes.
