<script setup lang="ts">
import { computed, onMounted, reactive, ref, toRefs, watch } from 'vue'

import OdsPage from '../../app/components/OdsPage.vue'
import OdsBreadcrumbs from '../../app/components/OdsBreadcrumbs.vue'
import OdsSearchPanel from '../../app/components/OdsSearchPanel.vue'
import OdsOrganizationTree from '../../app/components/organizations/OdsOrganizationTree.vue'
import { homePageBreadcrumb } from '../../app/composables/breadcrumbs'
import { useFetch, useRuntimeConfig, useSeoMeta } from 'nuxt/app'
import { useI18n } from 'vue-i18n'
import {
  syncFacetsFromRoute,
  useActiveFacets,
  useFacets,
  useFacetSync,
} from '../../app/composables/useFacets'
import type { Organization } from '../../app/piveau/organizations'
import { useOrganizationSearch, facets } from '../../app/piveau/organizations'
import type { SearchParamsBase } from '@piveau/sdk-core/hubSearch'
import type { OrganizationTreeNode } from '../../app/model/organizations'

interface HubSearchFacetItem {
  id: string
  count: number
}

interface HubSearchFacetGroup {
  id: string
  items: HubSearchFacetItem[]
}

interface HubSearchDatasetFacetsResponse {
  result?: {
    facets?: HubSearchFacetGroup[]
  }
}

const { t, locale } = useI18n()
const router = useRouter()
const route = useRoute()

const searchInput = ref(route.query.q || '')
const baseUrl = useRuntimeConfig().public.piveauHubSearchUrl as string

const onSearch = () => {
  router.push({
    name: route.name,
    query: {
      q: searchInput.value || undefined,
    },
  })
}

const { useSearch } = await useOrganizationSearch()

const { facetRefs, resetAllFacets } = useFacets(facets)

const piveauQueryParams: SearchParamsBase = reactive({
  limit: 1000,
  q: Array.isArray(route.query.q) ? route.query.q.join(' ') : route.query.q || '',
})

watch(() => route.query.q, (searchTerm) => {
  if (searchTerm) {
    searchInput.value = Array.isArray(searchTerm) ? searchTerm.join(' ') : searchTerm
  }
  else {
    searchInput.value = ''
  }
  piveauQueryParams.q = searchInput.value
})

const {
  query,
  getSearchResultsEnhanced: organizations,
  getAvailableFacetsLocalized,
  getSearchResultsCount,
} = useSearch({
  queryParams: toRefs(piveauQueryParams),
  selectedFacets: facetRefs,
})

await query.suspense()

const activeFacets = useActiveFacets({
  facets,
  getAvailableFacetsLocalized,
})

const { data: datasetFacets } = await useFetch<HubSearchDatasetFacetsResponse>(() => `${baseUrl}search`, {
  query: {
    filter: 'dataset',
    limit: 0,
  },
})

const { data: showcaseFacets } = await useFetch<HubSearchDatasetFacetsResponse>(() => `${baseUrl}search`, {
  query: {
    filter: 'resource',
    resource: 'showcase',
    limit: 0,
  },
})

function getLocalizedValue(value?: Record<string, string>) {
  if (!value) {
    return ''
  }

  return value[locale.value] || Object.values(value)[0] || ''
}

function getOrganizationLabel(organization: Organization) {
  return getLocalizedValue(organization.name) || getLocalizedValue(organization.pref_label) || organization.id
}

function getIdFromReference(reference: string) {
  return reference.split('/').filter(Boolean).at(-1) || reference
}

function getParentId(organization: Organization) {
  const directParent = organization.sub_organization_of?.[0]

  if (directParent) {
    return getIdFromReference(directParent)
  }

  const parentFromAncestors = organization.ancestors?.at(-1)

  if (!parentFromAncestors) {
    return undefined
  }

  if (parentFromAncestors.id) {
    return parentFromAncestors.id
  }

  if (parentFromAncestors.resource) {
    return getIdFromReference(parentFromAncestors.resource)
  }

  return undefined
}

const sortedOrganizations = computed(() => {
  const collator = new Intl.Collator(locale.value)

  return [...organizations.value].sort((a, b) => {
    const labelA = getOrganizationLabel(a)
    const labelB = getOrganizationLabel(b)
    return collator.compare(labelA, labelB)
  })
})

