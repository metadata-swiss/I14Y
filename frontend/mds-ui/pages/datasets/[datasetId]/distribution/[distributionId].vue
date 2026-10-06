<script setup lang="ts">
import { useI18n } from '#imports'

import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useDatasetsSearch } from '../../../../app/piveau/datasets'
import { homePageBreadcrumb } from '../../../../app/composables/breadcrumbs.js'
import OdsDetailsTable from '../../../../app/components/dataset-detail/OdsDetailsTable.vue'
import OdsPreview from '../../../../app/components/dataset-detail/OdsPreview.vue'
import OdsBreadcrumbs from '../../../../app/components/OdsBreadcrumbs.vue'
import OdsButton from '../../../../app/components/OdsButton.vue'
import OdsDownloadList from '../../../../app/components/distribution/OdsDownloadList.vue'
import OdsDistributionDetailHeader from '../../../../app/components/distribution/OdsDistributionDetailHeader.vue'
import OdsHero from '../../../../app/components/OdsHero.vue'
import { DcatApChV2DatasetAdapter } from '../../../../app/components/dataset-detail/model/dcat-ap-ch-v2-dataset-adapter'
import { useSeoMeta } from 'nuxt/app'
import { getDatasetBreadcrumbFromSessionStorage } from '../breadcrumb-session-stoage'
import OdsTagList from '../../../../app/components/dataset-detail/OdsTagList.vue'
import OdsItemKind from '../../../../app/components/dataset-detail/OdsItemKind.vue'

const { locale, t } = useI18n()

const localePath = useLocalePath()

const route = useRoute()
const router = useRouter()
const datasetId = route.params.datasetId as string
const distributionId = route.params.distributionId as string

const { useResource } = useDatasetsSearch()
const { query, isSuccess, resultEnhanced } = useResource(datasetId)
const { suspense } = query

const SUPPORTED_PREVIEW_FORMATS = ['csv', 'tsv', 'ods', 'xlsx', 'xls'] as const
type SupportedPreviewFormat = typeof SUPPORTED_PREVIEW_FORMATS[number]

const FORMAT_ALIASES: Record<string, SupportedPreviewFormat> = {
  'excel xlsx': 'xlsx',
  'excel xls': 'xls',
  'text/csv': 'csv',
  'text/tab-separated-values': 'tsv',
  'application/vnd.oasis.opendocument.spreadsheet': 'ods',
  'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': 'xlsx',
  'application/vnd.ms-excel': 'xls',
}

function normalizePreviewFormat(format: string): SupportedPreviewFormat | '' {
  const normalized = format.trim().toLowerCase()
  if (!normalized) {
    return ''
  }

  const formatParts = normalized.split(/[/#]/)
  const lastPart = formatParts[formatParts.length - 1] || normalized
  const alias = FORMAT_ALIASES[normalized] ?? FORMAT_ALIASES[lastPart]

  if (alias) {
    return alias
  }

  return SUPPORTED_PREVIEW_FORMATS.includes(lastPart as SupportedPreviewFormat)
    ? lastPart as SupportedPreviewFormat
    : ''
}

const dataset = computed(() => {
  if (!resultEnhanced.value) {
    return undefined
  }
  return new DcatApChV2DatasetAdapter(resultEnhanced.value)
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

const previewUrl = computed(() => {
  if (!distribution.value) {
    return ''
  }
  return distribution.value.downloadUrls[0] || distribution.value.accessUrls[0] || ''
})

const previewFormat = computed(() => {
  return normalizePreviewFormat(distribution.value?.format || '')
})

const isPreviewVisible = computed(() => {
  return Boolean(previewUrl.value && previewFormat.value)
})

const firstBreadcrumb = await homePageBreadcrumb(locale)

const breadcrumbs = computed(() => {
  const bc = []
  const storedBreadcrumbs = import.meta.client ? getDatasetBreadcrumbFromSessionStorage(datasetId) : null
  if (storedBreadcrumbs && import.meta.client) {
    if (storedBreadcrumbs.length === 4) {
      storedBreadcrumbs[1].title = t('message.header.navigation.datasets')
      storedBreadcrumbs[2].title = t('message.dataset_search.search_results')
      storedBreadcrumbs[3].title = dataset.value?.title ?? ''
    }
    else if (storedBreadcrumbs.length === 3) {
      storedBreadcrumbs[1].title = t('message.header.navigation.datasets')
      storedBreadcrumbs[2].title = dataset.value?.title ?? ''
    }
    bc.push(...storedBreadcrumbs)
    bc.push({
      title: distribution.value?.title,
    })
  }
  else {
    bc.push(firstBreadcrumb)
    bc.push({
      title: t('message.header.navigation.datasets'),
      path: '/datasets',
    },
    {
      title: resultEnhanced.value?.getTitle,
      path: {
        name: 'datasets-datasetId',
        params: { datasetId: datasetId },
      },
    },
    {
      title: distribution.value?.title,
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
</script>

<template>
  <main
    v-if="isSuccess && distribution && dataset"
    id="main-content"
  >
    <header id="main-header">
      <ClientOnly>
        <OdsBreadcrumbs :breadcrumbs="breadcrumbs" />
      </ClientOnly>
    </header>
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
          <a
            v-if="dataset.publisher.resource"
            class="link author__name link--external"
            target="_blank"
            :href="dataset.publisher.resource"
          >{{ dataset.publisher.name }}</a>
          <div v-else>
            {{ dataset.publisher.name }}
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
    <!------------- start ------------------>
    <!-- Preview of Tabular Distributions -->
    <!-- supported formats: CSV, XLSX, XLS, ODT, TSV -->
    <div
      v-if="isPreviewVisible"
      class="box"
    >
      <OdsPreview
        :download-url="previewUrl"
        :file-format="previewFormat"
        :title="distribution.title"
      />
    </div>
    <!------------- end --------------->
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
