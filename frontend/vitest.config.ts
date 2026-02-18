import { fileURLToPath } from 'node:url'
import { mergeConfig, defineConfig, configDefaults } from 'vitest/config'
import viteConfig from './vite.config'

export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      environment: 'jsdom',
      exclude: [...configDefaults.exclude, 'e2e/**'],
      root: fileURLToPath(new URL('./', import.meta.url)),
      coverage: {
        provider: 'v8',
        reporter: ['text', 'html', 'json-summary'],
        reportsDirectory: './coverage',
        all: true,
        include: ['src/**/*.ts'],
        exclude: [
          ...configDefaults.exclude,
          'e2e/**',
          'cypress/**',
          'src/**/*.d.ts',
          'src/main.ts',
          'src/theme.ts',
          'src/assets/**',
        ],
        thresholds: {
          lines: 40,
          functions: 40,
          statements: 40,
          branches: 30,
        },
      },
    },
  }),
)
