import type { Dataset } from '@piveau/sdk-core'
import type { LocaleInstance } from '@piveau/sdk-vue'
import { toValue } from 'vue'

export interface Catalog {
  id: string
  modified: string
  issued: string
  title: string
  publisher: CatalogPublisher
  description: string
  record: CatalogRecord
}

interface CatalogPublisher {
  id: string
  name: string
}

interface CatalogRecord {
  modified: string
  issued: string
}

/**
 * Extend enriched dataset with catalog information. This includes title and description in the current locale and the catalog record issued and modified dates.
 *
 * @param dataset dataset
 * @param localeInstance
 * @returns Catalog information (id, modified, issued, title, description, record: {modified, issued})
 */
export function getOdsCatalogInfo(dataset: Dataset, localeInstance: LocaleInstance): Catalog {
  const catalogInfo = dataset.catalog
  const recordInfo = dataset.catalog_record

  const locale = toValue(localeInstance.currentLocale)
  const availableLocalesForTitle = Object.keys(catalogInfo?.title || {})
  const availableLocalesForDescription = Object.keys(catalogInfo?.description || {})
  const availableLocalesForPublisher = Object.keys(catalogInfo?.publisher?.name || {})

  let title = ''
  let description = ''
  let publisher = ''

  if (availableLocalesForTitle.includes(locale)) {
    title = catalogInfo?.title?.[locale] ?? ''
  }
  else if (availableLocalesForTitle.length > 0 && availableLocalesForTitle[0] !== undefined) {
    title = catalogInfo?.title?.[availableLocalesForTitle[0] as string] ?? ''
  }

  if (availableLocalesForDescription.includes(locale)) {
    description = catalogInfo?.description?.[locale] ?? ''
  }
  else if (availableLocalesForDescription.length > 0 && availableLocalesForDescription[0] !== undefined) {
    description = catalogInfo?.description?.[availableLocalesForDescription[0] as string] ?? ''
  }

  if (availableLocalesForPublisher.includes(locale)) {
    const publisherNameByLocale = catalogInfo?.publisher?.name as Record<string, string> | undefined
    publisher = publisherNameByLocale?.[String(locale)] ?? ''
  }
  else if (availableLocalesForPublisher.length > 0 && availableLocalesForPublisher[0] !== undefined) {
    const publisherNameByLocale = catalogInfo?.publisher?.name as Record<string, string> | undefined
    publisher = publisherNameByLocale?.[availableLocalesForPublisher[0]] ?? ''
  }

  return {
    id: catalogInfo.id,
    modified: catalogInfo.modified ?? '',
    issued: catalogInfo.issued ?? '',
    title,
    publisher: {
      id: 'currently not provided by api',
      name: publisher,
    },
    description,
    record: {
      modified: recordInfo?.modified ?? '',
      issued: recordInfo?.issued ?? '',
    },
  }
}
