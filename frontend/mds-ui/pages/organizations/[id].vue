<script setup lang="ts">
import { computed } from 'vue'

import OdsPage from '../../app/components/OdsPage.vue'
import OdsBreadcrumbs from '../../app/components/OdsBreadcrumbs.vue'
import OdsCard from '../../app/components/content/OdsCard.vue'
import OdsInfoBlock from '../../app/components/OdsInfoBlock.vue'
import OdsOrganizationListItem from '../../app/components/organizations/OdsOrganizationListItem.vue'
import { homePageBreadcrumb } from '../../app/composables/breadcrumbs'
import { useFetch, useRuntimeConfig, useSeoMeta } from 'nuxt/app'
import { useI18n } from 'vue-i18n'

interface OrganizationClassification {
  resource?: string
  id?: string
  label?: Record<string, string>
}

interface OrganizationItem {
  id: string
  resource: string
  identifier?: string
  homepage?: string
  description?: Record<string, string>
  hierarchy_level?: number
  classification?: OrganizationClassification[]
  sub_organization_of?: string[]
  pref_label?: Record<string, string>
  name?: Record<string, string>
  ancestors?: Array<{
    id?: string
    resource?: string
    name?: Record<string, string>
    pref_label?: Record<string, string>
  }>
}

interface OrganizationTreeNode {
  id: string
  organization: OrganizationItem
  children: OrganizationTreeNode[]
}

interface HubSearchOrganizationsResponse {
  result?: {
    results?: OrganizationItem[]
  }
}

interface HubSearchOrganizationByIdResponse {
  success?: boolean
  result?: OrganizationItem
}

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

const route = useRoute()
const { t, locale } = useI18n()
const localePath = useLocalePath()

definePageMeta({
  path: '/organizations/:id',
})

const organizationId = computed(() => {
  const rawId = String(route.params.id ?? '')
  try {
    return decodeURIComponent(rawId)
  }
  catch {
    return rawId
  }
})
const baseUrl = useRuntimeConfig().public.piveauHubSearchUrl as string

const { data: organizationData, pending: organizationPending, error: organizationError } = await useFetch<HubSearchOrganizationByIdResponse>(
  () => `${baseUrl}organizations/${encodeURIComponent(organizationId.value)}`,
)

