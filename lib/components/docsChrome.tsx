import { DocsLayout } from 'fumadocs-ui/layouts/docs';
import type { ReactNode } from 'react';
import { baseOptions } from '@/app/layout.config';
import { source } from '@/lib/source';
import { MenuBar } from '@/lib/components/menuBar';

function HomeIcon() {
  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M15 21v-8a1 1 0 0 0-1-1h-4a1 1 0 0 0-1 1v8" />
      <path d="M3 10a2 2 0 0 1 .709-1.528l7-5.999a2 2 0 0 1 2.582 0l7 5.999A2 2 0 0 1 21 10v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" />
    </svg>
  );
}

/**
 * Shared chrome for every page that shows the docs sidebar: the full-width top
 * menu bar (with the sidebar collapse toggle) plus the page tree on the left.
 */
export function DocsChrome({ children }: { children: ReactNode }) {
  return (
    <DocsLayout
      tree={source.pageTree}
      {...baseOptions}
      // The main nav lives in the top menu bar; the sidebar keeps a Home link + page tree.
      links={[{ text: 'Home', url: '/', icon: <HomeIcon /> }]}
      // The menu bar is sticky and in flow, so the content sits below it; this keeps
      // the sidebar/TOC sticky offsets aligned with the 3.5rem bar.
      containerProps={{ className: '[--fd-nav-height:3.5rem]' }}
      // Fumadocs hides its own docs navbar on desktop; use our full-width bar.
      nav={{ component: <MenuBar collapsible /> }}
    >
      {children}
    </DocsLayout>
  );
}
