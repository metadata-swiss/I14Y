<script setup lang="ts">
import { useI18n } from '#imports'

import { computed, defineAsyncComponent } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useGetDatasetsByIdentifier } from '~~/api-client/generated/iop-core'
import { localize } from '~/utils/getCurrentTranslation'
import type { AppLanguage } from '~/constants/langages'
import { homePageBreadcrumb } from '~/composables/breadcrumbs.js'
import OdsDetailsTable from '~/components/dataset-detail/OdsDetailsTable.vue'
import OdsBreadcrumbs, { type BreadcrumbItem } from '~/components/OdsBreadcrumbs.vue'
import OdsButton from '~/components/OdsButton.vue'
import OdsDownloadList from '~/components/distribution/OdsDownloadList.vue'
import OdsDistributionDetailHeader from '~/components/distribution/OdsDistributionDetailHeader.vue'
import OdsHero from '~/components/OdsHero.vue'
import { DcatApChV2DatasetAdapter } from '~/model/dataset/dcat-ap-ch-v2-dataset-adapter'
import { useSeoMeta } from 'nuxt/app'
import { getDatasetBreadcrumbFromSessionStorage, translateDatasetBreadcrumbs } from '~/utils/breadcrumb-session-storage'
import OdsTagList from '~/components/dataset-detail/OdsTagList.vue'
import OdsItemKind from '~/components/dataset-detail/OdsItemKind.vue'
import { ensureFound } from '~/utils/ensureFound'

// Loaded only on this page and only when a preview is shown: it brings the table and chart libraries.
const OdsPreview = defineAsyncComponent(() => import('~/components/dataset-detail/OdsPreview.vue'))

const { locale, t } = useI18n()

const localePath = useLocalePath()

const route = useRoute()
const router = useRouter()
const datasetId = route.params.datasetId as string
const distributionId = route.params.distributionId as string

const query = useGetDatasetsByIdentifier(datasetId)
const { isSuccess, data: datasetModel } = query
const { suspense } = query

const dataset = computed(() => {
  if (!datasetModel.value) {
    return undefined
  }
  return new DcatApChV2DatasetAdapter(datasetModel.value, locale.value as AppLanguage, t)
})

const distribution = computed(() => {
  if (!dataset.value) {
    return undefined
  }
  const dists = dataset.value.distributions.find(d => d.id === distributionId) ?? undefined
  return dists
})

const hasDownloadUrl = computed(() => {
  const dist = distribution.value

  if (!dist) {
    // we are not ready
    return false
  }
  const downloadUrls = dist.downloadUrls
  if (downloadUrls.length > 0) {
    // we have at least one download url
    return true
  }
  // empty array return false
  return false
})

const isPreviewEnabled = Boolean(useRuntimeConfig().public.dataPreviewUrl)

const previewUrl = computed(() => {
  if (!distribution.value) {
    return ''
  }
  return distribution.value.downloadUrls[0] || distribution.value.accessUrls[0] || ''
})

const hasAccessUrl = computed(() => {
  const dist = distribution.value

  if (!dist) {
    // we are not ready
    return false
  }
  const accessUrls = dist.accessUrls
  if (accessUrls.length > 0) {
    // we have at least one accessUrl url
    return true
  }
  // empty array return false
  return false
})

const firstBreadcrumb = await homePageBreadcrumb(locale)

const breadcrumbs = computed(() => {
  const bc: BreadcrumbItem[] = []
  const storedBreadcrumbs = import.meta.client ? getDatasetBreadcrumbFromSessionStorage(datasetId) : null
  if (storedBreadcrumbs && import.meta.client) {
    translateDatasetBreadcrumbs(storedBreadcrumbs, {
      home: firstBreadcrumb.title,
      datasets: t('message.header.navigation.datasets'),
      searchResults: t('message.dataset_search.search_results'),
      dataset: dataset.value?.title ?? '',
    })
    bc.push(...storedBreadcrumbs)
    bc.push({
      title: distribution.value?.title ?? '',
    })
  }
  else {
    bc.push(firstBreadcrumb)
    bc.push({
      title: t('message.header.navigation.datasets'),
      path: '/datasets',
    },
    {
      title: dataset.value?.title ?? '',
      route: {
        name: 'datasets-datasetId',
        params: { datasetId: datasetId },
      },
    },
    {
      title: distribution.value?.title ?? '',
    },
    )
  }
  return bc
})

useSeoMeta({
  title: () => `${distribution.value?.title} | ${dataset.value?.title} | ${t('message.header.navigation.datasets')} | opendata.swiss`,
})

const toDatasetHref = computed(() => localePath('/datasets/' + datasetId))

function gotToDataset() {
  router.push(toDatasetHref.value)
}
await suspense()
ensureFound(datasetModel.value, query.error.value)
ensureFound(distribution.value)
</script>

