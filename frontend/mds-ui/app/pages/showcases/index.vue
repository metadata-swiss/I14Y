<script setup lang="ts">
import { computed, onMounted, ref, useTemplateRef, watch, type ComponentPublicInstance } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from '#imports'
import { useSeoMeta } from 'nuxt/app'

import OdsPage from '~/components/OdsPage.vue'
import { homePageBreadcrumb } from '~/composables/breadcrumbs.js'
import OdsBreadcrumbs from '~/components/OdsBreadcrumbs.vue'
import { showcaseFacets as facets, usePublishedShowcases, useShowcaseSearch } from '~/composables/useShowcases'
import OdsSearchPanel from '~/components/OdsSearchPanel.vue'
import OdsSearchResults from '~/components/OdsSearchResults.vue'
import {
  syncFacetsFromRoute,
  useActiveFacets,
  useFacets,
  useFacetSync,
} from '~/composables/useFacets'
import OdsSortSelect from '~/components/dataset/OdsSortSelect.vue'
import { useSorting } from '~/composables/sort'
import OdsShowcaseCard from '~/components/showcases/OdsShowcaseCard.vue'
import OdsPagination from '~/components/OdsPagination.vue'

const { locale, t } = useI18n()

const route = useRoute()
const router = useRouter()

const searchInput = ref(Array.isArray(route.query.q) ? route.query.q.join(' ') : route.query.q || '')

const { facetRefs, resetAllFacets } = useFacets(facets)

const onSearch = () => goToPage(1, { q: searchInput.value })

function goToPage(newPage: number | string, query = route.query) {
  const page = newPage ? Number(newPage) : 1
  // Collect all facet values from facetRefs
  const facetsQuery = facets.reduce((acc, facet) => {
    if (facetRefs[facet].value.length > 0) {
      acc[facet] = facetRefs[facet].value
    }
    return acc
  }, {} as Record<string, string[]>)
  router.push({
    name: route.name,
    query: { ...query, ...facetsQuery, page },
  })
}

const searchResultsElement = useTemplateRef<ComponentPublicInstance>('search-results')

const initialSort = `title.${locale.value}+asc`
const page = ref(route.query.page ? Number(route.query.page) - 1 : 0)
const q = ref(Array.isArray(route.query.q) ? route.query.q.join(' ') : route.query.q || '')
const sort = ref(typeof route.query.sort === 'string' ? route.query.sort.replace(/ /g, '+') : initialSort)

// Read the filters from the URL before searching, so they also apply when the page is rendered on the server.
syncFacetsFromRoute({
  facetRefs,
})

const showcases = await usePublishedShowcases()
const {
  searchResults,
  searchResultsCount,
  searchResultsPagesCount,
  availableFacets,
} = await useShowcaseSearch({
  showcases,
  q,
  sort,
  page,
  limit: 9,
  facetRefs,
})

const activeFacets = useActiveFacets({
  facets,
  getAvailableFacetsLocalized: () => availableFacets,
})

const breadcrumbs = [
  homePageBreadcrumb(t),
  {
    title: t('message.header.navigation.showcases'),
    path: '/showcases',
  },
]
useSeoMeta({
  title: `${t('message.header.navigation.showcases')} | opendata.swiss`,
})

function resetSearch() {
  searchInput.value = ''
  facets.forEach((facet) => {
    facetRefs[facet].value = []
  })
  page.value = 0
}

watch(() => route.query.page, (newPage) => {
  page.value = newPage ? Number(newPage) - 1 : 0
})

watch(() => route.query, (queryParam) => {
  if (Object.keys(queryParam).length === 0) {
    // query params are empty
    resetSearch()
  }
  else {
    // syncFacetsFromRoute()
  }
})

watch(() => route.query.q, (searchTerm) => {
  if (searchTerm) {
    searchInput.value = Array.isArray(searchTerm) ? searchTerm.join(' ') : searchTerm
  }
  else {
    searchInput.value = ''
  }
  q.value = searchInput.value
})

onMounted(() => {
  useFacetSync({
    facetRefs,
  })
})

const { selectedSort } = useSorting({
  initialSort,
  sortCallback(sortTerm) {
    selectedSort.value = (Array.isArray(sortTerm) ? sortTerm.join(' ') : sortTerm) ?? initialSort
    sort.value = selectedSort.value
  },
})

const sortOptions = computed(() => {
  return [
    { value: `title.${locale.value}+asc`, text: t('message.showcase.search.sort_by.title_asc') },
    { value: `title.${locale.value}+dsc`, text: t('message.showcase.search.sort_by.title_desc') },
    // Sorting by date (newest, oldest) comes back when the showcases have issued and modified dates.
  ]
})
</script>

<template>
  <OdsPage :hero="{ image: 'https://picsum.photos/1192/894/?image=29', title: t('message.showcase.hero.title') }">
    <template #hero-content>
      <p>
        {{ t('message.showcase.hero.text') }}
      </p>
      <!-- The "Submit a showcase" button comes back in V4 with the new submission form. -->
    </template>
    <template #header>
      <OdsBreadcrumbs :breadcrumbs="breadcrumbs" />
    </template>
    <!-- search panel -->
    <OdsSearchPanel
      :search-input="searchInput"
      :search-prompt="t('message.showcase.search.prompt')"
      :facet-refs="facetRefs"
      :active-facets="activeFacets"
      @search="onSearch"
      @reset-all-facets="resetAllFacets"
      @update:search-input="value => searchInput = value"
    />
    <!-- results -->
    <OdsSearchResults
      ref="search-results"
      :results-count="searchResultsCount"
    >
      <template #header-right>
        <OdsSortSelect
          v-model="selectedSort"
          :options="sortOptions"
        />
      </template>
      <div class="ods-card-list">
        <ul class="search-results-list">
          <li
            v-for="showcase in searchResults"
            :key="showcase.id"
          >
            <OdsShowcaseCard :showcase="showcase" />
          </li>
        </ul>
      </div>
      <div class="pagination pagination--right">
        <OdsPagination
          :current-page="(Number(route.query.page ?? 1))"
          :total-pages="searchResultsPagesCount"
          :pagination-items="[
            {
              icon: 'ChevronLeft',
              label: t('message.ods-pagination.previous'),
              page: Number(route.query.page ?? 1) - 1,
            },
            {
              icon: 'ChevronRight',
              label: t('message.ods-pagination.next'),
              page: Number(route.query.page ?? 1) + 1,
            },
          ]"
          :search-results-element="searchResultsElement ?? undefined"
          @page-change="goToPage"
        />
      </div>
    </OdsSearchResults>
  </OdsPage>
</template>

<style lang="scss" scoped>
ol, ul {
    list-style: none !important;
    margin: 0 !important;
    padding: 0 !important;
}

.ods-card-list {
        margin-top: 2.5rem !important;
}

@media (min-width: 1024px) {
  .ods-card-list {
        margin-top: 3rem !important;
  }
}
@media (min-width: 640px) {
  .ods-card-list {
        margin-top: 2.25rem !important;
  }
}
@media (min-width: 480px) {
    .ods-card-list {
        margin-top: 1.75rem !important;
  }
}

.tag:not(:last-child)::after {
    content: ', ';
}
</style>
