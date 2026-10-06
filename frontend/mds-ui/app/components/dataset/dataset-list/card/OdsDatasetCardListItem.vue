<template>
  <OdsCard
    :title="props.dataset.title ?? ''"
    type="universal"
    clickable
    class="strech-card"
  >
    <template #top-meta>
      <div>
        <span class="meta-info__item">{{ t('message.dataset_detail.dataset') }}</span>
        <span class="meta-info__item">{{ props.dataset.catalog.publisher.name }}</span>
      </div>
    </template>
    <div>
      <p lass="meta-info">
        <span
          v-for="(category, index) in props.dataset.getCategoriesForLanguage(locale).map(k => k.label)"
          :key="`${category}-${index}`"
          class="meta-info__item"
        >{{ category }}</span>
      </p>
    </div>
    <p>{{ props.dataset.description }} </p>
    <template #icons>
      <OdsTagList
        v-if="props.dataset.keywords.length > 0"
        :tags="[...props.dataset.formats, ...props.dataset.keywords.map(k => { k.size = 'ods'; k.variant = 'light'; return k })]"
      />
    </template>
    <template #footer-info>
      <p class="meta-info">
        <span
          v-if="props.dataset.modificationDate || props.dataset.releaseDate"
          class="meta-info__item"
        >
          {{ t('message.dataset_detail.modified_on') }}:
          <NuxtTime
            v-if="props.dataset.releaseDate"
            :datetime="props.dataset.modificationDate ?? props.dataset.releaseDate"
            :locale="locale"
          />
        </span>
      </p>
      <span class="meta-info__item">
        <CommentCount
          :page-id="`dataset-${props.dataset.id}`"
          :language="locale"
        />
      </span>
    </template>
    <template #footer-action>
      <NuxtLinkLocale
        :to="{ name: 'datasets-datasetId', params: { datasetId: props.dataset.id }, query: searchParams }"
        type="false"
        class="btn btn--outline btn--icon-only"
        aria-label="false"
      >
        <SvgIcon
          icon="ArrowRight"
          role="btn"
        />
        <span class="btn__text">Weiterlesen</span>
      </NuxtLinkLocale>
    </template>
  </OdsCard>
</template>

<script setup lang="ts">
import { useI18n } from '#imports'
import OdsCard from '../../../content/OdsCard.vue'
import SvgIcon from '~/components/SvgIcon.vue'
import type { DcatApChV2DatasetAdapter } from '../../../dataset-detail/model/dcat-ap-ch-v2-dataset-adapter.js'
import type { LocationQueryRaw } from 'vue-router'
import { CommentCount } from '@hyvor/hyvor-talk-vue'
import OdsTagList from '../../../dataset-detail/OdsTagList.vue'

const { t, locale } = useI18n()

interface Props {
  dataset: DcatApChV2DatasetAdapter
  searchParams?: LocationQueryRaw
}

const props = defineProps<Props>()
</script>

<style scoped lang="scss">
.strech-card {
  height: 100%;
}
</style>
