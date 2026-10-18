import type { MetadataRoute } from 'next';
import { source } from '@/lib/source';

export const dynamic = 'force-static';

const baseUrl = 'https://docs.websharper.com';

export default function sitemap(): MetadataRoute.Sitemap {
  // The docs index is served from the site root, so exclude it from the page list.
  const pages = source.getPages().filter((page) => page.url !== '/');
  return [
    {
      url: `${baseUrl}/`,
      changeFrequency: 'monthly',
      priority: 1,
    },
    ...pages.map((page) => ({
      url: `${baseUrl}${page.url}`,
      changeFrequency: 'monthly' as const,
      priority: 0.7,
    })),
  ];
}
