import { ref, watch } from 'vue'
import { type LocationQueryValue, useRoute, useRouter } from 'vue-router'

interface UseSorting {
  initialSort?: string
  sortCallback: (sortString: LocationQueryValue | LocationQueryValue[]) => void
}

export function useSorting({ initialSort, sortCallback }: UseSorting) {
  const router = useRouter()
  const route = useRoute()

  const selectedSort = ref<string>(typeof route.query.sort === 'string' ? route.query.sort.replace(/ /g, '+') : initialSort || '')

  watch(selectedSort, (sortString) => {
    // use the route to update the query parameters
    router.push({ query: { ...route.query, sort: sortString } })
  })

  watch(() => route.query.sort, value => value && sortCallback(value))

  return { selectedSort }
}
