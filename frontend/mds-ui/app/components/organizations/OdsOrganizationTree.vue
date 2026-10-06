<script setup lang="ts">
import OdsOrganizationListItem from './OdsOrganizationListItem.vue'
import OdsCard from '../content/OdsCard.vue'
import type { Organization } from '~/piveau/organizations.ts'
import type { OrganizationTreeNode } from '~/model/organizations.ts'

const props = defineProps<{
  nodes: OrganizationTreeNode[]
  datasetCountByOrganizationId?: Record<string, number>
  showcaseCountByOrganizationId?: Record<string, number>
}>()

const { t, locale } = useI18n()
const localePath = useLocalePath()

function getLocalizedValue(value?: Record<string, string>) {
  if (!value) {
    return ''
  }

  return value[locale.value] || Object.values(value)[0] || ''
}

function organizationLabel(organization: Organization) {
  return getLocalizedValue(organization.name) || getLocalizedValue(organization.pref_label) || organization.id
}

function organizationLink(organization: Organization) {
  return localePath(`/organizations/${encodeURIComponent(organization.id)}`)
}

function getDatasetCount(organizationId: string) {
  return props.datasetCountByOrganizationId?.[organizationId] || 0
}

function getShowcaseCount(organizationId: string) {
  return props.showcaseCountByOrganizationId?.[organizationId] || 0
}

function organizationLabelShort(organization: Organization) {
  const label = getLocalizedValue(organization.name) || getLocalizedValue(organization.pref_label) || organization.id
  const parts = label
    .split(/[\s-]+/)
    .filter(Boolean)

  const uppercaseInitials = parts
    .filter(part => /^[A-ZÀ-ÖØ-Þ]/.test(part))
    .map(part => part.charAt(0))

  if (uppercaseInitials.length >= 2) {
    return `${uppercaseInitials[0]}${uppercaseInitials[1]}`.toUpperCase()
  }

  if (parts.length >= 2) {
    return `${parts[0]!.charAt(0)}${parts[1]!.charAt(0)}`.toUpperCase()
  }

  return label.replace(/\s+/g, '').slice(0, 2).toUpperCase()
}
</script>

<template>
  <div class="list">
    <OdsCard
      v-for="node in props.nodes"
      :key="node.id"
      :title="organizationLabel(node.organization)"
      type="default"
    >
      <template #title>
        <div class="title">
          <div class="row">
            <div
              class="avatar"
            >
              {{ organizationLabelShort(node.organization) }}
            </div>
            <div class="org-props">
              <h2 class="h5 org-title">
                <NuxtLink :to="organizationLink(node.organization)">
                  {{ organizationLabel(node.organization) }}
                </NuxtLink>
              </h2>
            </div>
          </div>
          <div class="dataset-and-showcases">
            <div class="item">
              <p class="value">
                {{ getShowcaseCount(node.organization.id) }}
              </p>
              <p class="text">
                {{ t('message.header.navigation.showcases') }}
              </p>
            </div>
            <div class="item">
              <p class="value">
                {{ getDatasetCount(node.organization.id) }}
              </p>
              <p class="text">
                {{ t('message.organizations.datasets_count') }}
              </p>
            </div>
          </div>
        </div>
      </template>
      <!--
      <div class="organization-tree__row">
        <div>
          <p class="organization-tree__meta">
            {{ node.organization.id }}
          </p>
          <p class="organization-tree__meta">
            {{ t('message.organizations.datasets_count', { count: getDatasetCount(node.organization.id) }) }}
          </p>
        </div>

        <NuxtLink
          v-if="getDatasetCount(node.organization.id) > 0"
          class="organization-tree__link"
          :to="datasetsLink(node.organization)"
        >
          {{ t('message.organizations.show_datasets') }}
        </NuxtLink>
      </div>
      -->

      <OdsOrganizationListItem
        v-if="node.children.length > 0"
        :nodes="node.children"
        :dataset-count-by-organization-id="props.datasetCountByOrganizationId"
        :showcase-count-by-organization-id="props.showcaseCountByOrganizationId"
      />
    </OdsCard>
  </div>
</template>

<style lang="scss" scoped>
.list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.row {
  display: flex;
  flex-direction: row;
  justify-content: flex-start;
  align-items: center;
  gap: 12px;
  .avatar {
    display: flex;
    flex-direction: row;
    align-items: center;
    justify-content: center;
    background-color: lightgray;
    height: 48px;
    width: 48px;
  }
  .org-props {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
    .org-level {
      margin: 0;
      font-size: 0.75rem;
      line-height: 1.2;
      color: #4b5563;
      background: #eef2f7;
      border: 1px solid #d9e0ea;
      border-radius: 999px;
      padding: 0.15rem 0.5rem;
      width: fit-content;
    }
  }
}

.title {
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  .org-props {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
    align-items: flex-start;
    height: 100%;
  }
  .dataset-and-showcases {
    display: flex;
    flex-direction: row;
    gap: 0.75rem;
    .item {
      display: flex;
      flex-direction: column;
      .value {
        text-align: right;
        font-weight: bolder;
      }
      .text {
        color: gray;
      }
    }
  }
}
</style>
