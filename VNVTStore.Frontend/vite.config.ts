/* eslint-disable @typescript-eslint/triple-slash-reference */
/// <reference types="vitest/config" />
/// <reference types="vitest" />
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import path from 'path';

import { VitePWA } from 'vite-plugin-pwa';

// https://vite.dev/config/
import { fileURLToPath } from 'node:url';
import { storybookTest } from '@storybook/addon-vitest/vitest-plugin';
import { playwright } from '@vitest/browser-playwright';
const dirname = typeof __dirname !== 'undefined' ? __dirname : path.dirname(fileURLToPath(import.meta.url));

// More info at: https://storybook.js.org/docs/next/writing-tests/integrations/vitest-addon
export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: ['favicon.ico', 'robots.txt', 'apple-touch-icon.png'],
      manifest: {
        name: 'VNVT Store - Đồ Gia Dụng Cao Cấp',
        short_name: 'VNVT Store',
        description: 'Hệ thống cửa hàng đồ gia dụng cao cấp, chính hãng. Miễn phí vận chuyển toàn quốc.',
        theme_color: '#4F46E5',
        background_color: '#ffffff',
        display: 'standalone',
        orientation: 'portrait-primary',
        lang: 'vi',
        categories: ['shopping', 'business'],
        icons: [
          {
            src: 'vnvt-logo.svg',
            sizes: 'any',
            type: 'image/svg+xml',
            purpose: 'any'
          },
          {
            src: 'vnvt-logo.svg',
            sizes: 'any',
            type: 'image/svg+xml',
            purpose: 'maskable'
          }
        ]
      }
    })
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@components': path.resolve(__dirname, './src/components'),
      '@pages': path.resolve(__dirname, './src/pages'),
      '@hooks': path.resolve(__dirname, './src/hooks'),
      '@store': path.resolve(__dirname, './src/store'),
      '@services': path.resolve(__dirname, './src/services'),
      '@utils': path.resolve(__dirname, './src/utils'),
      '@types': path.resolve(__dirname, './src/types'),
      '@assets': path.resolve(__dirname, './src/assets'),
      '@config': path.resolve(__dirname, './src/config'),
      '@layouts': path.resolve(__dirname, './src/layouts'),
      '@features': path.resolve(__dirname, './src/features'),
      '@locales': path.resolve(__dirname, './src/locales')
    }
  },
  build: {
    rollupOptions: {
      output: {
      }
    },
    chunkSizeWarningLimit: 1000
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
    css: true,
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      thresholds: {
        statements: 80,
        branches: 80,
        functions: 80,
        lines: 80
      },
      include: ['src/components/**/*.{ts,tsx}', 'src/utils/**/*.{ts,tsx}'],
      exclude: ['src/**/*.stories.{ts,tsx}', 'src/**/*.test.{ts,tsx}']
    },
    projects: [
      {
        extends: true,
        test: {
          name: 'unit',
          environment: 'jsdom',
          include: ['src/**/*.test.{ts,tsx}', 'src/**/*.spec.{ts,tsx}'],
          exclude: ['src/**/*.stories.{ts,tsx}']
        }
      },
      {
        extends: true,
        plugins: [
          // The plugin will run tests for the stories defined in your Storybook config
          // See options at: https://storybook.js.org/docs/next/writing-tests/integrations/vitest-addon#storybooktest
          storybookTest({
            configDir: path.join(dirname, '.storybook')
          })],
        test: {
          name: 'storybook',
          browser: {
            enabled: true,
            headless: true,
            provider: playwright({}),
            instances: [{
              browser: 'chromium'
            }]
          },
          setupFiles: ['.storybook/vitest.setup.ts']
        }
      }]
  },
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5176',
        changeOrigin: true,
        secure: false,
      },
      '/hub': {
        target: 'http://localhost:5176',
        ws: true,
        changeOrigin: true,
      }
    }
  }
});