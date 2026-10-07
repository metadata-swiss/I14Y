import type { BreadcrumbItem } from '~/components/OdsBreadcrumbs.vue'

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

interface DatasetBreadcrumbTitles {
  home: string
  datasets: string
  searchResults: string
  dataset: string
}

/**
 * Translates the stored breadcrumbs of a dataset into the current language.
 * They are home, datasets, search results (only when the dataset was opened from a search) and the dataset.
 */
export function translateDatasetBreadcrumbs(breadcrumbs: BreadcrumbItem[], titles: DatasetBreadcrumbTitles) {
  const translatedTitles = breadcrumbs.length === 4
    ? [titles.home, titles.datasets, titles.searchResults, titles.dataset]
    : [titles.home, titles.datasets, titles.dataset]

  breadcrumbs.forEach((breadcrumb, index) => {
    breadcrumb.title = translatedTitles[index] ?? breadcrumb.title
  })
}
