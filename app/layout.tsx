import './global.css';
import { RootProvider } from 'fumadocs-ui/provider';
import { DM_Sans } from 'next/font/google';
import type { Metadata } from 'next';
import type { ReactNode } from 'react';

const searchPath = '/api/search'; //process.env.GHREPO !== undefined ? "/" + process.env.GHREPO + "/api/search" : "/api/search";

const dmSans = DM_Sans({
  subsets: ['latin'],
});

export const metadata: Metadata = {
  metadataBase: new URL('https://docs.websharper.com'),
  title: {
    default: 'WebSharper Documentation',
    template: '%s | WebSharper',
  },
  description:
    'Documentation for WebSharper: build web applications and services in F# and C# with a .NET-to-JavaScript compiler, type-safe client-server communication, sitelets, reactive templating and remoting.',
  keywords: [
    'WebSharper',
    'F#',
    'C#',
    '.NET',
    'JavaScript',
    'F# to JavaScript',
    'C# to JavaScript',
    'sitelets',
    'client-server',
    'remoting',
    'WebSharper.UI',
    'functional reactive programming',
  ],
  openGraph: {
    type: 'website',
    siteName: 'WebSharper',
    url: '/',
  },
  twitter: {
    card: 'summary_large_image',
  },
};

export default function Layout({ children }: { children: ReactNode }) {
  return (
    <html lang="en" className={dmSans.className} suppressHydrationWarning>
      <head>
        <link rel="icon" href="/favicon.png" sizes="any" />
      </head>
      <body className="flex flex-col min-h-screen">
        <RootProvider search={{ options: { type: 'static', api: searchPath } }}>
          {children}
        </RootProvider>
      </body>
    </html>
  );
}
