<script lang="ts" setup>
import OdsBreadcrumbs from '../app/components/OdsBreadcrumbs.vue'
import { useBreadcrumbs } from '../app/composables/breadcrumbs'
import { loadPageBreadcrumb } from '../app/lib/breadcrumbs'
import OdsPage from '../app/components/OdsPage.vue'

const route = useRoute()
const router = useRouter()
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

onMounted(() => {
  if (!page.value) {
    router.replace('/404')
  }
})

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
