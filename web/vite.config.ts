import { defineConfig } from 'vitest/config';
import { viteSingleFile } from 'vite-plugin-singlefile';

// Minimal declaration so we can read PORT without pulling in @types/node.
declare const process: { env: Record<string, string | undefined> };

// Build emits a single self-contained dist/index.html (JS + CSS inlined) so each
// phase can be published to docs/builds/phaseN.html as an openable snapshot.
// When PORT is set (e.g. by the preview harness), bind to it exactly so the proxy
// can reach the dev server; otherwise default to 5173.
const envPort = Number(process.env.PORT) || undefined;
export default defineConfig({
  server: { port: envPort ?? 5173, strictPort: !!envPort },
  plugins: [viteSingleFile()],
  build: {
    target: 'esnext', // top-level await in main.ts (boot awaits the Pixi renderer)
    outDir: 'dist',
    assetsInlineLimit: 1e9,
    chunkSizeWarningLimit: 1e9,
  },
  test: {
    globals: true,
    environment: 'node',
  },
});
