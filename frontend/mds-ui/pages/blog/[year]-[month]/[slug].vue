<script setup>
import OdsPage from '../../../app/components/OdsPage.vue'
import { homePageBreadcrumb } from '../../../app/composables/breadcrumbs'
import OdsBreadcrumbs from '../../../app/components/OdsBreadcrumbs.vue'

const route = useRoute()
const { locale, t } = useI18n()
const { year, month, slug } = route.params

const { data: post } = await useAsyncData(route.path, () => {
  return queryPublishedContent('blog')
    .where('path', 'LIKE', `%${slug}.${locale.value}`)
    .where('date', '>', `${year}-${month.padStart(2, '0')}-01`)
    .where('date', '<', `${year}-${month.padStart(2, '0')}-32`)
    .orWhere(q =>
      q.where('slug', '=', slug)
        .where('path', 'LIKE', `%${slug}.${locale.value}`),
    )
    .first()
})

useSeoMeta({
  title: `${post.value?.title} | ${t('message.header.navigation.blog')} | opendata.swiss`,
})

const breadcrumbs = [
  await homePageBreadcrumb(locale),
  {
    title: t('message.header.navigation.blog'),
    path: '/blog',
  },
  {
    title: post.value?.title || slug,
  },
]
</script>

<template>
  <OdsPage
    v-if="post"
    :comments-id="`blog-${post.id}`"
    :page="post"
  >
    <template #header>
      <OdsBreadcrumbs :breadcrumbs="breadcrumbs" />
    </template>
  </OdsPage>
</template>
