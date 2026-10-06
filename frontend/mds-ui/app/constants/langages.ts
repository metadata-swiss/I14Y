/**
 * Available application languages and language precedence.
 */
export const APP_LANGUAGES = ['de', 'fr', 'it', 'en'] as const

export type AppLanguage = typeof APP_LANGUAGES[number]