function sortTree(nodes: OrganizationTreeNode[]) {
  const collator = new Intl.Collator(locale.value)

  nodes.sort((a, b) => {
    return collator.compare(getOrganizationLabel(a.organization), getOrganizationLabel(b.organization))
  })

  for (const node of nodes) {
    sortTree(node.children)
  }
}

const organizationTree = computed<OrganizationTreeNode[]>(() => {
  const nodesById = new Map<string, OrganizationTreeNode>()

  for (const organization of sortedOrganizations.value) {
    nodesById.set(organization.id, {
      id: organization.id,
      organization,
      children: [],
    })
  }

  const roots: OrganizationTreeNode[] = []

  for (const node of nodesById.values()) {
    const parentId = getParentId(node.organization)
    const parentNode = parentId ? nodesById.get(parentId) : undefined

    if (parentNode && parentNode.id !== node.id) {
      parentNode.children.push(node)
      continue
    }

    roots.push(node)
  }

  sortTree(roots)
  return roots
})

const datasetCountByOrganizationId = computed<Record<string, number>>(() => {
  const counts: Record<string, number> = {}
  const organizationFacet = datasetFacets.value?.result?.facets?.find(facet => facet.id === 'organization')

  if (!organizationFacet) {
    return counts
  }

  for (const item of organizationFacet.items) {
    counts[item.id] = item.count
  }

  return counts
})

const showcaseCountByOrganizationId = computed<Record<string, number>>(() => {
  const counts: Record<string, number> = {}
  const organizationFacet = showcaseFacets.value?.result?.facets?.find(facet => facet.id === 'organization')

  if (!organizationFacet) {
    return counts
  }

  for (const item of organizationFacet.items) {
    counts[item.id] = item.count
  }

  return counts
})

const matchingOrganizationIds = computed(() => {
  return new Set(organizations.value.map(organization => organization.id))
})

function filterTree(nodes: OrganizationTreeNode[], matches: Set<string>): OrganizationTreeNode[] {
  return nodes
    .map((node) => {
      const filteredChildren = filterTree(node.children, matches)

      if (matches.has(node.id) || filteredChildren.length > 0) {
        return {
          ...node,
          children: filteredChildren,
        }
      }

      return null
    })
    .filter((node): node is OrganizationTreeNode => node !== null)
}

const filteredOrganizationTree = computed(() => {
  return filterTree(organizationTree.value, matchingOrganizationIds.value)
})

const breadcrumbs = [
  await homePageBreadcrumb(locale),
  {
    title: t('message.header.navigation.organizations'),
    path: '/organizations',
  },
]

useSeoMeta({
  title: `${t('message.header.navigation.organizations')} | opendata.swiss`,
})

onMounted(() => {
  syncFacetsFromRoute({
    facetRefs,
  })

  useFacetSync({
    facetRefs,
  })
})
</script>

<template>
  <OdsPage :hero="{ title: t('message.header.navigation.organizations') }">
    <template #header>
      <OdsBreadcrumbs :breadcrumbs="breadcrumbs" />
    </template>

    <OdsSearchPanel
      :search-input="searchInput"
      :search-prompt="t('message.organizations.search_placeholder')"
      :title="t('message.header.navigation.organizations')"
      :facet-refs="facetRefs"
      :active-facets="activeFacets"
      @search="onSearch"
      @reset-all-facets="resetAllFacets"
      @update:search-input="value => searchInput = value"
    />

    <section class="section section--default">
      <div class="container">
        <p class="organization-count">
          <strong>{{ getSearchResultsCount }}</strong>
          {{ t('message.header.navigation.organizations') }}
        </p>

        <OdsOrganizationTree
          v-if="organizations.length > 0"
          :nodes="filteredOrganizationTree"
          :dataset-count-by-organization-id="datasetCountByOrganizationId"
          :showcase-count-by-organization-id="showcaseCountByOrganizationId"
        />

        <p
          v-else
          class="notification notification--info"
        >
          {{ t('message.organizations.empty') }}
        </p>
      </div>
    </section>
  </OdsPage>
</template>

<style lang="scss" scoped>
.organization-count {
  margin-bottom: 1.5rem;
}
</style>
