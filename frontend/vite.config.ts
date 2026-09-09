/// <reference types="vitest/config" />
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  // Pinned to 5173 (not Vite's default-if-free behavior) because the deployed backend's CORS
  // allow-list (backend/src/Chh.Api/appsettings.Production.json) only lists
  // "http://localhost:5173" — if this port is busy and Vite silently falls back to 5174+, every
  // API call gets CORS-blocked with a confusing "no Access-Control-Allow-Origin" error instead of
  // a clear "port in use" one. strictPort surfaces that failure immediately instead.
  server: {
    port: 5173,
    strictPort: true,
  },
  test: {
    environment: "jsdom",
    setupFiles: ["./tests/setup.ts"],
    globals: true,
    coverage: {
      provider: "v8",
      reporter: ["text", "html"],
      exclude: [
        "**/*.config.{js,ts,cjs}",
        ".eslintrc.cjs",
        "dist/**",
        "src/main.tsx",
        "src/router.tsx",
        "src/vite-env.d.ts",
        "**/*.test.{ts,tsx}",
        "tests/msw/handlers.ts",
      ],
    },
  },
});
