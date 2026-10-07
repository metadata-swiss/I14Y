import type { ComputedRef, MaybeRefOrGetter, WritableComputedRef } from 'vue'
import { reactive, watch } from 'vue'
import type { LocationQuery } from 'vue-router'
import { useRoute, useRouter } from '#vue-router'
/**
 * A filter of a search page: the values found in the results, and how many results have each value.
 */
export interface FacetGroup {
  id: string
  title: string
  items: FacetItem[]
}

export interface FacetItem {
  id: string
  title: string | undefined
  count: number
}

type FacetRefs<F extends string> = Record<F, Ref<string[]>>

interface SyncFacetsFromRouteArgs {
  facetRefs: Record<string, Ref<string[]>>
}

export function syncFacetsFromRoute({ facetRefs }: SyncFacetsFromRouteArgs) {
  const route = useRoute()

  Object.entries(facetRefs).forEach(([facet, facetRef]) => {
    const newVal = route.query[facet] || []
    // A query parameter without a value (?format) is null.
    facetRef.value = (Array.isArray(newVal) ? newVal : [newVal]).filter(value => value !== null)
  })
}

export function useFacets<F extends string>(facets: readonly F[]) {
  const route = useRoute()
  const router = useRouter()

  // 1. Main reactive object for your logic/UI
  const selectedFacets = reactive(
    Object.fromEntries(facets.map(facet => [facet, [] as string[]])),
  )

  // 2. facetRefs for useSearch API (syncs with selectedFacets)
  const facetRefs = Object.fromEntries(
    facets.map(facet => [facet, computed({
      get: () => selectedFacets[facet] ?? [],
      set: (val: string[]) => { selectedFacets[facet] = val },
    })]),
  ) as Record<F, WritableComputedRef<string[]>>

  // 3. Use selectedFacets everywhere in your code and UI
  function resetAllFacets() {
    for (const key in selectedFacets) {
      selectedFacets[key] = []
    }
    // Reset the 'facets' query parameter
    const query = { ...route.query }
    if (query.page && query.page !== '1') {
      query.page = '1' // Reset page to 1 if facets are restored from route
    }
    delete query['facets']
    router.push({ query })
  }

  return { resetAllFacets, facetRefs }
}

interface UseFacetSyncArgs<F extends string> {
  facetRefs: FacetRefs<F>
}

export function useFacetSync<F extends string>({
  facetRefs,
}: UseFacetSyncArgs<F>) {
  const route = useRoute()
  const router = useRouter()

  const facets = Object.keys(facetRefs) as F[]

  const hasFacetChanged = (query: LocationQuery, facet: string, newVal: string[]) => {
    const current = Array.isArray(query[facet]) ? query[facet] : query[facet] ? [query[facet]] : []
    const currentValues = new Set(current)
    return newVal.length !== currentValues.size || newVal.some(value => !currentValues.has(value))
  }

  facets.forEach((facet) => {
    watch(facetRefs[facet], (newVal) => {
      const query = { ...route.query }
      if (!hasFacetChanged(route.query, facet, newVal)) {
        return
      }

      query[facet] = newVal
      if (query.page && query.page !== '1') {
        query.page = '1'
      }

      router.push({ query })
    })
  })
}

interface UseActiveFacetsArgs {
  facets: readonly string[]
  getAvailableFacetsLocalized: (locale?: MaybeRefOrGetter<string>) => ComputedRef<FacetGroup[]>
}

export function useActiveFacets({ facets, getAvailableFacetsLocalized }: UseActiveFacetsArgs) {
  const { locale } = useI18n()

  const availableFacets = getAvailableFacetsLocalized(locale)

  return computed<FacetGroup[]>(() => {
    return availableFacets.value.filter(f => facets.includes(f.id)).sort((a, b) => a.title.localeCompare(b.title))
  })
}
