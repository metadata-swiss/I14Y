<template>
  <OdsCard
    :title="post.title"
    clickable
  >
    <template #image>
      <NuxtImg
        :src="post.image"
        :alt="post.title"
      />
    </template>

    <template
      v-if="post.date"
      #top-meta
    >
      <NuxtTime
        class="meta-info__item"
        :datetime="new Date(post.date)"
        relative
      />
    </template>
    <template #footer-action>
      <NuxtLinkLocale
        :to="{ name: 'blog-year-month-slug', params: { year: post.year, month: post.month, slug: post.slug } }"
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
import type { BlogCollectionItem } from '@nuxt/content'

const { locale } = useI18n()

const { post: collectionItem } = defineProps<{
  post: BlogCollectionItem
}>()

const post = computed(() => {
  const date = new Date(collectionItem.date!)

  return {
    ...collectionItem,
    year: date.getFullYear(),
    month: date.getMonth() + 1,
    slug: collectionItem.slug || collectionItem.path.split('/').pop()!.replace(`.${locale.value}`, ''),
  }
})
</script>