<template>
  <div v-if="isSuccess && distribution && dataset">
    <header id="main-header">
      <ClientOnly>
        <OdsBreadcrumbs :breadcrumbs="breadcrumbs" />
      </ClientOnly>
    </header>
    <main id="main-content">
      <section class="section section--default bg--secondary-50">
        <div class="container">
          <div class="first-header">
            <OdsItemKind :kind="t('message.dataset_detail.distribution')" />
            <OdsButton
              v-if="hasDownloadUrl"
              :href="distribution.downloadUrls[0]"
              variant="outline-negative"
              :title="t('message.dataset_detail.download') + ' ' + distribution.format "
              icon="Download"
            />
            <OdsButton
              v-if="hasAccessUrl && !hasDownloadUrl"
              :href="distribution.accessUrls[0]"
              variant="outline-negative"
              :title="t('message.dataset_detail.go_to_resource') + ' ' + distribution.format "
              icon="External"
            />
          </div>
          <OdsDistributionDetailHeader :distribution="distribution" />
        </div>
      </section>

      <OdsHero
        type="default"
      >
        <template #title>
          {{ distribution.title }}
        </template>
        <template #description>
          <MDC :value="distribution.description ?? ''" />
          <div
            v-if="dataset.keywords.length > 0"
            class="keywords"
          >
            <OdsTagList
              :tags="dataset.keywords"
            />
          </div>
        </template>
        <template #authors>
          <div
            v-if="localize(dataset.publisher.name, locale)"
            class="disc-images"
            aria-hidden="true"
          >
            <div class="disc-image">
              <img
                src="https://picsum.photos/120/120/?image=29"
                :title="localize(dataset.publisher.name, locale)"
              >
            </div>
          </div>
          <address
            v-if="localize(dataset.publisher.name, locale)"
            class="authors__names"
          >
            <a
              v-if="dataset.publisher.homepage"
              class="link author__name link--external"
              target="_blank"
              :href="dataset.publisher.homepage"
            >{{ localize(dataset.publisher.name, locale) }}</a>
            <div v-else>
              {{ localize(dataset.publisher.name, locale) }}
            </div>
          </address>
        </template>
      </OdsHero>
      <section class="section">
        <div class="container container--grid gap--responsive">
          <div class="container__main vertical-spacing">
            <div class="container__mobile">
              <div
                v-if="hasDownloadUrl"
                class="box"
              >
                <h2 class="h5">
                  {{ t('message.dataset_detail.download') }}
                </h2>
                <OdsDownloadList
                  :urls="distribution.downloadUrls"
                  :name="distribution.title"
                  :format="distribution.format"
                  :languages="distribution.languages"
                  :byte-size="distribution.formattedByteSize"
                  icon="Download"
                />
              </div>
              <div
                v-if="hasAccessUrl"
                class="box"
              >
                <h2 class="h5">
                  Access
                </h2>
                <OdsDownloadList
                  :urls="distribution.accessUrls"
                  :name="distribution.title"
                  :format="distribution.format"
                  :languages="distribution.languages"
                  :byte-size="distribution.formattedByteSize"
                  icon="External"
                />
              </div>
            </div>
            <h2 class="h2">
              {{ t('message.dataset_detail.additional_information') }}
            </h2>
            <OdsDetailsTable
              :table-entries="distribution.propertyTable"
              type="block"
            />
          </div>
          <div class="hidden container__aside md:block">
            <div
              id="aside-content"
              class="sticky sticky--top"
            >
              <div
                v-if="distribution.downloadUrls.length > 0"
                class="box"
              >
                <h2 class="h5">
                  {{ t('message.dataset_detail.download') }}
                </h2>
                <OdsDownloadList
                  :urls="distribution.downloadUrls"
                  :name="distribution.title"
                  :format="distribution.format"
                  :languages="distribution.languages"
                  :byte-size="distribution.formattedByteSize"
                  icon="Download"
                />
              </div>
              <div
                v-if="hasAccessUrl && !hasDownloadUrl"
                class="box"
              >
                <h2 class="h5">
                  Access
                </h2>
                <OdsDownloadList
                  :urls="distribution.accessUrls"
                  :name="distribution.title"
                  :format="distribution.format"
                  :languages="distribution.languages"
                  :byte-size="distribution.formattedByteSize"
                  icon="External"
                />
              </div>
            </div>
          </div>
        </div>
      </section>
      <!-- Preview of tabular distributions (CSV, TSV, XLSX, XLS, ODS), only when a preview service is configured. -->
      <ClientOnly v-if="isPreviewEnabled && previewUrl">
        <OdsPreview
          :download-url="previewUrl"
          :file-format="distribution?.format ?? ''"
          :title="distribution?.title"
        />
      </ClientOnly>
      <section class="section publication-back-button-section">
        <div class="container">
          <OdsButton
            :title="t(`message.dataset_detail.to_dataset`) "
            icon="ArrowLeft"
            variant="outline"
            class="btn--back"
            @click="gotToDataset()"
          />
        </div>
      </section>
    </main>
  </div>
</template>

<style lang="scss" scoped>
#main-header {
  @media (min-width: 1024px) {
    min-height: 65.5px;
  }
  @media (min-width: 1280px) {
    min-height: 73.5px;
  }
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

.keywords {
  margin-top: 40px;
}
</style>
