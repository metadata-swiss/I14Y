import type { BreadcrumbItem } from '../../../app/components/OdsBreadcrumbs.vue'

const DATASET_BREADCRUMBS_STORAGE_KEY = 'ch.opendata-swiss.datasets.breadcrumbs'

export function getDatasetBreadcrumbFromSessionStorage(datasetId: string) {
  const stored = sessionStorage.getItem(DATASET_BREADCRUMBS_STORAGE_KEY)
  if (stored) {
    try {
      const sessionBreadcrumbs = JSON.parse(stored) as Record<string, BreadcrumbItem[]>
      if (sessionBreadcrumbs[datasetId]) {
        return sessionBreadcrumbs[datasetId]
      }
    }
    catch {
      sessionStorage.removeItem(DATASET_BREADCRUMBS_STORAGE_KEY)
    }
  }
  return null
}

export function storeDatasetBreadcrumbInSessionStorage(datasetId: string, breadcrumbs: BreadcrumbItem[]) {
  const breadcrumbsObject = { [datasetId]: breadcrumbs } as Record<string, BreadcrumbItem[]>
  sessionStorage.setItem(DATASET_BREADCRUMBS_STORAGE_KEY, JSON.stringify(breadcrumbsObject))
}

export function clearDatasetBreadcrumbFromSessionStorage() {
  sessionStorage.removeItem(DATASET_BREADCRUMBS_STORAGE_KEY)
}
