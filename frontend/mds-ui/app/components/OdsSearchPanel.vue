<template>
  <section
    :class="{ 'section section--default bg--secondary-50': !small }"
    :aside="aside ? 'true' : 'false'"
  >
    <div class="container">
      <template v-if="!small">
        <h3
          v-if="aside"
          class="h3"
        >
          {{ title || t('message.dataset_search.search_results') }}
        </h3>
        <h1
          v-else
          class="h1"
        >
          {{ title || t('message.dataset_search.search_results') }}
        </h1>
      </template>
      <div class="search search--large search--page-result">
        <div class="search__group">
          <input
            id="search-input"
            ref="_inputElement"
            v-model="modelValue"
            :placeholder="searchPrompt"
            type="search"
            autocomplete="off"
            class="search"
            @keyup.enter="onSearch"
          >
          <OdsButton
            variant="bare"
            :title="t('message.dataset_search.search_button')"
            size="lg"
            icon="Search"
            icon-only
            @click="onSearch"
          />
        </div>
      </div>
      <div
        v-if="facetRefs && activeFacets"
        class="search__filters"
      >
        <OdsFilterPanel
          :facet-refs="facetRefs"
          :facets="activeFacets"
          @reset-all-facets="$emit('reset-all-facets')"
        />
      </div>
      <div
        v-if="facetRefs && activeFacets"
        class="filters__active"
      />
    </div>
  </section>
</template>

<script setup lang="ts">
import OdsButton from './OdsButton.vue'
import OdsFilterPanel from '~/components/dataset/OdsFilterPanel.vue'
import type { SearchResultFacetGroupLocalized } from '@piveau/sdk-vue'

const { t } = useI18n()

interface PropTypes {
  searchInput: string | string[]
  searchPrompt: string
  aside?: boolean
  small?: boolean
  title?: string
  facetRefs?: Record<string, Ref<string[]>>
  activeFacets?: SearchResultFacetGroupLocalized[]
  autoSearch?: boolean
}

const { searchInput, autoSearch, activeFacets } = defineProps<PropTypes>()

const emit = defineEmits({
  'search': (_: string) => true,
  'reset-all-facets': () => true,
  'update:searchInput': (_: string | string[]) => true,
})

const modelValue = computed({
  get: () => {
    if (Array.isArray(searchInput)) {
      return searchInput[0] || ''
    }
    return searchInput || ''
  },
  set: (value: string) => {
    emit('update:searchInput', value)
    if (autoSearch) {
      onSearch()
    }
  },
})

const _inputElement = ref<HTMLInputElement | null>(null)

const onSearch = () => {
  emit('search', modelValue.value?.trim() || '')
}
</script>

<style scoped>
section[aside] .search {
  width: 100%
}

h3 {
  margin-top: 0;
}
</style>
