<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from '#imports'

import { Comments } from '@hyvor/hyvor-talk-vue'

import { useDatasetsSearch } from '../../../app/piveau/datasets'
import { DcatApChV2DatasetAdapter } from '../../../app/components/dataset-detail/model/dcat-ap-ch-v2-dataset-adapter'

import { homePageBreadcrumb } from '../../../app/composables/breadcrumbs'
import OdsBreadcrumbs, { type BreadcrumbItem } from '../../../app/components/OdsBreadcrumbs.vue'
import OdsDetailsTable from '../../../app/components/dataset-detail/OdsDetailsTable.vue'
import OdsTagList from '../../../app/components/dataset-detail/OdsTagList.vue'
import OdsDistributionList from '../../../app/components/dataset-detail/OdsDistributionList.vue'
import OdsButton from '../../../app/components/OdsButton.vue'
import OdsDatasetDetailHeader from '../../../app/components/dataset-detail/OdsDatasetDetailHeader.vue'
import OdsMetadataDownload from '../../../app/components/dataset-detail/OdsMetadataDownload.vue'
import Hero from '../../../app/components/OdsHero.vue'
import { useRuntimeConfig, useSeoMeta } from 'nuxt/app'
import { getDatasetBreadcrumbFromSessionStorage, storeDatasetBreadcrumbInSessionStorage } from './breadcrumb-session-stoage'
import type { TagItem } from '../../../app/components/OdsTagItem.vue'
import OdsItemKind from '../../../app/components/dataset-detail/OdsItemKind.vue'
import { getTranslationFor } from '@piveau/sdk-vue'

const { locale, t } = useI18n()
const route = useRoute()
const router = useRouter()
const datasetId = route.params.datasetId as string

const { useResource } = useDatasetsSearch()
const { query, isSuccess, resultEnhanced } = useResource(datasetId)

const { suspense } = query

const localePath = useLocalePath()

const dataset = computed(() => {
  if (!resultEnhanced.value) {
    return undefined
  }
  return new DcatApChV2DatasetAdapter(resultEnhanced.value)
})

const distributions = computed(() => (dataset.value?.distributions ?? []).sort((a, b) => a.title.localeCompare(b.title)))

const searchBreadcrumb = ref<BreadcrumbItem | null>(null)

const { comments: { websiteId } } = useRuntimeConfig().public

const homePage = await homePageBreadcrumb(locale)
const breadcrumbs = computed(() => {
  if (import.meta.client) {
    const storedBreadcrumbs = getDatasetBreadcrumbFromSessionStorage(datasetId)
    if (storedBreadcrumbs) {
      storedBreadcrumbs[0].title = t('message.header.navigation.datasets')
      if (storedBreadcrumbs.length === 4) {
        storedBreadcrumbs[1].title = t('message.header.navigation.datasets')
        storedBreadcrumbs[2].title = t('message.dataset_search.search_results')
        storedBreadcrumbs[3].title = dataset.value?.title ?? ''
      }
      else if (storedBreadcrumbs.length === 3) {
        storedBreadcrumbs[1].title = t('message.header.navigation.datasets')
        storedBreadcrumbs[2].title = dataset.value?.title ?? ''
      }
      storeDatasetBreadcrumbInSessionStorage(datasetId, storedBreadcrumbs)
      return storedBreadcrumbs
    }
  }

  const result = [
    homePage,
    {
      title: t('message.header.navigation.datasets'),
      path: '/datasets',
    },
  ]

  if (searchBreadcrumb.value) {
    result.push(searchBreadcrumb.value)
  }

  result.push({
    title: resultEnhanced.value?.getTitle,
    path: {
      name: 'datasets-datasetId',
      params: { datasetId: datasetId },
    },
  })

  if (import.meta.client) {
    storeDatasetBreadcrumbInSessionStorage(datasetId, result)
  }

  return result
})

useSeoMeta({
  title: () => `${dataset.value?.title} | ${t('message.header.navigation.datasets')} | opendata.swiss`,
})

watch(() => route.query.search,
  () => {
    if (import.meta.client) {
      const { search, ...rest } = route.query
      if (search) {
        router.replace({ query: rest })
        if (typeof search === 'string') {
          searchBreadcrumb.value = {
            id: 'search',
            title: t('message.dataset_search.search_results'),
            route: {
              path: '/datasets',
              query: Object.fromEntries(new URLSearchParams(decodeURIComponent(search))),
            },
          }
        }
      }
    }
  },
  { immediate: true },
)

const toDatasetSearchRoute = computed(() => {
  const currentBreadcrumbs = breadcrumbs.value
  if (!currentBreadcrumbs) {
    return { path: '/datasets' }
  }
  const breadcrumbWithSearch = currentBreadcrumbs[currentBreadcrumbs.length - 2]
  if (breadcrumbWithSearch.route) {
    return breadcrumbWithSearch.route
  }
  return { path: breadcrumbWithSearch.path, query: {} }
})

const toDatasetSearchHref = computed(() => localePath(toDatasetSearchRoute.value))

function goToDatsetSearch() {
  console.log('got to ', toDatasetSearchHref)
  router.push(toDatasetSearchHref.value)
}

function setTagAndGotToDatasetSearch(tag: TagItem) {
  console.log(tag)
  console.log(toDatasetSearchHref.value)
  console.log(toDatasetSearchRoute.value)
}

await suspense()
</script>

