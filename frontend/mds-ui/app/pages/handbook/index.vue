<script lang="ts" setup>
import { useBreadcrumbs } from '~/composables/breadcrumbs'
import { loadPageBreadcrumb } from '~/utils/breadcrumbs'
import OdsHandbookPage from '~/components/handbook/OdsHandbookPage.vue'

const route = useRoute()
const { locale } = useI18n()

const breadcrumbs = await useBreadcrumbs({
  route,
  locale,
  loadContent: loadPageBreadcrumb(locale),
})

const { data: page } = await useAsyncData(route.path, () => {
  return queryPublishedContent('pages')
    .where('path', 'LIKE', `%handbook.${locale.value}`)
    .first()
})

useSeoMeta({
  title: `${page.value?.title} | opendata.swiss`,
})
</script>

<template>
  <OdsHandbookPage
    v-if="page"
    :page="page"
    :breadcrumbs="breadcrumbs"
  />
</template>
