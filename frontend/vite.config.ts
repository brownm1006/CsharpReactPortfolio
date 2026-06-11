/// <reference types="vitest/config" />

import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import type { UserConfig } from "vite";
import type { InlineConfig } from "vitest";

type ViteConfigWithTests = UserConfig & {
  test: InlineConfig;
};

const config: ViteConfigWithTests = {
  plugins: [react()],
  server: {
    host: "0.0.0.0",
    port: 5173,
  },
  test: {
    environment: "jsdom",
    globals: true,
    setupFiles: "./vitest.setup",
  },
};

export default defineConfig(config);
