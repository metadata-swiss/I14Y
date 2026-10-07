<script setup lang="ts">
import { computed, onMounted, reactive, ref, toRefs, useTemplateRef, watch, type ComponentPublicInstance } from 'vue'

import { useRoute, useRouter } from 'vue-router'
import { useI18n } from '#imports'

import { useDatasetsSearch, datasetFacets } from '~/composables/useDatasets'
import OdsBreadcrumbs, { type BreadcrumbItem } from '~/components/OdsBreadcrumbs.vue'
import OdsPagination from '~/components/OdsPagination.vue'
import OdsDatasetList from '~/components/dataset/dataset-list/OdsDatasetList.vue'
import OdsListCardToggle from '~/components/dataset/list-card-toggle/OdsListCardToggle.vue'
import { homePageBreadcrumb } from '~/composables/breadcrumbs'
import SvgIcon from '~/components/SvgIcon.vue'
import { useSeoMeta } from 'nuxt/app'
import { clearDatasetBreadcrumbFromSessionStorage } from '~/utils/breadcrumb-session-storage'
import { DcatApChV2DatasetAdapter } from '~/model/dataset/dcat-ap-ch-v2-dataset-adapter'
import { syncFacetsFromRoute, useActiveFacets, useFacets, useFacetSync } from '~/composables/useFacets'

import OdsSearchPanel from '~/components/OdsSearchPanel.vue'
import OdsSearchResults from '~/components/OdsSearchResults.vue'
import type { AppLanguage } from '~/constants/langages'

const { t, locale } = useI18n()

const router = useRouter()
const route = useRoute()
const { facetRefs, resetAllFacets } = useFacets(datasetFacets)

// Read the filters from the URL before searching, so they also apply when the page is rendered on the server.
syncFacetsFromRoute({
  facetRefs,
})

if (import.meta.client) {
  clearDatasetBreadcrumbFromSessionStorage()
}

function resetSearch() {
  searchInput.value = ''
  datasetFacets.forEach((facet) => {
    facetRefs[facet].value = []
  })
  queryParams.page = 0
}

// iop-core gap [G5]: GET /api/Search can't sort, so the sort select (relevance, title, modification date) is hidden.

const queryParams = reactive({
  limit: 10,
  page: route.query.page ? Number(route.query.page) - 1 : 0,
  q: Array.isArray(route.query.q) ? route.query.q.join(' ') : route.query.q || '',
})

const { useSearch } = useDatasetsSearch()
const {
  query,
  getSearchResultsEnhanced,
  getSearchResultsCount,
  getSearchResultsPagesCount,
  getAvailableFacetsLocalized,
} = useSearch({
  queryParams: toRefs(queryParams),
  selectedFacets: facetRefs,
})

const datasets = computed(() => {
  const result = getSearchResultsEnhanced.value
  if (!result) {
    return []
  }
  return getSearchResultsEnhanced.value.map(item => DcatApChV2DatasetAdapter.fromSearchResult(item, locale.value as AppLanguage, t))
})

const { suspense } = query

const LIST_TYPE_KEY = 'datasets-list-type'
const getInitialListType = () => {
  const stored = typeof window !== 'undefined' ? window.localStorage.getItem(LIST_TYPE_KEY) : null
  return stored === 'card' ? 'card' : 'list'
}
const listType = ref<'card' | 'list'>(getInitialListType())

watch(listType, (newType) => {
  if (typeof window !== 'undefined') {
    window.localStorage.setItem(LIST_TYPE_KEY, newType)
  }
})

const activeFacets = useActiveFacets({
  facets: datasetFacets,
  getAvailableFacetsLocalized,
})

function goToPage(newPage: number | string, query = route.query) {
  const page = newPage ? Number(newPage) : 1
  // Collect all facet values from facetRefs
  const facetsQuery = datasetFacets.reduce((acc, facet) => {
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

const searchInput = ref(queryParams.q)
const onSearch = () => goToPage(1, { q: searchInput.value })

const homeBreadcrumb = await homePageBreadcrumb(locale)
const datasetBreadcrumb = computed<BreadcrumbItem>(() => {
  const bc = {
    id: 'datasets',
    title: t('message.header.navigation.datasets'),
    route: '/datasets',
  }
  return bc
})

const resultBreadcrumb = computed<BreadcrumbItem | null>(() => {
  const notFirstPage = route.query.page && Number(route.query.page) > 1
  const hasFacetFilters = Object.values(facetRefs).some(facetRef => facetRef.value.length > 0)
  const hasOtherQueryParams = Object.keys(route.query).length > 1 || (route.query.q && route.query.q !== '')
  if (notFirstPage || hasOtherQueryParams || hasFacetFilters) {
    const resultBc = {
      id: 'search-results',
      title: t('message.dataset_search.search_results'),
      route,
    }
    return resultBc
  }
  return null
})

const breadcrumbs = computed<BreadcrumbItem[]>(() => {
  const lastBreadcrumb = resultBreadcrumb.value
  if (lastBreadcrumb === null) {
    return [
      homeBreadcrumb,
      datasetBreadcrumb.value,
    ]
  }
  return [
    homeBreadcrumb,
    datasetBreadcrumb.value,
    lastBreadcrumb,
  ]
})

watch(() => route.query.page, (newPage) => {
  queryParams.page = newPage ? Number(newPage) - 1 : 0
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
  queryParams.q = searchInput.value
})

onMounted(() => {
  useFacetSync({
    facetRefs,
  })
})

useSeoMeta({
  title: `${t('message.header.navigation.datasets')} | opendata.swiss`,
})

await suspense()
</script>

<template>
  <div>
    <header id="main-header">
      <OdsBreadcrumbs :breadcrumbs="breadcrumbs" />
    </header>

    <main id="main-content">
      <!-- search panel -->
      <OdsSearchPanel
        :search-input="searchInput"
        :search-prompt="t('message.dataset_search.search_placeholder')"
        :facet-refs="facetRefs"
        :active-facets="activeFacets"
        @search="onSearch"
        @reset-all-facets="resetAllFacets"
        @update:search-input="value => searchInput = value"
      />
      <!-- results -->

      <OdsSearchResults
        ref="search-results"
        :results-count="getSearchResultsCount"
      >
        <template #header-right>
          <!-- iop-core gap [G5]: no sorting in GET /api/Search.
          <OdsSortSelect
            v-model="selectedSort"
            :options="sortOptions"
          />
          <div class="separator separator--vertical" />
          -->
          <OdsListCardToggle v-model="listType" />
        </template>

        <!-- <div v-if="isFetching" class="is-fetching">
              Fetching...
            </div> -->
        <OdsDatasetList
          :items="datasets"
          :list-type="listType"
          :search-params="route.query"
        />
        <div class="pagination pagination--right">
          <OdsPagination
            :current-page="(Number(route.query.page ?? 1))"
            :total-pages="getSearchResultsPagesCount"
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

        <div class="notification notification--info">
          <SvgIcon
            icon="InfoCircle"
            role="notification"
          />
          <div class="notification__content">
            <div class="text--bold">
              Haben Sie nicht gefunden wonach Sie suchen?
            </div>
            <div>
              Gerne geben wir Ihnen auch persönlich Auskunft. Bitte melden Sie sich
              via Kontaktformular bei uns.
            </div>
            <a
              href="#"
              class="link"
            >Kontaktformular</a>
          </div>
        </div>
      </OdsSearchResults>
    </main>
  </div>
</template>

<style lang="scss" scoped>

</style>
