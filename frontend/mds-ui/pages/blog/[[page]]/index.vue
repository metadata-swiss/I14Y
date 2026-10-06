<script setup>
import OdsPage from '../../../app/components/OdsPage.vue'
import { homePageBreadcrumb } from '../../../app/composables/breadcrumbs.js'
import OdsBreadcrumbs from '../../../app/components/OdsBreadcrumbs.vue'
import OdsBlogPostCard from '../../../app/components/blog/OdsBlogPostCard.vue'
import OdsPagination from '../../../app/components/OdsPagination.vue'
import OdsSearchPanel from '../../../app/components/OdsSearchPanel.vue'
import { useRouter } from 'vue-router'

const route = useRoute()
const { locale, t } = useI18n()
const localePath = useLocalePath()

const PAGE_SIZE = 1 // Number of posts per page
const page = parseInt(route.params.page) || 1

const { data: posts } = await useAsyncData(route.path, () => {
  return queryPublishedContent('blog')
    .where('path', 'LIKE', `%.${locale.value}`)
    .order('date', 'DESC')
    .skip((page - 1) * PAGE_SIZE)
    .limit(PAGE_SIZE)
    .all()
})
const { data: pageCount } = await useAsyncData(route.path + '_pageCount', () => {
  return queryPublishedContent('blog')
    .where('path', 'LIKE', `%.${locale.value}`)
    .order('date', 'DESC')
    .count()
})

useSeoMeta({
  title: `${t('message.header.navigation.blog')} | opendata.swiss`,
})

const breadcrumbs = [
  await homePageBreadcrumb(locale),
  {
    title: t('message.header.navigation.blog'),
    path: '/blog',
  },
]

const searchInput = ref('')
const router = useRouter()
const onSearch = (value) => {
  router.push(localePath({
    path: '/blog/search',
    query: { q: value.trim() },
  }))
}
</script>

<template>
  <OdsPage>
    <template #header>
      <OdsBreadcrumbs :breadcrumbs="breadcrumbs" />
    </template>
    <OdsSearchPanel
      v-model:search-input="searchInput"
      small
      :search-prompt="t('message.blog.search_prompt')"
      @search="onSearch"
    />
    <section class="section section--default">
      <div class="container gap--responsive">
        <div
          class="search-results search-results--grid"
          aria-live="polite"
          aria-busy="false"
        >
          <ul class="search-results-list">
            <li
              v-for="post in posts"
              :key="post.id"
            >
              <OdsBlogPostCard :post="post" />
            </li>
          </ul>
        </div>
      </div>
      <div class="container">
        <OdsPagination
          :current-page="page"
          :page-label="t('message.ods-pagination.page')"
          :total-pages="parseInt(pageCount)"
          :pagination-items="[
            {
              icon: 'ChevronLeft',
              label: t('message.ods-pagination.previous'),
              link: `/blog/${page > 1 ? parseInt(page) - 1 : 1}`,
            },
            {
              icon: 'ChevronRight',
              label: t('message.ods-pagination.next'),
              link: `/blog/${page < pageCount ? parseInt(page) + 1 : pageCount}`,
            },
          ]"
          @page-change="navigateTo(localePath({ name: 'blog-page', params: { page: $event } }))"
        />
      </div>
    </section>
  </OdsPage>
</template>
