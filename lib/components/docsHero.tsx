import Link from 'fumadocs-core/link';
import { LargeSearchToggle } from 'fumadocs-ui/components/layout/search-toggle';
import { InstallCommand } from './installCommand';

function ArrowIcon() {
  return (
    <svg
      viewBox="0 0 24 24"
      className="size-4"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M5 12h14" />
      <path d="m12 5 7 7-7 7" />
    </svg>
  );
}

function GitHubIcon() {
  return (
    <svg viewBox="0 0 16 16" className="size-4" fill="currentColor" aria-hidden="true">
      <path d="M8 0C3.58 0 0 3.58 0 8c0 3.54 2.29 6.53 5.47 7.59.4.07.55-.17.55-.38 0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13-.28-.15-.68-.52-.01-.53.63-.01 1.08.58 1.23.82.72 1.21 1.87.87 2.33.66.07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95 0-.87.31-1.59.82-2.15-.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82.64-.18 1.32-.27 2-.27.68 0 1.36.09 2 .27 1.53-1.04 2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82 2.15 0 3.07-1.87 3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01 2.2 0 .21.15.46.55.38A8.012 8.012 0 0 0 16 8c0-4.42-3.58-8-8-8z" />
    </svg>
  );
}

function ExternalIcon() {
  return (
    <svg
      viewBox="0 0 24 24"
      className="size-3.5"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M15 3h6v6" />
      <path d="M10 14 21 3" />
      <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6" />
    </svg>
  );
}

/**
 * Documentation root hero. Used from content/index.mdx.
 */
export function DocsHero({
  title,
  subtitle,
  command,
}: {
  title: string;
  subtitle: string;
  command?: string;
}) {
  return (
    <div className="not-prose relative mb-16 overflow-hidden rounded-2xl border border-fd-border bg-fd-card/40 px-6 py-12 sm:px-10 sm:py-16">
      <div
        aria-hidden="true"
        className="pointer-events-none absolute -top-28 -right-10 size-80 rounded-full bg-brand-primary/20 blur-3xl"
      />
      <div className="relative">
        <div className="flex items-center gap-3">
          <img src="/logo.png" alt="WebSharper" className="size-10" />
          <span className="text-xs font-semibold uppercase tracking-[0.2em] text-fd-muted-foreground">
            WebSharper Documentation
          </span>
        </div>

        <h1 className="mt-6 max-w-3xl text-3xl font-bold tracking-tight text-fd-foreground sm:text-5xl">
          {title}
        </h1>
        <p className="mt-5 max-w-2xl text-base text-fd-muted-foreground sm:text-lg">
          {subtitle}
        </p>

        <div className="mt-8 flex flex-wrap items-center gap-3">
          <Link
            href="/core/getting-started"
            className="inline-flex items-center gap-1.5 rounded-lg bg-fd-primary px-5 py-2.5 text-sm font-semibold text-fd-primary-foreground transition-opacity hover:opacity-90"
          >
            Get started
            <ArrowIcon />
          </Link>
          <Link
            href="https://github.com/dotnet-websharper/core"
            className="inline-flex items-center gap-1.5 rounded-lg border border-fd-border bg-fd-background px-5 py-2.5 text-sm font-medium transition-colors hover:bg-fd-accent"
          >
            <GitHubIcon />
            GitHub
          </Link>
          <Link
            href="https://websharper.com"
            className="inline-flex items-center gap-1.5 rounded-lg border border-fd-border bg-fd-background px-5 py-2.5 text-sm font-medium transition-colors hover:bg-fd-accent"
          >
            websharper.com
            <ExternalIcon />
          </Link>
        </div>

        {command ? (
          <div className="mt-8 max-w-xl">
            <InstallCommand command={command} />
          </div>
        ) : null}

        <div className="mt-6 max-w-xl">
          <LargeSearchToggle className="h-11 w-full rounded-xl px-3 text-base" />
        </div>
      </div>
    </div>
  );
}
