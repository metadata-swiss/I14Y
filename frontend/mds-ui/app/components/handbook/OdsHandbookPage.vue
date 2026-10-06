<template>
  <OdsPage
    v-if="page"
    :comments-id="`handbook-${page.id}`"
    :page="page"
  >
    <template #header>
      <OdsBreadcrumbs :breadcrumbs="breadcrumbs" />
    </template>
    <template #before-aside-content>
      <OdsSearchPanel
        v-model:search-input="searchInput"
        aside
        :title="t('message.handbook.search_prompt')"
        :search-prompt="t('message.handbook.search_prompt_short')"
        @search="onSearch"
      />
      <OdsCard title="Handbook">
        <OdsAccordion id="handbook">
          <OdsAccordionItem
            v-for="section in sections"
            :id="section.id"
            :key="section.id"
            :title="section.title"
            :open="isSectionOpen(section)"
          >
            <NuxtLinkLocale
              :to="getArticleUrl(section)"
              :class="['menu__item', 'menu__item--border', 'menu__item--condensed', { 'menu__item--active': page.id === section.id }]"
              style="margin-top: unset"
            >
              <div>{{ section.title }}</div>
            </NuxtLinkLocale>
            <NuxtLinkLocale
              v-for="article in getArticlesByParent(section)"
              :key="article.id"
              :to="getArticleUrl(article)"
              :class="['menu__item', 'menu__item--border', 'menu__item--condensed', { 'menu__item--active': page.id === article.id }]"
              style="margin-top: unset"
            >
              <div>{{ article.title }}</div>
            </NuxtLinkLocale>
          </OdsAccordionItem>
        </OdsAccordion>
      </OdsCard>
    </template>
  </OdsPage>
</template>

<script setup lang="ts">
import type { HandbookCollectionItem, PagesCollectionItem } from '@nuxt/content'
import OdsPage from '~/components/OdsPage.vue'
import OdsBreadcrumbs, { type BreadcrumbItem } from '~/components/OdsBreadcrumbs.vue'
import OdsSearchPanel from '~/components/OdsSearchPanel.vue'
import { useRouter } from '#vue-router'
import OdsCard from '../content/OdsCard.vue'
import OdsAccordion from '~/components/OdsAccordion.vue'
import OdsAccordionItem from '~/components/OdsAccordionItem.vue'
import { queryHandbook, useGetArticleUrl } from '~/composables/handbook'
import { sortContent } from '~/lib/sortContent'

const { t, locale } = useI18n()

const { page } = defineProps<{
  page: HandbookCollectionItem | PagesCollectionItem
  breadcrumbs: BreadcrumbItem[]
}>()

const searchInput = ref('')

const getArticleUrl = await useGetArticleUrl()

const router = useRouter()
const localePath = useLocalePath()
const onSearch = (value: string) => {
  router.push(localePath({
    path: '/handbook/search',
    query: { q: value.trim() },
  }))
}

const { data: articles } = await useAsyncData('handbook-articles', () => queryHandbook().all())

const localizedArticles = computed(() => articles.value?.filter(article => article.path.endsWith(`.${locale.value}`)) || [])

const getSlug = (article: HandbookCollectionItem) => article.slug

const sections = computed(() => {
  const sections = localizedArticles.value?.filter(article => !article.parent) || []

  return sortContent(sections, getSlug)
})

function getArticlesByParent(parent: HandbookCollectionItem) {
  const articles = localizedArticles.value
    ?.filter(article => article.parent && (
      parent.path.endsWith(`handbook/${article.parent}.${locale.value}`)
      || parent.path.endsWith(`handbook/${article.parent}.de.md`)
    ))

  return sortContent(articles, getSlug)
}

function isSectionOpen(section: HandbookCollectionItem) {
  if ('parent' in page) {
    if (page.parent) {
      let current: HandbookCollectionItem | undefined = page as HandbookCollectionItem

      while (current) {
        if (current.path === section.path) {
          return true
        }

        const currentParent: string | undefined = current.parent
        current = articles.value?.find((article) => {
          if (!currentParent) return false
          return article.path.endsWith(`handbook/${currentParent}.${locale.value}`)
            || (locale.value !== 'de' && article.path.endsWith(`handbook/${currentParent}.de.md`))
        })
      }

      return false
    }
    else {
      return page.id === section.id
    }
  }

  return true
}
</script>