const { data: organizationsData } = await useFetch<HubSearchOrganizationsResponse>(() => `${baseUrl}search`, {
  query: {
    filter: 'organization',
    limit: 1000,
  },
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

function getOrganizationLabel(organization: OrganizationItem) {
  return getLocalizedValue(organization.name) || getLocalizedValue(organization.pref_label) || organization.id
}

function getIdFromReference(reference: string) {
  return reference.split('/').filter(Boolean).at(-1) || reference
}

function getParentId(organization: OrganizationItem) {
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

const organizations = computed(() => organizationsData.value?.result?.results ?? [])

const sortedOrganizations = computed(() => {
  const collator = new Intl.Collator(locale.value)

  return [...organizations.value].sort((a, b) => {
    const labelA = getOrganizationLabel(a)
    const labelB = getOrganizationLabel(b)
    return collator.compare(labelA, labelB)
  })
})

const organizationById = computed(() => {
  return new Map(sortedOrganizations.value.map(organization => [organization.id, organization]))
})

const organization = computed(() => organizationData.value?.result)

const parentOrganization = computed(() => {
  const currentOrganization = organization.value
  if (!currentOrganization) {
    return undefined
  }

  const parentFromAncestors = currentOrganization.ancestors?.at(-1)
  if (parentFromAncestors && (parentFromAncestors.id || parentFromAncestors.resource)) {
    return {
      id: parentFromAncestors.id ?? (parentFromAncestors.resource ? getIdFromReference(parentFromAncestors.resource) : ''),
      resource: parentFromAncestors.resource ?? '',
      name: parentFromAncestors.name,
      pref_label: parentFromAncestors.pref_label,
    } as OrganizationItem
  }

  const parentId = getParentId(currentOrganization)
  return parentId ? organizationById.value.get(parentId) : undefined
})

const subOrganizationNodes = computed<OrganizationTreeNode[]>(() => {
  const currentId = organization.value?.id
  if (!currentId) {
    return []
  }

  const nodesById = new Map<string, OrganizationTreeNode>()

  for (const item of sortedOrganizations.value) {
    nodesById.set(item.id, {
      id: item.id,
      organization: item,
      children: [],
    })
  }

  for (const node of nodesById.values()) {
    const parentId = getParentId(node.organization)
    const parentNode = parentId ? nodesById.get(parentId) : undefined
    if (parentNode && parentNode.id !== node.id) {
      parentNode.children.push(node)
    }
  }

  const currentNode = nodesById.get(currentId)
  return currentNode?.children ?? []
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

const datasetCount = computed(() => datasetCountByOrganizationId.value[organizationId.value] ?? 0)
const showcaseCount = computed(() => showcaseCountByOrganizationId.value[organizationId.value] ?? 0)

const localizedDescription = computed(() => getLocalizedValue(organization.value?.description))
const classifications = computed(() => organization.value?.classification ?? [])

function organizationLink(organization: OrganizationItem) {
  return localePath(`/organizations/${encodeURIComponent(organization.id)}`)
}

const datasetsLink = computed(() => {
  return {
    path: localePath('/datasets'),
    query: {
      organization: organizationId.value,
    },
  }
})

const homeBreadcrumb = await homePageBreadcrumb(locale)

const breadcrumbs = computed(() => [
  homeBreadcrumb,
  {
    title: t('message.header.navigation.organizations'),
    path: '/organizations',
  },
  {
    title: organization.value ? getOrganizationLabel(organization.value) : organizationId.value,
  },
])

useSeoMeta({
  title: computed(() => `${organization.value ? getOrganizationLabel(organization.value) : organizationId.value} | ${t('message.header.navigation.organizations')} | opendata.swiss`),
})
</script>

<template>
  <OdsPage :hero="{ title: organization ? getOrganizationLabel(organization) : organizationId }">
    <template #header>
      <OdsBreadcrumbs :breadcrumbs="breadcrumbs" />
    </template>

    <section class="section section--default">
      <div class="container">
        <p
          v-if="organizationError"
          class="notification notification--danger"
        >
          {{ t('message.organizations.load_error') }}
        </p>

        <p
          v-else-if="!organizationPending && !organization"
          class="notification notification--info"
        >
          {{ t('message.organizations.empty') }}
        </p>

        <div
          v-else-if="organization"
          class="grid"
        >
          <OdsCard :title="getOrganizationLabel(organization)">
            <p class="organization-meta">
              {{ organization.id }}
            </p>

            <a
              v-if="organization.resource"
              :href="organization.resource"
              target="_blank"
              rel="noopener noreferrer"
              class="link--external"
            >
              {{ organization.resource }}
            </a>

            <div class="metrics">
              <div class="metric">
                <p class="metric__value">
                  {{ datasetCount }}
                </p>
                <p class="metric__label">
                  {{ t('message.organizations.datasets_count', { count: datasetCount }) }}
                </p>
              </div>
              <div class="metric">
                <p class="metric__value">
                  {{ showcaseCount }}
                </p>
                <p class="metric__label">
                  {{ t('message.header.navigation.showcases') }}
                </p>
              </div>
            </div>

            <NuxtLink
              v-if="datasetCount > 0"
              :to="datasetsLink"
              class="organization-datasets-link"
            >
              {{ t('message.organizations.show_datasets') }}
            </NuxtLink>
          </OdsCard>

          <OdsCard :title="t('message.dataset_detail.additional_information')">
            <OdsInfoBlock
              v-if="organization.identifier"
              :title="t('message.organizations.identifier')"
            >
              {{ organization.identifier }}
            </OdsInfoBlock>

            <OdsInfoBlock
              v-if="organization.homepage"
              :title="t('message.organizations.homepage')"
            >
              <a
                :href="organization.homepage"
                target="_blank"
                rel="noopener noreferrer"
                class="link--external"
              >
                {{ organization.homepage }}
              </a>
            </OdsInfoBlock>

            <OdsInfoBlock
              v-if="organization.hierarchy_level !== undefined"
              :title="t('message.organizations.hierarchy_level')"
            >
              {{ organization.hierarchy_level }}
            </OdsInfoBlock>

            <OdsInfoBlock
              v-if="classifications.length > 0"
              :title="t('message.organizations.classification')"
            >
              <ul class="sub-organization-list">
                <li
                  v-for="item in classifications"
                  :key="item.id || item.resource"
                >
                  <a
                    v-if="item.resource"
                    :href="item.resource"
                    target="_blank"
                    rel="noopener noreferrer"
                    class="link--external"
                  >
                    {{ getLocalizedValue(item.label) || item.id || item.resource }}
                  </a>
                  <template v-else>
                    {{ getLocalizedValue(item.label) || item.id }}
                  </template>
                </li>
              </ul>
            </OdsInfoBlock>

            <OdsInfoBlock
              v-if="localizedDescription"
              :title="t('message.organizations.description')"
            >
              {{ localizedDescription }}
            </OdsInfoBlock>
          </OdsCard>

          <OdsCard
            v-if="parentOrganization"
            :title="t('message.organizations.parent')"
          >
            <NuxtLink :to="organizationLink(parentOrganization)">
              {{ getOrganizationLabel(parentOrganization) }}
            </NuxtLink>
          </OdsCard>

          <OdsCard
            v-if="subOrganizationNodes.length > 0"
            :title="t('message.organizations.sub_organizations')"
          >
            <OdsOrganizationListItem
              :nodes="subOrganizationNodes"
              :dataset-count-by-organization-id="datasetCountByOrganizationId"
              :showcase-count-by-organization-id="showcaseCountByOrganizationId"
              :level="0"
            />
          </OdsCard>
        </div>
      </div>
    </section>
  </OdsPage>
</template>

<style lang="scss" scoped>
.grid {
  display: grid;
  gap: 1rem;
}

.metrics {
  display: flex;
  gap: 1.5rem;
  margin-top: 1rem;
  margin-bottom: 1rem;
}

.metric {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.metric__value {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 700;
}

.metric__label {
  margin: 0;
  color: var(--color-text-muted, #5a6270);
}

.organization-meta {
  color: var(--color-text-muted, #5a6270);
  margin: 0 0 0.5rem;
}

.organization-datasets-link {
  font-weight: 600;
}

.sub-organization-list {
  margin: 0;
  padding-left: 1rem;
}
</style>
