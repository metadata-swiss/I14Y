<template>
  <div
    class="pagination"
    :class="!field ? 'pagination--extended' : ''"
  >
    <div
      v-if="field"
      class="pagination__text"
    >
      {{ pageLabel || t('message.ods-pagination.page') }}
    </div>
    <input
      v-if="field"
      v-model="currentPage"
      class="pagination__input"
      pattern="\d+"
      :class="computedClasses"
      :aria-label="t('message.ods-pagination.input_label')"
      inputmode="numeric"
      type="text"
      @keyup.enter="checkBoundariesAndEmit($event)"
    >
    <div
      v-if="field"
      class="pagination__text"
    >
      {{ totalPagesLabel || t('message.ods-pagination.of', { pageCount: totalPages }) }}
    </div>
    <ul class="pagination_items">
      <li
        v-for="(item, index) in paginationItems"
        :key="`item-${index}`"
      >
        <PaginationItem
          :icon="item.icon"
          :label="item.label"
          :link="'link' in item ? item.link : getPageLink(item.page)"
          :type="type"
          :disabled="(index === 0 && currentPage === 1) || (index === paginationItems.length - 1 && currentPage === totalPages)"
          @click="scrollToResults"
        />
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import PaginationItem from './OdsPaginationItem.vue'
import { computed, type ComponentPublicInstance } from 'vue'
import type { RouteLocationNamedI18n } from 'vue-router'

import { useI18n } from '#imports'

const { t } = useI18n()
const route = useRoute()

const currentPage = defineModel('currentPage', {
  type: Number,
  required: true,
})

function getPageLink(page: number) {
  return { name: route.name, query: { ...route.query, page } } as unknown as RouteLocationNamedI18n
}

const emit = defineEmits({
  pageChange: (_page: number) => true,
})

interface PaginationLinkItem {
  icon?: string
  label?: string
  link: string
}

interface PaginationNumberItem {
  icon?: string
  label?: string
  page: number
}

interface Props {
  type?: 'outline' | 'outline-negative'
  field?: boolean
  totalPagesLabel?: string
  totalPages?: number
  pageLabel?: string
  paginationItems?: Array<PaginationLinkItem | PaginationNumberItem>
  searchResultsElement?: HTMLElement | ComponentPublicInstance
}

const props = withDefaults(defineProps<Props>(), {
  type: 'outline',
  field: true,
  totalPagesLabel: '',
  totalPages: Infinity,
  pageLabel: '',
  paginationItems: () => [],
  searchResultsElement: undefined,
})

const computedClasses = computed(() => {
  let base = 'input input--base '
  if (props.type) base += `input--${props.type} `
  return base
})

function checkBoundariesAndEmit(event: Event) {
  const input = event.target as HTMLInputElement
  let page = parseInt(input.value, 10)

  if (isNaN(page) || page < 1) {
    page = 1
  }
  else if (page > props.totalPages) {
    page = props.totalPages
  }

  currentPage.value = page
  emit('pageChange', page)
  scrollToResults()
}

function scrollToResults() {
  if (!props.searchResultsElement) {
    return
  }

  let element: HTMLElement
  if ('$el' in props.searchResultsElement) {
    element = props.searchResultsElement?.$el
  }
  else {
    element = props.searchResultsElement
  }

  element.scrollIntoView({ behavior: 'smooth', block: 'start' })
}
</script>
