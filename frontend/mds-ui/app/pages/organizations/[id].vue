<script setup lang="ts">
import { computed } from 'vue'

import OdsPage from '~/components/OdsPage.vue'
import OdsBreadcrumbs from '~/components/OdsBreadcrumbs.vue'
import OdsCard from '~/components/content/OdsCard.vue'
import OdsInfoBlock from '~/components/OdsInfoBlock.vue'
import OdsOrganizationListItem from '~/components/organizations/OdsOrganizationListItem.vue'
import { homePageBreadcrumb } from '~/composables/breadcrumbs'
import { useSeoMeta } from 'nuxt/app'
import { useI18n } from 'vue-i18n'
import { useOrganizations } from '~/composables/useOrganizations'
import { getParentId, organizationLabel, type Organization, type OrganizationTreeNode } from '~/model/organizations'
import { useDatasetCountByOrganization } from '~/composables/useDatasets'
import { localize } from '~/utils/getCurrentTranslation'

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

// The organization is looked up in the list of all organizations, which the tree of sub organizations needs anyway.
const { query: organizationsQuery, organizations } = useOrganizations()
const { query: datasetCountQuery, counts: datasetCountByOrganizationId } = useDatasetCountByOrganization()

await Promise.all([organizationsQuery.suspense(), datasetCountQuery.suspense()])

const organizationPending = organizationsQuery.isPending
const organizationError = organizationsQuery.error

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

const organizationById = computed(() => {
  return new Map(sortedOrganizations.value.map(organization => [organization.id, organization]))
})

const organization = computed(() => organizations.value.find(item => item.identifier === organizationId.value))

const parentOrganization = computed(() => {
  const currentOrganization = organization.value
  if (!currentOrganization) {
    return undefined
  }

  const parentId = getParentId(currentOrganization)
  return parentId ? organizationById.value.get(parentId) : undefined
})

// iop-core has no hierarchy level; it's the number of parents.
const hierarchyLevel = computed(() => {
  let level = 0
  let current = organization.value
  const visited = new Set<string>()

  while (current && !visited.has(current.id)) {
    visited.add(current.id)
    const parentId = getParentId(current)
    current = parentId ? organizationById.value.get(parentId) : undefined
    if (current) {
      level++
    }
  }

  return level
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

// Showcases come from Nuxt Content in phase 4; until then no showcase counts are shown.
const showcaseCountByOrganizationId = computed<Record<string, number>>(() => ({}))

const datasetCount = computed(() => datasetCountByOrganizationId.value[organizationId.value] ?? 0)
const showcaseCount = computed(() => showcaseCountByOrganizationId.value[organizationId.value] ?? 0)

const localizedDescription = computed(() => localize(organization.value?.description, locale.value))
const classifications = computed(() => organization.value?.classification ? [organization.value.classification] : [])

function organizationLink(organization: Organization) {
  return localePath(`/organizations/${encodeURIComponent(organization.identifier ?? '')}`)
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
              {{ organization.identifier }}
            </p>

            <!-- iop-core gap [G8]: agents have no URI, so the link to the organization's resource isn't shown. -->

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
              v-if="organization.homePage"
              :title="t('message.organizations.homepage')"
            >
              <a
                :href="organization.homePage"
                target="_blank"
                rel="noopener noreferrer"
                class="link--external"
              >
                {{ organization.homePage }}
              </a>
            </OdsInfoBlock>

            <OdsInfoBlock
              :title="t('message.organizations.hierarchy_level')"
            >
              {{ hierarchyLevel }}
            </OdsInfoBlock>

            <OdsInfoBlock
              v-if="classifications.length > 0"
              :title="t('message.organizations.classification')"
            >
              <ul class="sub-organization-list">
                <li
                  v-for="item in classifications"
                  :key="item.code ?? ''"
                >
                  <a
                    v-if="item.uri"
                    :href="item.uri"
                    target="_blank"
                    rel="noopener noreferrer"
                    class="link--external"
                  >
                    {{ localize(item.name, locale) || item.code || item.uri }}
                  </a>
                  <template v-else>
                    {{ localize(item.name, locale) || item.code }}
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
