import defaultMdxComponents from 'fumadocs-ui/mdx';
import type { MDXComponents } from 'mdx/types';
import { Tabs, Tab } from 'fumadocs-ui/components/tabs';
import { Steps, Step } from 'fumadocs-ui/components/steps';
import { Asciinema } from '@/lib/components/asciinemaPlayer';
import { DocsHero } from '@/lib/components/docsHero';

// use this function to get MDX components, you will need it for rendering MDX
export function getMDXComponents(components?: MDXComponents): MDXComponents {
  return {
    ...defaultMdxComponents,
    Asciinema,
    DocsHero,
    Tabs,
    Tab,
    Steps,
    Step,
    ...components,
  };
}
