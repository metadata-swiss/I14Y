<script setup lang="ts">
import OdsSection, { type SectionLayout } from '~/components/content/OdsSection.vue'
import { useAsyncData } from '#app'
import OdsBlogPostCard from '../blog/OdsBlogPostCard.vue'

const { max: limit } = defineProps<{
  title: string
  max: number
  layout: SectionLayout
}>()

const { locale } = useI18n()

const { data: pinnedPosts } = await useAsyncData('blog-pinned', async () => {
  return queryPublishedContent('blog')
    .where('path', 'LIKE', `%.${locale.value}`)
    .where('pinned', '=', true)
    .limit(limit)
    .all()
})

const { data: latestPosts } = await useAsyncData('blog-latest', async () => {
  let query = queryPublishedContent('blog')
    .where('path', 'LIKE', `%.${locale.value}`)
    .order('date', 'DESC')

  if (pinnedPosts.value?.length) {
    query = query.andWhere((query) => {
      return pinnedPosts.value!.reduce((acc, pinned) => acc.where('id', '<>', pinned.id), query)
    })
  }

  return query.limit(limit)
    .all()
})
</script>

<template>
  <OdsSection
    id="latest-news"
    :layout="layout"
    accent-color="100"
    text-color="600"
    :title="title"
  >
    <OdsBlogPostCard
      v-for="post in pinnedPosts"
      :key="post.id"
      :post="post"
    />
    <OdsBlogPostCard
      v-for="post in latestPosts"
      :key="post.id"
      :post="post"
    />
  </OdsSection>
</template>
