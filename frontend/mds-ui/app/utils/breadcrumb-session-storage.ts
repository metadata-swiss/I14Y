// The search a dataset was opened from, for the "search results" breadcrumb (see useDatasetBreadcrumbs).
// Only the last dataset is kept.
const DATASET_SEARCH_STORAGE_KEY = 'ch.opendata-swiss.datasets.search'

export function getDatasetSearchFromSessionStorage(datasetId: string): string | null {
  const stored = sessionStorage.getItem(DATASET_SEARCH_STORAGE_KEY)
  if (stored) {
    try {
      const searches = JSON.parse(stored) as Record<string, string>
      return searches[datasetId] ?? null
    }
    catch {
      sessionStorage.removeItem(DATASET_SEARCH_STORAGE_KEY)
    }
  }
  return null
}

export function storeDatasetSearchInSessionStorage(datasetId: string, search: string) {
  sessionStorage.setItem(DATASET_SEARCH_STORAGE_KEY, JSON.stringify({ [datasetId]: search }))
}

export function clearDatasetSearchFromSessionStorage() {
  sessionStorage.removeItem(DATASET_SEARCH_STORAGE_KEY)
}
