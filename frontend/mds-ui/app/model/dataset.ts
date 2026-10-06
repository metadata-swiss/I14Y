import type { dcatApDataset } from '@piveau/sdk-vue'
import type { getKeywords } from '~/piveau/get-keywords'
import type { getOdsAccrualPeriodicity } from '~/piveau/get-ods-accrual-periodicity'
import type { getOdsCatalogInfo } from '~/piveau/get-ods-catalog-info'
import type { getOdsFormats } from '~/piveau/get-ods-formats'
import type { getOdsLicenses } from '~/piveau/get-ods-licenses'

export type Dataset = ReturnType<ReturnType<typeof dcatApDataset>['setup']> & {
  getOdsFormats: ReturnType<typeof getOdsFormats>
  getKeywords: ReturnType<typeof getKeywords>
  getOdsCatalogInfo: ReturnType<typeof getOdsCatalogInfo>
  getOdsAccrualPeriodicity: ReturnType<typeof getOdsAccrualPeriodicity>
  getOdsLicenses: ReturnType<typeof getOdsLicenses>
}
