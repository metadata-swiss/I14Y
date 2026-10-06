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
