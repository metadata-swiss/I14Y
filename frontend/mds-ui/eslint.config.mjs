// @ts-check
import reactPlugin from 'eslint-plugin-react'
import withNuxt from './.nuxt/eslint.config.mjs'

export default withNuxt(
  {
    settings: {
      files: [
        '**/*.ts',
        '**/*.js',
        '**/*.tsx',
        '**/*.jsx',
        '**/*.mjs',
        '**/*.vue',
      ],
      react: {
        version: 'detect',
      },
    },
    rules: {
      '@stylistic/no-tabs': 'off',
      'quotes': ['error', 'single'],
    },
  },
  reactPlugin.configs.flat.recommended,
  {
    files: ['**/*.test.ts'],
    rules: {
      '@typescript-eslint/no-unused-expressions': 'off',
    },
  },
)
