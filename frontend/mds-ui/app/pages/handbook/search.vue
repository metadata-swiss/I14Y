<script setup lang="ts">
import { homePageBreadcrumb } from '~/composables/breadcrumbs.js'
import { useAsyncData, useSeoMeta } from 'nuxt/app'
import OdsCmsSearchPage from '~/components/OdsCmsSearchPage.vue'
import { queryHandbook, useGetArticleUrl } from '~/composables/handbook.js'
import type { HandbookCollectionItem } from '@nuxt/content'
import type { SearchResult } from 'minisearch'

const { t, locale } = useI18n()

const getArticleUrl = await useGetArticleUrl()

const { data: pages } = await useAsyncData('handbook-search-pages', async () => {
  return queryHandbook()
    .select('path', 'title', 'parent', 'slug')
    .where('path', 'LIKE', `%.${locale.value}%`)
    .all()
})

const currentLanguage = new RegExp(`\\.${locale.value}(#.+)?$`)
const { data, error } = await useAsyncData('handbook-search', async () => {
  const sections = await queryCollectionSearchSections('handbook')

  return (sections || [])
    .filter(section => currentLanguage.test(section.id))
})

const searchResultToLink = (pages: Pick<HandbookCollectionItem, 'path' | 'slug' | 'parent'>[]) => (result: SearchResult) => {
  const [id, hash] = result.id.split('#')

  let path
  const page = pages.find(({ path }) => result.id.startsWith(path))
  if (page) {
    path = getArticleUrl(page)
  }
  else {
    path = id.substring(0, id.lastIndexOf('.'))
  }
  return {
    path,
    hash: hash ? `#${hash}` : undefined,
  }
}

if (error.value) {
  console.error('Failed to fetch handbook search sections', error.value)
}

const breadcrumbs = [
  await homePageBreadcrumb(locale),
  {
    title: t('message.header.navigation.handbook'),
    path: '/handbook',
  },
  {
    title: t('message.header.navigation.search'),
  },
]

useSeoMeta({
  title: `${t('message.header.navigation.search')} | ${t('message.header.navigation.handbook')} | opendata.swiss`,
})
</script>

<template>
  <OdsCmsSearchPage
    :search-prompt="t('message.handbook.search_prompt')"
    :breadcrumbs="breadcrumbs"
    :data="data ?? []"
    :search-result-to-link="searchResultToLink(pages ?? [])"
  />
</template>
