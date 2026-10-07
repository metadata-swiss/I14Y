import type { MaybeRefOrGetter, Ref } from 'vue'
import { computed, toValue } from 'vue'
import { useI18n } from '#imports'
import {
  SearchResourceType,
  useGetSearch,
  useGetSearchCount,
  useGetVocabulariesByIdentifier,
  type GetSearchCountParams,
  type GetSearchParams,
} from '~~/api-client/generated/iop-core'
import type { FacetGroup } from '~/composables/useFacets'
import { organizationLabel } from '~/model/organizations'
import { localize } from '~/utils/getCurrentTranslation'

// iop-core gap [G1] [G3] [G4]: piveau also offered the facets catalog, license and keywords.
export const datasetFacets = ['categories', 'organization', 'format'] as const

// iop-core gap [G1]: GET /api/Search can't filter by DCAT catalog, so the datasets of opendata.swiss
// are approximated by the datasets with public access.
const OPEN_DATASETS = {
  types: [SearchResourceType.Dataset],
  accessRights: ['PUBLIC'],
}

/**
 * The opendata.swiss categories: the EU data themes, as used by the DCAT catalogs of iop-core.
 */
const CATEGORIES_VOCABULARY = 'VOCAB_EU_DATA_THEME'

interface DatasetSearchQueryParams {
  q: Ref<string>
  /** Zero-based, like piveau. */
  page: Ref<number>
  limit: Ref<number>
}

interface UseSearchParams {
  queryParams: DatasetSearchQueryParams
  selectedFacets: Record<string, Ref<string[]>>
}

/**
 * Returns the iop-core dataset search.
 * The members are named like the piveau hub-search definition, which the dataset search page was built on.
 * A single dataset is loaded with the generated useGetDatasetsByIdentifier.
 */
export function useDatasetsSearch() {
  /**
   * Search datasets with GET /api/Search. The facets and the number of results come from GET /api/Search/count.
   */
  function useSearch({ queryParams, selectedFacets }: UseSearchParams) {
    const { t, locale } = useI18n()

    const countParams = computed<GetSearchCountParams>(() => ({
      ...OPEN_DATASETS,
      language: locale.value,
      query: queryParams.q.value || undefined,
      // iop-core gap [G2]: these are the I14Y themes. The opendata.swiss categories (EU data themes of the
      // catalog records) can't be searched.
      themes: selectedFacets.categories?.value,
      publishers: selectedFacets.organization?.value,
      formats: selectedFacets.format?.value,
    }))

    // iop-core gap [G5]: GET /api/Search has no sort parameter; results are sorted by relevance.
    const searchParams = computed<GetSearchParams>(() => ({
      ...countParams.value,
      page: queryParams.page.value + 1,
      pageSize: queryParams.limit.value,
    }))

    const searchQuery = useGetSearch(searchParams)
    const countQuery = useGetSearchCount(countParams)

    const query = {
      suspense: () => Promise.all([searchQuery.suspense(), countQuery.suspense()]),
    }

    const getSearchResultsEnhanced = computed(() => searchQuery.data.value ?? [])

    const getSearchResultsCount = computed(() => countQuery.data.value?.totalDocCount ?? 0)

    const getSearchResultsPagesCount = computed(() => Math.ceil(getSearchResultsCount.value / queryParams.limit.value))

    function getAvailableFacetsLocalized(lang?: MaybeRefOrGetter<string>) {
      return computed<FacetGroup[]>(() => {
        const language = toValue(lang) ?? locale.value
        const counts = countQuery.data.value

        return [
          {
            id: 'categories',
            title: t('message.dataset_detail.categories'),
            items: (counts?.themes ?? []).map(item => ({ id: item.value.code ?? '', title: localize(item.value.name, language), count: item.count ?? 0 })),
          },
          {
            id: 'organization',
            title: t('message.dataset_detail.publisher'),
            items: (counts?.publishers ?? []).map(item => ({ id: item.value.identifier ?? '', title: organizationLabel(item.value, language), count: item.count ?? 0 })),
          },
          {
            id: 'format',
            title: t('message.dataset_detail.format'),
            items: (counts?.formats ?? []).map(item => ({ id: item.value.code ?? '', title: localize(item.value.name, language) || item.value.code || '', count: item.count ?? 0 })),
          },
        ]
      })
    }

    return {
      query,
      getSearchResultsEnhanced,
      getSearchResultsCount,
      getSearchResultsPagesCount,
      getAvailableFacetsLocalized,
    }
  }

  return { useSearch }
}

/**
 * Returns the number of datasets per organization identifier.
 */
export function useDatasetCountByOrganization() {
  const query = useGetSearchCount(OPEN_DATASETS)

  const counts = computed<Record<string, number>>(() => Object.fromEntries(
    (query.data.value?.publishers ?? []).map(item => [item.value.identifier ?? '', item.count ?? 0]),
  ))

  return { query, counts }
}

/**
 * Returns the opendata.swiss categories from GET /api/Vocabularies/{identifier}.
 */
export function useCategories() {
  const query = useGetVocabulariesByIdentifier(CATEGORIES_VOCABULARY)

  const categories = computed(() => query.data.value?.entries ?? [])

  return { query, categories }
}
