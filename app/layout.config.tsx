import type { BaseLayoutProps } from 'fumadocs-ui/layouts/shared';

/**
 * Shared layout configurations
 *
 * The docs are served from the site root (see lib/source.ts), so every page
 * lives under app/(docs)/ and shares app/(docs)/layout.tsx.
 */
export const baseOptions: BaseLayoutProps = {
  nav: {
    title: (
      <>
        <img src="/logo.png" alt="WebSharper" className="size-6" />
        <span className="font-semibold">WebSharper</span>
      </>
    ),
    url: '/',
  },
  githubUrl: 'https://github.com/dotnet-websharper/core',
  // Mirrors the main site's menu bar (websharper.com).
  links: [
    {
      text: 'Home',
      url: 'https://websharper.com',
      external: true,
    },
    {
      text: 'Docs',
      url: '/',
      active: 'nested-url',
    },
    {
      text: 'Features',
      url: 'https://websharper.com/features',
      external: true,
    },
    {
      text: 'OCaml',
      url: 'https://websharper.com/ocaml',
      external: true,
    },
    {
      text: 'Warp',
      url: 'https://websharper.com/warp',
      external: true,
    },
    {
      text: 'Support',
      url: 'https://websharper.com/support',
      external: true,
    },
    {
      text: 'NuGet',
      url: 'https://www.nuget.org/packages/WebSharper',
      external: true,
    },
  ],
};
