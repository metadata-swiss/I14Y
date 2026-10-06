<template>
  <div class="card card--list card--clickable">
    <div class="card__content">
      <div class="card__body">
        <p class="meta-info">
          <span class="meta-info__item">{{ t('message.dataset_detail.dataset') }}</span>
          <span class="meta-info__item">{{ props.dataset.catalog.publisher.name }}</span>
        </p>
        <div class="card__title">
          <h3>{{ props.dataset.title }}</h3>
        </div>
        <p lass="meta-info">
          <span
            v-for="(category, index) in props.dataset.getCategoriesForLanguage(locale).map(k => k.label)"
            :key="`${category}-${index}`"
            class="meta-info__item"
          >{{ category }}</span>
        </p>
        <p>{{ props.dataset.description }}</p>
      </div>
      <div class="default-margin">
        <OdsTagList
          v-if="props.dataset.keywords.length > 0"
          :tags="[...props.dataset.formats, ...props.dataset.keywords.map(k => { k.size = 'ods'; k.variant = 'light'; return k })]"
        />
      </div>
      <div class="card__footer default-margin">
        <div class="card__footer__info">
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
            <span class="meta-info__item">
              <CommentCount
                :page-id="`dataset-${props.dataset.id}`"
                :language="locale"
              />
            </span>
          </p>
        </div>
        <div class="card__footer__action">
          <NuxtLinkLocale
            :to="{ name: 'datasets-datasetId', params: { datasetId: props.dataset.id }, query: searchParams }"
            type="false"
            class="btn btn--outline btn--icon-only"
            aria-label="false"
          >
            <span class="btn__text">Weiterlesen</span>
          </NuxtLinkLocale>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from '#imports'
import type { DcatApChV2DatasetAdapter } from '../../../dataset-detail/model/dcat-ap-ch-v2-dataset-adapter'
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

<style lang="scss" scoped>
.default-margin {
  margin-top: 16px;
}
</style>
