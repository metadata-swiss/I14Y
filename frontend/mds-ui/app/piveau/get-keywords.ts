import type { Dataset } from '@piveau/sdk-core'
import type { LocaleInstance } from '@piveau/sdk-vue'

/**
 * Add keywords from an enriched dataset.
 *
 * @param dataset Enriched dataset
 * @param localeInstance
 * @returns Array of keywords in the current locale
 */
export function getKeywords(dataset: Dataset, localeInstance: LocaleInstance) {
  const keywords = (dataset?.keywords || [])
  const locale = localeInstance.currentLocale

  return keywords.filter((keyword) => {
    return keyword.language === locale
  })
}
