import { ImageResponse } from 'next/og';

export const dynamic = 'force-static';

export const alt = 'WebSharper documentation';
export const size = { width: 1200, height: 630 };
export const contentType = 'image/png';

export default function Image() {
  return new ImageResponse(
    (
      <div
        style={{
          width: '100%',
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'space-between',
          padding: '80px',
          background:
            'linear-gradient(135deg, #0b1020 0%, #1a1140 55%, #3b1152 100%)',
          color: '#ffffff',
          fontFamily: 'sans-serif',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: 20 }}>
          <div
            style={{
              width: 56,
              height: 56,
              borderRadius: 14,
              background: 'linear-gradient(135deg,#6366f1,#ec4899)',
            }}
          />
          <div style={{ display: 'flex', fontSize: 34, fontWeight: 700 }}>
            WebSharper
          </div>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column' }}>
          <div style={{ display: 'flex', fontSize: 72, fontWeight: 800 }}>
            Documentation
          </div>
          <div style={{ display: 'flex', fontSize: 30, marginTop: 16, opacity: 0.85 }}>
            Guides and reference for F# and C# web development
          </div>
        </div>

        <div style={{ display: 'flex', fontSize: 26, opacity: 0.75 }}>
          Sitelets &nbsp;·&nbsp; Remoting &nbsp;·&nbsp; Reactive UI &nbsp;·&nbsp;
          Templating &nbsp;·&nbsp; Forms
        </div>
      </div>
    ),
    { ...size },
  );
}
