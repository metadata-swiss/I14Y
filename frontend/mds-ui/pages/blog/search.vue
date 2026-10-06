<script setup lang="ts">
import { homePageBreadcrumb } from '../../app/composables/breadcrumbs.js'
import { useAsyncData, useSeoMeta } from 'nuxt/app'
import OdsCmsSearchPage from '../../app/components/OdsCmsSearchPage.vue'
import type { SearchResult } from 'minisearch'

const { t, locale } = useI18n()

const currentLanguage = new RegExp(`\\.${locale.value}(#.+)?$`)
const { data: { value: [sections, blog] }, error } = await useAsyncData('blog-search', async () => {
  let sections = await queryCollectionSearchSections('blog')
  sections = (sections || []).filter(section => currentLanguage.test(section.id))

  const blog = await queryPublishedContent('blog')
    .where('path', 'IN', sections.map(section => section.id))
    .all()

  return [sections, blog]
})

const searchResultToLink = (result: SearchResult) => {
  const [id, hash] = result.id.split('#')

  let path
  const slug = `${id.substring(0, id.lastIndexOf('.'))}`.split('/').pop()
  const article = blog.find(({ path }) => result.id.startsWith(path))
  if (article) {
    path = '/' + ['blog', article.date.substring(0, 7), slug].join('/')
  }
  return {
    path,
    hash: hash ? `#${hash}` : null,
  }
}

const searchResultToImage = (result: SearchResult) => {
  const article = blog.find(({ path }) => result.id.startsWith(path))
  return article?.image || ''
}

if (error.value) {
  console.error('Failed to fetch blog search sections', error.value)
}

const breadcrumbs = [
  await homePageBreadcrumb(locale),
  {
    title: t('message.header.navigation.blog'),
    path: '/blog',
  },
  {
    title: t('message.header.navigation.search'),
  },
]

useSeoMeta({
  title: `${t('message.header.navigation.search')} | ${t('message.header.navigation.blog')} | opendata.swiss`,
})
</script>

<template>
  <OdsCmsSearchPage
    :search-prompt="t('message.blog.search_prompt')"
    :breadcrumbs="breadcrumbs"
    :data="sections"
    :search-result-to-link="searchResultToLink"
    :search-result-to-image="searchResultToImage"
  />
</template>
