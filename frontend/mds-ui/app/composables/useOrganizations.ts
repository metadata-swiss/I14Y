import type { MaybeRefOrGetter, Ref } from 'vue'
import { computed, toValue } from 'vue'
import { useI18n } from '#imports'
import { useGetAgents } from '~~/api-client/generated/iop-core'
import type { FacetGroup } from '~/composables/useFacets'
import type { Organization } from '~/model/organizations'
import { localize } from '~/utils/getCurrentTranslation'

export const organizationFacets = ['classification'] as const

// iop-core gap [G7]: GET /api/Agents has no text search and no facets, so all organizations are loaded and searched here.
const ALL_ORGANIZATIONS = { page: 1, pageSize: 1000 }

/**
 * Returns all organizations from iop-core.
 */
export function useOrganizations() {
  const query = useGetAgents(ALL_ORGANIZATIONS)

  const organizations = computed(() => (query.data.value ?? []).filter((organization): organization is Organization => !!organization.id))

  return { query, organizations }
}

interface UseOrganizationSearchParams {
  q: MaybeRefOrGetter<string>
  selectedFacets: Record<string, Ref<string[]>>
}

/**
 * Search organizations by text and classification.
 * Returns the same members as the piveau hub-search `useSearch`, which the organizations page was built on.
 */
export function useOrganizationSearch({ q, selectedFacets }: UseOrganizationSearchParams) {
  const { t } = useI18n()
  const { query, organizations } = useOrganizations()

  function matchesText(organization: Organization) {
    const text = toValue(q).trim().toLowerCase()
    if (!text) {
      return true
    }
    const values = [organization.identifier, ...Object.values(organization.name ?? {}), ...Object.values(organization.prefLabel ?? {})]
    return values.some(value => value?.toLowerCase().includes(text))
  }

  function matchesClassification(organization: Organization) {
    const selected = selectedFacets.classification?.value ?? []
    return selected.length === 0 || selected.includes(organization.classification?.code ?? '')
  }

  const textMatches = computed(() => organizations.value.filter(matchesText))

  const getSearchResultsEnhanced = computed(() => textMatches.value.filter(matchesClassification))

  const getSearchResultsCount = computed(() => getSearchResultsEnhanced.value.length)

  function getAvailableFacetsLocalized(locale?: MaybeRefOrGetter<string>) {
    return computed<FacetGroup[]>(() => {
      const lang = toValue(locale) ?? 'de'
      const items = new Map<string, FacetGroup['items'][number]>()

      for (const organization of textMatches.value) {
        const classification = organization.classification
        if (!classification?.code) {
          continue
        }
        const item = items.get(classification.code) ?? { id: classification.code, title: localize(classification.name, lang) || classification.code, count: 0 }
        item.count++
        items.set(classification.code, item)
      }

      return [{ id: 'classification', title: t('message.organizations.classification'), items: [...items.values()] }]
    })
  }

  return { query, getSearchResultsEnhanced, getSearchResultsCount, getAvailableFacetsLocalized }
}
