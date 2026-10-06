<script setup lang="ts">
import OdsButton from '../OdsButton.vue'
import SvgIcon from '../SvgIcon.vue'

interface OrganizationItem {
  id: string
  resource: string
  pref_label?: Record<string, string>
  name?: Record<string, string>
}

interface OrganizationTreeNode {
  id: string
  organization: OrganizationItem
  children: OrganizationTreeNode[]
}

const props = withDefaults(defineProps<{
  nodes: OrganizationTreeNode[]
  datasetCountByOrganizationId?: Record<string, number>
  showcaseCountByOrganizationId?: Record<string, number>
  level?: number
}>(), {
  level: 0,
})

const { t, locale } = useI18n()
const localePath = useLocalePath()
const expandedNodeIds = ref<Record<string, boolean>>({})

function getLocalizedValue(value?: Record<string, string>) {
  if (!value) {
    return ''
  }

  return value[locale.value] || Object.values(value)[0] || ''
}

function organizationLabel(organization: OrganizationItem) {
  return getLocalizedValue(organization.name) || getLocalizedValue(organization.pref_label) || organization.id
}

function organizationLink(organization: OrganizationItem) {
  return localePath(`/organizations/${encodeURIComponent(organization.id)}`)
}

function organizationLabelShort(organization: OrganizationItem) {
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

function getDatasetCount(organizationId: string) {
  return props.datasetCountByOrganizationId?.[organizationId] || 0
}

function isNodeExpanded(nodeId: string) {
  return expandedNodeIds.value[nodeId] ?? true
}

function toggleNode(nodeId: string) {
  expandedNodeIds.value[nodeId] = !isNodeExpanded(nodeId)
}
</script>

<template>
  <div
    v-for="node in props.nodes"
    :key="node.id"
    class="org-list"
    :data-level="props.level"
  >
    <div class="column">
      <div class="row">
        <div
          class="avatar"
          :data-level="props.level"
        >
          {{ organizationLabelShort(node.organization) }}
        </div>
        <div class="org-props">
          <h2 class="h6 org-title">
            <NuxtLink :to="organizationLink(node.organization)">
              {{ organizationLabel(node.organization) }}
            </NuxtLink>
          </h2>
          <div>
            <p
              class="org-dataset-count"
            >
              {{ t('message.organizations.datasets_count', { count: getDatasetCount(node.organization.id) }) }}
            </p>
          </div>
        </div>
      </div>
      <div class="buttons">
        <div>
          <OdsButton
            v-if="node.children.length > 0"
            variant="link"
            :title="isNodeExpanded(node.id) ? t('message.dataset_search.hide_filters') : t('message.dataset_search.show_filters')"
            :aria-label="isNodeExpanded(node.id) ? t('message.dataset_search.hide_filters') : t('message.dataset_search.show_filters')"
            size="sm"
            @click="toggleNode(node.id)"
          >
            <template #icon>
              <SvgIcon
                icon="ChevronDown"
                role="btn"
                :class="{ rotated: isNodeExpanded(node.id) }"
              />
            </template>
            {{ node.children.length + " " + t('message.organizations.sub_organizations') }}
          </OdsButton>
        </div>
      </div>
    </div>
    <Transition name="expand">
      <div
        v-if="node.children.length > 0"
        v-show="isNodeExpanded(node.id)"
        class="organization-tree__children"
      >
        <OdsOrganizationListItem
          :nodes="node.children"
          :dataset-count-by-organization-id="props.datasetCountByOrganizationId"
          :showcase-count-by-organization-id="props.showcaseCountByOrganizationId"
          :level="props.level + 1"
        />
      </div>
    </Transition>
  </div>
</template>

<style lang="scss" scoped>
.org-list {
  border-left: 2px solid var(--color-border, #d6d9dd);
  margin-left: 2rem;
  padding-left: 2rem;
  padding-bottom: 2rem;
  padding-top: 2rem;
}

.org-list[data-level='0'] {
  margin-top: 0;
  margin-left: 1.25rem;
  padding-left: 1.25rem;
  padding-bottom: 1.25rem;
  padding-top: 1.25rem;
}

.org-list[data-level='1'] {
  margin-left: 1.5rem;
  padding-left: 1.25rem;
  padding-bottom: 0.5rem;
  padding-top: 0.5rem;
}

.organization-tree__children {
  overflow: hidden;
}

.column {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
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
  .buttons {
    display: flex;
    flex-direction: row;
    justify-content: flex-end;
    width: 100%;
  }
}
.rotated {
  transform: rotate(180deg);
  transition: transform 0.2s;
}

.expand-enter-active,
.expand-leave-active {
  transition: max-height 0.28s ease, opacity 0.2s ease, transform 0.22s ease;
}

.expand-enter-from,
.expand-leave-to {
  max-height: 0;
  opacity: 0;
  transform: translateY(-4px);
}

.expand-enter-to,
.expand-leave-from {
  max-height: 1200px;
  opacity: 1;
  transform: translateY(0);
}

@media (prefers-reduced-motion: reduce) {
  .expand-enter-active,
  .expand-leave-active,
  .rotated {
    transition: none;
  }
}

.avatar[data-level='0'] {
  background-color: lightgray;
  height: 48px;
  width: 48px;
}

.avatar[data-level='1'] {
  background-color: lightblue;
  height: 32px;
  width: 32px;
  font-size: smaller;
}
</style>
