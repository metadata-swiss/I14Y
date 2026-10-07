import { useVocabularySearch } from '~/piveau/vocabularies'

/**
 * Shared cache for showcase types so the query runs only once per SSR request
 * and once on the client. Subsequent consumers reuse the same data.
 */
export function useShowcaseTypes() {
  type ShowcaseType = { resource: string, pref_label?: Record<string, string> }

  const data = useState<ShowcaseType[]>('showcase-types:data', () => [])
  const loaded = useState<boolean>('showcase-types:loaded', () => false)
  const pending = useState<boolean>('showcase-types:pending', () => false)
  const error = useState<unknown | null>('showcase-types:error', () => null)

  // Single shared in-flight promise per request + client session
  const inflight = useState<Promise<void> | null>('showcase-types:inflight', () => null)

  const { query, getSearchResultsEnhanced } = useVocabularySearch().useSearch({
    queryParams: { vocabulary: 'showcase-types' },
  })

  async function runFetch() {
    pending.value = true
    error.value = null
    try {
      await query.suspense()
      data.value = getSearchResultsEnhanced.value ?? []
      loaded.value = true
    }
    catch (e) {
      error.value = e
      // Do not mark loaded so another attempt may retry later
    }
    finally {
      pending.value = false
    }
  }

  async function ensureLoaded() {
    if (loaded.value) return data

    if (!inflight.value) {
      inflight.value = runFetch().finally(() => {
        inflight.value = null
      })
    }

    // Await the shared promise so ALL callers wait for completion
    await inflight.value
    return data
  }

  return { data, loaded, pending, error, ensureLoaded }
}
