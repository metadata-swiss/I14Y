<template>
  <OdsCard
    style="height: 100%;"
    :title="showcase.title ? getCurrentTranslation(showcase.title, locale) : ''"
    clickable
  >
    <template #image>
      <img
        v-if="showcase.image"
        :src="showcase.image[0]"
        :alt="showcase.title ? getCurrentTranslation(showcase.title, locale) : ''"
      >
    </template>

    <template #top-meta>
      <div>
        <span class="meta-info__item">
          {{ getCurrentTranslation(showcaseType(showcase)?.pref_label, locale) }}
        </span>
        <span
          v-if="showcase.references?.length"
          class="meta-info__item"
        >
          {{ t('message.showcase.search.dataset_references', { count: showcase.references?.length }) }}
        </span>
      </div>
    </template>

    <template #footer-info>
      {{ showcase.keywords?.map(({ label }) => label).join(', ') }}
    </template>

    <div class="body">
      <MDC
        v-if="!noExcerpt"
        :value="getCurrentTranslation(showcase.abstract, locale)"
      />
    </div>

    <template #footer-action>
      <NuxtLinkLocale
        :to="{ name: 'showcase-id', params: { id: showcase.id } }"
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
import OdsCard from '~/components/content/OdsCard.vue'
import SvgIcon from '~/components/SvgIcon.vue'
import { getCurrentTranslation } from '~/lib/getCurrentTranslation'
import { useShowcaseTypes } from '~~/composables/useShowcaseTypes'
import type { PiveauShowcase } from '~/piveau/showcases'

const { locale, t } = useI18n()

const { showcase } = defineProps<{
  showcase: PiveauShowcase
  noExcerpt?: boolean
}>()

const { data: showcaseTypes, ensureLoaded } = useShowcaseTypes()
await ensureLoaded()

function showcaseType(showcase: PiveauShowcase) {
  return showcaseTypes.value?.find(type => type.resource === showcase.type.resource)
}
</script>

<style lang="scss" scoped>
.body {
  overflow: hidden;
  text-overflow: ellipsis;
  height: 15em;
  -webkit-mask-image: linear-gradient(180deg, #000 60%, transparent 100%);
  mask-image: linear-gradient(180deg, #000 60%, transparent 100%);
}
</style>
