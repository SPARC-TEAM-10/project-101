/// <reference types="vitest/config" />
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
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