<template>
  <div v-if="isSuccess && dataset">
    <header id="main-header">
      <ClientOnly>
        <OdsBreadcrumbs :breadcrumbs="breadcrumbs" />
      </ClientOnly>
    </header>
    <main id="main-content">
      <section class="section section--default bg--secondary-50">
        <div class="container">
          <div class="first-header">
            <OdsItemKind :kind="t('message.dataset_detail.dataset')" />
            <form
              method="post"
              action="/api/subscribe/dataset"
              style="display: inline-block;"
              class="subscribe-form"
            >
              <input
                type="hidden"
                name="dataset"
                :value="dataset.id"
              >
              <OdsButton
                type="submit"
                class="btn"
                :title="t(`message.subscribe.header`)"
                icon="Plus"
                variant="outline-negative"
              />
            </form>
          </div>
          <OdsDatasetDetailHeader :dataset="dataset" />
        </div>
      </section>

      <Hero
        type="default"
      >
        <template #title>
          {{ dataset.title }}
        </template>
        <template #description>
          <MDC :value="dataset.description" />
          <div
            v-if="dataset.keywords.length > 0"
            class="keywords"
          >
            <OdsTagList
              :tags="dataset.keywords"
              @tag-clicked="setTagAndGotToDatasetSearch"
            />
          </div>
        </template>
        <template #authors>
          <div
            v-if="dataset.publisher && dataset.publisher.name"
            class="disc-images"
            aria-hidden="true"
          >
            <div class="disc-image">
              <img
                src="https://picsum.photos/120/120/?image=29"
                :title="dataset.publisher.name"
              >
            </div>
          </div>
          <address
            v-if="dataset.publisher && dataset.publisher.name"
            class="authors__names"
          >
            <NuxtLinkLocale
              v-if="dataset.publisher.id"
              :to="{ name: 'organizations-id', params: { id: dataset.publisher.id } }"
            >
              {{ getTranslationFor(dataset.publisher.name, [locale]) }}
            </NuxtLinkLocale>
            <a
              v-else-if="dataset.publisher.homepage"
              class="link author__name link--external"
              target="_blank"
              :href="dataset.publisher.homepage"
            >{{ getTranslationFor(dataset.publisher.name, [locale]) }}</a>
            <span v-else>
              {{ getTranslationFor(dataset.publisher.name, [locale]) }}
            </span>
          </address>
        </template>
      </Hero>
      <section class="section">
        <div class="container container--grid gap--responsive">
          <div class="container__main vertical-spacing">
            <h2 class="h2">
              {{ t('message.dataset_detail.distributions') }}
            </h2>
            <OdsDistributionList :distributions="distributions" />

            <h2 class="h2">
              {{ t('message.dataset_detail.additional_information') }}
            </h2>
            <OdsDetailsTable
              :table-entries="dataset.propertyTable"
              type="block"
            />
            <OdsMetadataDownload :dataset="dataset" />
          </div>
          <div class="hidden container__aside md:block">
            <div
              id="aside-content"
              class="sticky sticky--top"
            >
              <div class="box">
                <h2 class="h5">
                  {{ t(`message.subscribe.header`) }}
                </h2>
                <form
                  method="post"
                  action="/api/subscribe/dataset"
                  style="display: inline-block;"
                  class="subscribe-form"
                >
                  <input
                    type="hidden"
                    name="dataset"
                    :value="dataset.id"
                  >
                  <input
                    type="submit"
                    class="btn btn--outline"
                    :value="t(`message.subscribe.to_dataset`)"
                  >
                </form>
                <form
                  v-for="category in dataset.getCategoriesForLanguage(locale)"
                  :key="category.id"
                  method="post"
                  action="/api/subscribe/category"
                  style="display: inline-block;"
                  class="subscribe-form"
                >
                  <input
                    type="hidden"
                    name="category"
                    :value="category.id"
                  >
                  <input
                    type="submit"
                    class="btn btn--outline"
                    :value="`${t(`message.subscribe.to_category`)} ${category.label}`"
                  >
                </form>
              </div>
            </div>
          </div>
        </div>

        <div class="container">
          <Comments
            :website-id="websiteId"
            :page-id="`dataset-${dataset.id}`"
            :page-language="locale"
          />
        </div>
      </section>

      <section class="section publication-back-button-section">
        <div class="container">
          <OdsButton
            :title="t('message.dataset_detail.to_search')"
            icon="ArrowLeft"
            variant="outline"
            class="btn--back"
            size="sm"
            @click="goToDatsetSearch()"
          />
        </div>
      </section>
    </main>
  </div>
</template>

<style lang="scss" scoped>
#main-header {
  /* avoid layout shift from ssr to csr */
  @media (min-width: 1024px) {
    min-height: 65.5px;
  }
  @media (min-width: 1280px) {
    min-height: 73.5px;
  }
}

.subscribe-form input[type=submit] {
  box-shadow: none;
  cursor: pointer;
}

.subscribe-form input[type=submit]:hover {
  text-decoration: underline;
}

form.subscribe-form:not(:first-of-type) {
  margin-top: 1rem;
}

.keywords {
  margin-top: 40px;
}

.first-header {
  display: flex;
  flex-direction: column;
  justify-content: flex-start;
  align-items: flex-start;
  margin-bottom: 32px;

  @media (min-width: 600px) {
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    margin-bottom: 32px;
  }
}
</style>
