import type { ReactNode } from 'react';
import { DocsChrome } from '@/lib/components/docsChrome';

export default function Layout({ children }: { children: ReactNode }) {
  return <DocsChrome>{children}</DocsChrome>;
}
