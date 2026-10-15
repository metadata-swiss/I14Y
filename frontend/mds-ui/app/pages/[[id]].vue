<script lang="ts" setup>
import OdsBreadcrumbs from '~/components/OdsBreadcrumbs.vue'
import { useBreadcrumbs } from '~/composables/breadcrumbs'
import { loadPageBreadcrumb } from '~/utils/breadcrumbs'
import OdsPage from '~/components/OdsPage.vue'
import { ensureFound } from '~/utils/ensureFound'

const route = useRoute()
const { locale } = useI18n()

const breadcrumbs = await useBreadcrumbs({
  route,
  locale,
  loadContent: loadPageBreadcrumb(locale),
})

const { data: page } = await useAsyncData(route.path, () => {
  const slug = route.params.id || 'index'

  return queryPublishedContent('pages')
    .where('stem', 'LIKE', `%${slug}.${locale.value}`)
    .first()
})
ensureFound(page.value)

useSeoMeta({
  title: `${page.value?.title} | opendata.swiss`,
})
</script>

<template>
  <OdsPage
    v-if="page"
    :page="page"
  >
    <template #header>
      <OdsBreadcrumbs :breadcrumbs="breadcrumbs" />
    </template>
  </OdsPage>
</template>
