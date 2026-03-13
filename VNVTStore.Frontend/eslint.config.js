// For more info, see https://github.com/storybookjs/eslint-plugin-storybook#configuration-flat-config-format
// import storybook from "eslint-plugin-storybook";

import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import pluginReact from 'eslint-plugin-react'
import testingLibrary from 'eslint-plugin-testing-library'
import jestDom from 'eslint-plugin-jest-dom'



export default tseslint.config(
  { ignores: ['dist'] },
  js.configs.recommended,
  ...tseslint.configs.recommended,
  pluginReact.configs.flat.recommended,
  {
    files: ['src/**/__tests__/**/*.{ts,tsx}', 'src/**/*.test.{ts,tsx}'],
    ...testingLibrary.configs['flat/react'],
    ...jestDom.configs['flat/recommended'],
  },
  {
    languageOptions: {
      ecmaVersion: 2020,
      globals: {
        ...globals.browser,
        ...globals.node,
        // Vitest globals if used
        describe: 'readonly',
        it: 'readonly',
        expect: 'readonly',
        vi: 'readonly',
        beforeEach: 'readonly',
        afterEach: 'readonly',
        beforeAll: 'readonly',
        afterAll: 'readonly',
      },
    }
  },
  {
    files: ['**/*.{ts,tsx}'],
    languageOptions: {
      parserOptions: {
        ecmaFeatures: {
          jsx: true,
        },
      },
    },
    plugins: {
      'react-hooks': reactHooks,
      'react-refresh': reactRefresh,
    },
    rules: {
      ...reactHooks.configs.recommended.rules,
      'react-hooks/exhaustive-deps': 'error',
      'react-refresh/only-export-components': [
        'off',
        { allowConstantExport: true },
      ],
      'react/react-in-jsx-scope': 'off', // Not needed for React 17+
      'react/prop-types': 'off', // We use TS
      'no-unused-vars': 'off', // Handle via @typescript-eslint
      '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_', varsIgnorePattern: '^_' }],
      '@typescript-eslint/no-explicit-any': 'error',
      'eqeqeq': 'error',
    },
  },
  {
    settings: {
      react: {
        version: 'detect',
      },
    },
  },
)
