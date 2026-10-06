import { schemaDataset } from '@piveau/sdk-core'
import { dcatApDataset, defineHubSearch } from '@piveau/sdk-vue'
import { getKeywords } from './get-keywords'
import { getOdsFormats } from './get-ods-formats'
import { getOdsCatalogInfo } from './get-ods-catalog-info'
import { getOdsAccrualPeriodicity } from './get-ods-accrual-periodicity'
import { getOdsLicenses } from './get-ods-licenses'

export const facets = ['catalog', 'categories', 'organization', 'format', 'license', 'keywords']

/**
 * Returns a piveau hub-search query definition for DCAT-AP datasets
 */
export function useDatasetsSearch() {
  const baseUrl = useRuntimeConfig().public.piveauHubSearchUrl as string

  return defineHubSearch({
    baseUrl,
    index: 'dataset',
    indexDetails: 'datasets',
    facets,
    schema: schemaDataset,
  }, (dataset, localeInstance) => {
    const { setup: base } = dcatApDataset()

    return {
      ...base(dataset, localeInstance),
      getKeywords: getKeywords(dataset, localeInstance),
      getOdsCatalogInfo: getOdsCatalogInfo(dataset, localeInstance),
      getOdsFormats: getOdsFormats(dataset),
      getOdsAccrualPeriodicity: getOdsAccrualPeriodicity(dataset),
      getResource: dataset.resource,
      getOdsLicenses: getOdsLicenses(dataset),
      getPublisher: dataset.catalog.publisher,
    }
  })
}
