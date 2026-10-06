import type { ComputedRef, MaybeRefOrGetter } from 'vue'
import { reactive, watch } from 'vue'
import type { LocationQuery, LocationQueryValue } from 'vue-router'
import { useRoute, useRouter } from '#vue-router'
import type { SearchResultFacetGroupLocalized } from '@piveau/sdk-vue'

type FacetRefs<F extends string> = Record<F, Ref<string[]>>

interface SyncFacetsFromRouteArgs {
  facetRefs: Record<string, Ref<LocationQueryValue[]>>
}

export function syncFacetsFromRoute({ facetRefs }: SyncFacetsFromRouteArgs) {
  const route = useRoute()
  const facets: string[] = Object.keys(facetRefs)

  facets.forEach((facet) => {
    const newVal = route.query[facet] || []
    facetRefs[facet]!.value = Array.isArray(newVal) ? newVal : [newVal]
  })
}

export function useFacets(facets: string[]) {
  const route = useRoute()
  const router = useRouter()

  // 1. Main reactive object for your logic/UI
  const selectedFacets = reactive(
    Object.fromEntries(facets.map(facet => [facet, [] as string[]])),
  )

  // 2. facetRefs for useSearch API (syncs with selectedFacets)
  const facetRefs = Object.fromEntries(
    facets.map(facet => [facet, computed({
      get: () => selectedFacets[facet],
      set: (val: string[]) => { selectedFacets[facet] = val },
    })]),
  )

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
  facets: string[]
  getAvailableFacetsLocalized: (locale?: MaybeRefOrGetter<string>) => ComputedRef<SearchResultFacetGroupLocalized[]>
}

export function useActiveFacets({ facets, getAvailableFacetsLocalized }: UseActiveFacetsArgs) {
  const { locale } = useI18n()

  const availableFacets = getAvailableFacetsLocalized(locale)

  return computed<SearchResultFacetGroupLocalized[]>(() => {
    return availableFacets.value.filter(f => facets.includes(f.id)).sort((a, b) => a.title.localeCompare(b.title))
  })
}
