import type { MultiLanguageModel } from '~~/api-client/generated/iop-core'
import { APP_LANGUAGES, type AppLanguage } from '~/constants/langages'

type TranslatedString = Record<string, string>

export function getCurrentTranslation(translations: TranslatedString | undefined = {}, locale: AppLanguage): string {
  return (
    translations[locale]
    || APP_LANGUAGES.map(lang => translations[lang]).find(Boolean)
    || Object.values(translations)[0]
    || ''
  )
}

/**
 * Get the text of a multilingual iop-core value in the given language.
 * If not available, the other APP_LANGUAGES are tried in order, then any available language.
 *
 * @param value multilingual value
 * @param lang language
 * @returns the text, or an empty string
 */
export function localize(value: MultiLanguageModel | undefined, lang: AppLanguage | string): string {
  return getCurrentTranslation({ ...value } as TranslatedString, lang as AppLanguage)
}
