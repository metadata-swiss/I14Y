<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'

import OdsPage from '~/components/OdsPage.vue'
import OdsBreadcrumbs from '~/components/OdsBreadcrumbs.vue'
import OdsSearchPanel from '~/components/OdsSearchPanel.vue'
import OdsOrganizationTree from '~/components/organizations/OdsOrganizationTree.vue'
import { homePageBreadcrumb } from '~/composables/breadcrumbs'
import { useSeoMeta } from 'nuxt/app'
import { useI18n } from 'vue-i18n'
import {
  syncFacetsFromRoute,
  useActiveFacets,
  useFacets,
  useFacetSync,
} from '~/composables/useFacets'
import { getParentId, organizationLabel, type Organization, type OrganizationTreeNode } from '~/model/organizations'
import { useOrganizationSearch, organizationFacets } from '~/composables/useOrganizations'
import { useDatasetCountByOrganization } from '~/composables/useDatasets'

const { t, locale } = useI18n()
const router = useRouter()
const route = useRoute()

const searchInput = ref(Array.isArray(route.query.q) ? route.query.q.join(' ') : route.query.q || '')

const onSearch = () => {
  router.push({
    name: route.name,
    query: {
      q: searchInput.value || undefined,
    },
  })
}

const { facetRefs, resetAllFacets } = useFacets(organizationFacets)

// Read the filters from the URL before searching, so they also apply when the page is rendered on the server.
syncFacetsFromRoute({
  facetRefs,
})

const queryParams = reactive({
  q: searchInput.value,
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

const {
  query,
  getSearchResultsEnhanced: organizations,
  getAvailableFacetsLocalized,
  getSearchResultsCount,
} = useOrganizationSearch({
  q: () => queryParams.q,
  selectedFacets: facetRefs,
})

const { query: datasetCountQuery, counts: datasetCountByOrganizationId } = useDatasetCountByOrganization()

await Promise.all([query.suspense(), datasetCountQuery.suspense()])

const activeFacets = useActiveFacets({
  facets: organizationFacets,
  getAvailableFacetsLocalized,
})

// Showcases come from Nuxt Content in phase 4; until then no showcase counts are shown.
const showcaseCountByOrganizationId = computed<Record<string, number>>(() => ({}))

function getOrganizationLabel(organization: Organization) {
  return organizationLabel(organization, locale.value)
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
