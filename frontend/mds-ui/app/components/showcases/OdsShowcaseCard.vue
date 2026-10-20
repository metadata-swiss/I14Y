<template>
  <OdsCard
    style="height: 100%;"
    :title="showcase.title"
    clickable
  >
    <template #image>
      <img
        v-if="image"
        :src="image"
        :alt="showcase.title"
      >
    </template>

    <template #top-meta>
      <div>
        <span class="meta-info__item">
          {{ showcaseTypeLabel(showcase.type, t) }}
        </span>
        <span
          v-if="showcase.datasets?.length"
          class="meta-info__item"
        >
          {{ t('message.showcase.search.dataset_references', { count: showcase.datasets.length }) }}
        </span>
      </div>
    </template>

    <template #footer-info>
      {{ showcase.keywords?.join(', ') }}
    </template>

    <div
      v-if="!noExcerpt"
      class="body"
    >
      <p>{{ showcase.description }}</p>
    </div>

    <template #footer-action>
      <NuxtLinkLocale
        :to="{ name: 'showcase-id', params: { id: showcaseId(showcase) } }"
        type="false"
        class="btn btn--outline btn--icon-only"
      >
        <SvgIcon
          icon="ArrowRight"
          role="btn"
        />
        <span class="btn__text">{{ t('message.button.read_more') }}</span>
      </NuxtLinkLocale>
    </template>
  </OdsCard>
</template>

<script setup lang="ts">
import OdsCard from '~/components/content/OdsCard.vue'
import SvgIcon from '~/components/SvgIcon.vue'
import { showcaseTypeLabel } from '~/utils/showcase-type'
import { showcaseId, type Showcase } from '~/composables/useShowcases'

const { t } = useI18n()

const { showcase } = defineProps<{
  showcase: Showcase
  noExcerpt?: boolean
}>()

const image = computed(() => showcase.images?.[0]?.image)
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
