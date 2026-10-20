<script setup>
import OdsPage from '~/components/OdsPage.vue'
import { homePageBreadcrumb } from '~/composables/breadcrumbs.js'
import OdsInfoBlock from '~/components/OdsInfoBlock.vue'
import OdsTagItem from '~/components/OdsTagItem.vue'
import OdsBreadcrumbs from '~/components/OdsBreadcrumbs.vue'
import OdsCard from '~/components/content/OdsCard.vue'
import OdsButton from '~/components/OdsButton.vue'
import { localize } from '~/utils/getCurrentTranslation'
import { showcaseTypeLabel } from '~/utils/showcase-type'
import { useThemes } from '~/composables/useDatasets'
import { getDatasetsByIdentifier } from '~~/api-client/generated/iop-core'
import { DcatApChV2DatasetAdapter } from '~/model/dataset/dcat-ap-ch-v2-dataset-adapter'
import { ensureFound } from '~/utils/ensureFound'

const route = useRoute()
const { locale, t } = useI18n()
const { id } = route.params

const { data: showcase } = await useAsyncData(route.path, async () => {
  const currentTranslation = await queryPublishedContent('showcases')
    .where('stem', 'LIKE', `%${id}.${locale.value}`)
    .where('active', '=', true)
    .first()

  if (!currentTranslation) {
    return null
  }

  const germanTranslation = await queryPublishedContent('showcases')
    .where('stem', 'LIKE', `%${id}.de`)
    .where('active', '=', true)
    .select('relationships')
    .first()

  return {
    ...currentTranslation,
    contactDetails: germanTranslation?.relationships
      .find(relationship => relationship.type === 'pointOfContact'),
  }
})
ensureFound(showcase.value)

const breadcrumbs = [
  homePageBreadcrumb(t),
  {
    title: t('message.header.navigation.showcases'),
    path: '/showcases',
  },
  {
    title: showcase.value?.title || id,
  },
]

const showcaseType = computed(() => showcaseTypeLabel(showcase.value.type, t))

// The categories are I14Y themes, stored as their codes; their names come from the iop-core vocabulary.
const { query: themesQuery, themes } = useThemes()
await themesQuery.suspense().catch(() => undefined)
const showcaseCategories = computed(() => (showcase.value.themes ?? [])
  .map(code => themes.value.find(theme => theme.code === code))
  .filter(Boolean))

// The datasets are referenced by their URI in the old portal. The last part of it is looked up as identifier in iop-core.
// A dataset that isn't found there keeps the label from the content and has no link.
const { data: datasetModels } = await useAsyncData(`${route.path}:datasets`, () => Promise.all(
  showcase.value.datasets.map(async ({ id: uri, label }) => {
    const identifier = uri.split('/').pop()
    const model = await getDatasetsByIdentifier(identifier).catch(() => null)
    return { identifier, label, model }
  }),
))

const showcaseDatasets = computed(() => (datasetModels.value ?? []).map(({ identifier, label, model }) => {
  if (!model) {
    return { key: identifier, title: label || identifier }
  }
  const dataset = new DcatApChV2DatasetAdapter(model, locale.value, t)
  return {
    key: identifier,
    id: dataset.id,
    title: dataset.title,
    publisherName: localize(dataset.publisher.name, locale.value),
    publisherHomepage: dataset.publisher.homepage,
  }
}))

useSeoMeta({
  title: `${showcase.value?.title} | ${t('message.header.navigation.showcases')} | opendata.swiss`,
})
</script>

<template>
  <OdsPage
    v-if="showcase"
    :page="showcase"
  >
    <template #header>
      <OdsBreadcrumbs :breadcrumbs="breadcrumbs" />
    </template>

    <template #hero-content>
      <img
        v-if="showcase.image"
        :src="showcase.image"
        :alt="showcase.title"
      >
    </template>

    <template #aside-content>
      <OdsCard
        v-if="showcase.url"
        :title="t('message.showcase.externalLink')"
      >
        <OdsButton
          icon="External"
          variant="outline-negative"
          :href="showcase.url"
        >
          {{ t('message.showcase.open') }}
        </OdsButton>
      </OdsCard>

      <OdsCard :title="t('message.dataset_detail.additional_information')">
        <OdsInfoBlock :title="t('message.showcase.type.header')">
          {{ showcaseType }}
        </OdsInfoBlock>
        <OdsInfoBlock
          v-if="showcaseCategories.length > 0"
          :title="t('message.showcase.categories')"
        >
          <ul>
            <li
              v-for="category in showcaseCategories"
              :key="category.code"
            >
              {{ localize(category.name, locale) }}
            </li>
          </ul>
        </OdsInfoBlock>
        <OdsInfoBlock
          v-if="showcaseDatasets.length > 0"
          :title="t('message.showcase.datasets')"
        >
          <ul>
            <li
              v-for="dataset in showcaseDatasets"
              :key="dataset.key"
            >
              <NuxtLinkLocale
                v-if="dataset.id"
                :to="{ name: 'datasets-datasetId', params: { datasetId: dataset.id } }"
              >
                {{ dataset.title }}
              </NuxtLinkLocale>
              <template v-else>
                {{ dataset.title }}
              </template>
              <template v-if="dataset.publisherName">
                {{ ` ${t('message.showcase.dataset_from')}` }}
                <a
                  v-if="dataset.publisherHomepage"
                  :href="dataset.publisherHomepage"
                  rel="noopener noreferrer"
                  class="link--external"
                  target="_blank"
                >
                  {{ dataset.publisherName }}
                </a>
                <template v-else>
                  {{ dataset.publisherName }}
                </template>
              </template>
            </li>
          </ul>
        </OdsInfoBlock>
        <OdsInfoBlock
          v-if="showcase.keywords?.length > 0"
          :title="t('message.showcase.keywords')"
        >
          <OdsTagItem
            v-for="keyword in showcase.keywords"
            :id="`keyword-${keyword}`"
            :key="keyword"
            :label="keyword"
          />
        </OdsInfoBlock>
        <OdsInfoBlock
          v-if="showcase.contactDetails"
          :title="t('message.showcase.submitted_by')"
        >
          <p>{{ showcase.contactDetails.name }}</p>
          <a
            v-for="link in showcase.contactDetails.url"
            :key="link"
            class="link--external"
            target="_blank"
            :href="link"
          >
            {{ link }}
          </a>
        </OdsInfoBlock>
      </OdsCard>
    </template>
  </OdsPage>
</template>
