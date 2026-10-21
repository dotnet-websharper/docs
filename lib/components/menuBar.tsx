'use client';

import Link from 'fumadocs-core/link';
import { SearchToggle } from 'fumadocs-ui/components/layout/search-toggle';
import { ThemeToggle } from 'fumadocs-ui/components/layout/theme-toggle';
import { NavbarSidebarTrigger } from 'fumadocs-ui/layouts/docs';
import { useSidebar } from 'fumadocs-ui/contexts/sidebar';

const links = [
  { text: 'Home', url: 'https://websharper.com' },
  { text: 'Examples', url: 'https://websharper.com/examples' },
  { text: 'Docs', url: '/' },
  { text: 'Features', url: 'https://websharper.com/features' },
  { text: 'OCaml', url: 'https://websharper.com/ocaml' },
  { text: 'Warp', url: 'https://websharper.com/warp' },
  { text: 'Support', url: 'https://websharper.com/support' },
];

function CollapseToggle() {
  const { collapsed, setCollapsed } = useSidebar();

  return (
    <button
      type="button"
      aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
      title={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
      onClick={() => setCollapsed((prev) => !prev)}
      className="hidden size-9 shrink-0 items-center justify-center rounded-md text-fd-muted-foreground transition-colors hover:bg-fd-accent hover:text-fd-accent-foreground md:inline-flex"
    >
      {collapsed ? (
        <svg
          viewBox="0 0 24 24"
          className="size-5"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
          aria-hidden="true"
        >
          <rect width="18" height="18" x="3" y="3" rx="2" />
          <path d="M9 3v18" />
          <path d="m14 9 3 3-3 3" />
        </svg>
      ) : (
        <svg
          viewBox="0 0 24 24"
          className="size-5"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
          aria-hidden="true"
        >
          <rect width="18" height="18" x="3" y="3" rx="2" />
          <path d="M9 3v18" />
          <path d="m16 15-3-3 3-3" />
        </svg>
      )}
    </button>
  );
}

/**
 * Full-width top menu bar for the docs, mirroring websharper.com.
 *
 * Sticky (in flow) so the sidebar and page content sit below it.
 * Set `collapsible` on pages that have the docs sidebar, so the sidebar
 * collapse toggle is shown.
 */
export function MenuBar({ collapsible = false }: { collapsible?: boolean }) {
  return (
    <header className="sticky top-0 z-40 h-14 w-full border-b border-fd-border bg-fd-background/80 backdrop-blur-lg">
      <nav className="flex h-full w-full items-center px-4 lg:px-6">
        <Link href="https://websharper.com" className="flex shrink-0 items-center gap-2 font-semibold">
          <img src="/logo.png" alt="WebSharper" className="size-7" />
          <span>WebSharper</span>
        </Link>
        {collapsible && <CollapseToggle />}
        <ul className="hidden items-center gap-5 ps-8 text-sm md:flex">
          {links.map((l) => (
            <li key={l.text}>
              <Link
                href={l.url}
                className="font-bold text-gray-700 transition-colors duration-300 hover:text-gray-600 dark:text-gray-300 dark:hover:text-white"
              >
                {l.text}
              </Link>
            </li>
          ))}
        </ul>
        <div className="ms-auto flex items-center gap-1">
          <SearchToggle hideIfDisabled />
          <ThemeToggle />
          <a
            href="https://github.com/dotnet-websharper/core"
            aria-label="GitHub"
            className="inline-flex size-9 items-center justify-center rounded-md text-fd-muted-foreground transition-colors hover:text-fd-foreground"
          >
            <svg viewBox="0 0 24 24" className="size-5" fill="currentColor" aria-hidden="true">
              <path d="M12 .5C5.7.5.5 5.7.5 12c0 5.1 3.3 9.4 7.9 10.9.6.1.8-.3.8-.6v-2c-3.2.7-3.9-1.4-3.9-1.4-.5-1.3-1.3-1.7-1.3-1.7-1.1-.7.1-.7.1-.7 1.2.1 1.8 1.2 1.8 1.2 1 1.8 2.7 1.3 3.4 1 .1-.8.4-1.3.7-1.6-2.6-.3-5.3-1.3-5.3-5.8 0-1.3.5-2.3 1.2-3.1-.1-.3-.5-1.5.1-3.1 0 0 1-.3 3.3 1.2a11.5 11.5 0 0 1 6 0C17.3 4.9 18.3 5.2 18.3 5.2c.6 1.6.2 2.8.1 3.1.8.8 1.2 1.8 1.2 3.1 0 4.5-2.7 5.5-5.3 5.8.4.4.8 1.1.8 2.2v3.3c0 .3.2.7.8.6 4.6-1.5 7.9-5.8 7.9-10.9C23.5 5.7 18.3.5 12 .5z" />
            </svg>
          </a>
          <a
            href="https://www.nuget.org/packages?q=websharper"
            aria-label="NuGet"
            className="inline-flex size-9 items-center justify-center rounded-md text-fd-muted-foreground transition-colors hover:text-fd-foreground"
          >
            <svg viewBox="0 0 24 24" className="size-5" fill="currentColor" aria-hidden="true">
              <path d="M1.998.342a1.997 1.997 0 1 0 0 3.995 1.997 1.997 0 0 0 0-3.995zm9.18 4.34a6.156 6.156 0 0 0-6.153 6.155v6.667c0 3.4 2.756 6.154 6.154 6.154h6.667c3.4 0 6.154-2.755 6.154-6.154v-6.667a6.154 6.154 0 0 0-6.154-6.155zm-1.477 2.8a2.496 2.496 0 1 1 0 4.993 2.496 2.496 0 0 1 0-4.993zm7.968 6.16a3.996 3.996 0 1 1-.002 7.992 3.996 3.996 0 0 1 .002-7.992z" />
            </svg>
          </a>
          <NavbarSidebarTrigger className="-me-1.5 md:hidden" />
        </div>
      </nav>
    </header>
  );
}
