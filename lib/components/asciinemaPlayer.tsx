'use client';

import { useEffect, useRef } from 'react';
import type { Player } from 'asciinema-player';
import 'asciinema-player/dist/bundle/asciinema-player.css';

export function Asciinema({
  src,
  cols = 100,
  rows = 20,
  speed = 2.5,
}: {
  src: string;
  cols?: number;
  rows?: number;
  speed?: number;
}) {
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    let player: Player | undefined;
    let cancelled = false;

    void (async () => {
      const { create } = await import('asciinema-player');
      if (cancelled || !ref.current) return;
      player = create(src, ref.current, {
        cols,
        rows,
        speed,
        theme: 'dracula',
        autoPlay: true,
        loop: true,
        idleTimeLimit: 6,
      });
    })();

    return () => {
      cancelled = true;
      player?.dispose();
    };
  }, [src, cols, rows, speed]);

  return <div ref={ref} className="not-prose my-6" />;
}
