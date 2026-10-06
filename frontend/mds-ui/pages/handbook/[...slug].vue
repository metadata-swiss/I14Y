<script lang="ts" setup>
import { loadHandbookBreadcrumb } from '~/lib/breadcrumbs'
import OdsHandbookPage from '../../app/components/handbook/OdsHandbookPage.vue'
import { queryHandbook } from '../../app/composables/handbook'

const { locale, t } = useI18n()
const route = useRoute()

const breadcrumbs = await useBreadcrumbs({
  route,
  locale,
  loadContent: loadHandbookBreadcrumb(locale),
})

const { data } = await useAsyncData(route.path, async () => {
  const path = [...route.params.slug]

  const articles = await queryHandbook().all()

  const localizedArticles = articles.filter(article => article.path.endsWith(`.${locale.value}`))

  // Find the article whose slug matches the last part of the path
  const leaf = localizedArticles.find(article => article.slug === path[path.length - 1])

  if (!leaf) {
    return undefined
  }

  // verify parent chain by building the path from the leaf upwards
  function getPathSegments(article: typeof leaf): string[] {
    const segments = [article.slug]
    let current = article
    while (current.parent) {
      // Find parent in the current locale
      let parent = articles.find(a => a.path.endsWith(`handbook/${current.parent}.${locale.value}`))

      // Fallback to German version of parent if localized version is missing
      if (!parent && locale.value !== 'de') {
        parent = articles.find(a => a.path.endsWith(`handbook/${current.parent}.de.md`))
      }

      if (!parent) break
      segments.unshift(parent.slug)
      current = parent
    }
    return segments
  }

  const actualPath = getPathSegments(leaf)
  if (actualPath.join('/') === path.join('/')) {
    return leaf
  }

  return undefined
})

useSeoMeta({
  title: `${data.value?.title} | ${t('message.header.navigation.handbook')} | opendata.swiss`,
})

// TODO: make sidebar navigation dynamic based on the handbook content
const _navigation = ref([
  {
    title: 'Guide',
    icon: 'i-lucide-book-open',
    path: '#getting-started',
    children: [
      {
        title: 'Introduction',
        path: '#introduction',
        active: true,
      },
      {
        title: 'Installation',
        path: '#installation',
      },
    ],
  },
  {
    title: 'Composables',
    icon: 'i-lucide-database',
    path: '#composables',
    children: [
      {
        title: 'defineShortcuts',
        path: '#defineshortcuts',
      },
      {
        title: 'useModal',
        path: '#usemodal',
      },
    ],
  },
])
</script>

<template>
  <OdsHandbookPage
    :page="data"
    :comments-id="`handbook-${data.id}`"
    :breadcrumbs="breadcrumbs"
  />
</template>
