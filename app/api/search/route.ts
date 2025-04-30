import { source } from '@/lib/source';
import { createSearchAPI } from 'fumadocs-core/search/server';
 
// it should be cached forever
export const revalidate = false;
 
export const { staticGET: GET } = createSearchAPI('advanced', {
  indexes: source.getPages().map((page) => ({
    title: page.data.title,
    description: page.data.description,
    url: page.url,
    id: page.url,
    structuredData: page.data.structuredData,
  })),
});