<script setup lang="ts">
import { type RouteLocationNamedI18n, useRouter } from 'vue-router'
import MiniSearch, { type SearchResult } from 'minisearch'
import OdsBreadcrumbs from '../../app/components/OdsBreadcrumbs.vue'
import OdsPage from '../../app/components/OdsPage.vue'
import OdsSearchPanel from '../../app/components/OdsSearchPanel.vue'
import OdsCard from './content/OdsCard.vue'
import OdsSearchResults from '../../app/components/OdsSearchResults.vue'
import type { BreadcrumbItem } from '~/components/OdsBreadcrumbs.vue'
import { debounce } from 'perfect-debounce'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()

const searchInput = ref(route.query.q as string)

const onSearch = debounce((value: string) => {
  searchInput.value = value
  if (route.query.q === value?.trim()) return

  router.push({
    name: route.name,
    query: { q: value?.trim() || undefined },
  })
}, 300)

watch(searchInput, onSearch)

interface SearchData {
  id: string
  title: string
  titles: string[]
  content: string
}

const { data } = defineProps<{
  searchPrompt: string
  data: SearchData[]
  breadcrumbs: BreadcrumbItem[]
  searchResultToLink: (result: SearchResult) => RouteLocationNamedI18n
  searchResultToImage?: (result: SearchResult) => string
}>()

const miniSearch = computed(() => {
  const ms = new MiniSearch({
    fields: ['title', 'content'],
    storeFields: ['title', 'content'],
    searchOptions: {
      prefix: true,
      fuzzy: 0.2,
    },
  })

  ms.addAll(data)

  return ms
})

const result = computed(() => {
  if (!miniSearch.value) return []

  return miniSearch.value.search(toValue(searchInput.value) || '')
    .sort((a, b) => a.score > b.score ? -1 : a.score < b.score ? 1 : 0)
    .slice(0, 10)
})

watch(
  () => route.query.q,
  (value) => {
    searchInput.value = value as string
  },
)
</script>

<template>
  <OdsPage>
    <template #header>
      <OdsBreadcrumbs :breadcrumbs="breadcrumbs" />
      <OdsSearchPanel
        v-model:search-input="searchInput"
        small
        :search-prompt="searchPrompt"
        @search="onSearch"
      />
    </template>
    <OdsSearchResults :results-count="result.length">
      <OdsCard
        v-for="article in result"
        :key="article.id"
        :title="article.title"
        type="list"
        clickable
      >
        <p>{{ article.content }}</p>

        <template
          v-if="searchResultToImage"
          #image
        >
          <NuxtImg
            :src="searchResultToImage(article)"
            :alt="article.title"
          />
        </template>

        <template #footer-action>
          <NuxtLinkLocale
            :to="searchResultToLink(article)"
            class="btn btn--outline btn--icon-only"
            aria-label="false"
          >
            <svg
              viewBox="0 0 24 24"
              xmlns="http://www.w3.org/2000/svg"
              class="icon icon--base icon--ArrowRight btn__icon"
            >
              <path
                xmlns="http://www.w3.org/2000/svg"
                d="m16.444 19.204 4.066-7.044-4.066-7.044-.65.375 3.633 6.294h-15.187v.75h15.187l-3.633 6.294z"
              />
            </svg>
            <span class="btn__text">{{ t('message.handbook.read_more') }}</span>
          </NuxtLinkLocale>
        </template>
      </OdsCard>
    </OdsSearchResults>
  </OdsPage>
</template>
