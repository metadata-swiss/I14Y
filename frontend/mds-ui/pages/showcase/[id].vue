<script setup>
import OdsPage from '../../app/components/OdsPage.vue'
import { homePageBreadcrumb } from '../../app/composables/breadcrumbs.js'
import OdsInfoBlock from '../../app/components/OdsInfoBlock.vue'
import OdsTagItem from '../../app/components/OdsTagItem.vue'
import OdsBreadcrumbs from '../../app/components/OdsBreadcrumbs.vue'
import OdsCard from '../../app/components/content/OdsCard.vue'
import OdsButton from '../../app/components/OdsButton.vue'
import { getCurrentTranslation } from '../../app/lib/getCurrentTranslation.js'
import { useVocabularySearch } from '../../app/piveau/vocabularies.js'
import { useDatasetsSearch } from '../../app/piveau/datasets.js'

const route = useRoute()
const { locale, t } = useI18n()
const { id } = route.params

const { data: showcase } = await useAsyncData(route.path, async () => {
  const currentTranslation = await queryPublishedContent('showcases')
    .where('stem', 'LIKE', `%${id}.${locale.value}`)
    .where('active', '=', true)
    .first()

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

const breadcrumbs = [
  await homePageBreadcrumb(locale),
  {
    title: t('message.header.navigation.showcases'),
    path: '/showcases',
  },
  {
    title: showcase.value?.title || id,
  },
]

const showcaseCategoriesRaw = await Promise.all(showcase.value?.themes.map(async (themeId) => {
  const { query, resultEnhanced } = useVocabularySearch().useResource('data-theme/vocable', { additionalParams: { resource: themeId } })
  await query.suspense()
  return resultEnhanced.value
}))

const { query, resultEnhanced } = useVocabularySearch().useResource('showcase-types/vocable', { additionalParams: { resource: showcase.value.type } })
await query.suspense()
const showcaseType = resultEnhanced.value

const showcaseCategories = computed(() => showcaseCategoriesRaw.filter(Boolean))

const showcaseDatasetsRaw = await Promise.all(showcase.value.datasets.map(async ({ id: uri }) => {
  const id = uri.split('/').pop()
  const { query, resultEnhanced } = useDatasetsSearch().useResource(id)
  await query.suspense()
  return resultEnhanced.value
}))

const showcaseDatasets = computed(() => showcaseDatasetsRaw.filter(Boolean))

useSeoMeta({
  title: `${showcase.value?.title} | ${t('message.header.navigation.showcases')} | opendata.swiss`,
})
</script>

<template>
  <OdsPage
    v-if="showcase"
    :comments-id="showcase.stem.replace(/\.\w\w$/, '')"
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
          {{ showcaseType?.pref_label && getCurrentTranslation(showcaseType.pref_label, locale) }}
        </OdsInfoBlock>
        <OdsInfoBlock
          v-if="showcaseCategories.length > 0"
          :title="t('message.showcase.categories')"
        >
          <ul>
            <li
              v-for="category in showcaseCategories"
              :key="category.id"
            >
              {{ category.pref_label && getCurrentTranslation(category.pref_label, locale) }}
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
              :key="dataset.getId"
            >
              <NuxtLinkLocale :to="{ name: 'datasets-datasetId', params: { datasetId: dataset.getId } }">
                {{ dataset.getTitle }}
              </NuxtLinkLocale>
              <template v-if="dataset.getPublisher">
                {{ ` ${t('message.showcase.dataset_from')}` }}
                <a
                  v-if="dataset.getPublisher.homepage"
                  :href="dataset.getPublisher.homepage"
                  rel="noopener noreferrer"
                  class="link--external"
                  target="_blank"
                >
                  {{ dataset.getPublisher.name }}
                </a>
                <template v-else>
                  {{ dataset.getPublisher.name }}
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
