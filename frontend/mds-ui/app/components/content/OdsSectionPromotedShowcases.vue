<script setup lang="ts">
import OdsSection, { type SectionLayout } from './OdsSection.vue'
import OdsShowcaseCard from '~/components/showcases/OdsShowcaseCard.vue'
import { showcaseId, usePublishedShowcases } from '~/composables/useShowcases'

const { showcases: selected = [], max } = defineProps<{
  title: string
  max: number
  layout: SectionLayout
  // The showcases chosen by the editors, by file name (the id in /showcase/{id}).
  showcases?: { id: string, label?: string }[]
}>()

const publishedShowcases = await usePublishedShowcases()

// The showcases chosen by the editors first, then the pinned ones, then the others, up to `max`.
// The chosen and pinned ones show their description.
const promotedShowcases = computed(() => {
  const selectedIds = selected.map(({ id }) => id)
  const isSelected = (showcase: { stem: string }) => selectedIds.includes(showcaseId(showcase))
  const chosen = selectedIds.flatMap(id => publishedShowcases.value.filter(showcase => showcaseId(showcase) === id))
  const pinned = publishedShowcases.value.filter(showcase => showcase.pinned && !isSelected(showcase))
  const others = publishedShowcases.value.filter(showcase => !showcase.pinned && !isSelected(showcase))

  return [
    ...[...chosen, ...pinned].map(showcase => ({ showcase, excerpt: true })),
    ...others.map(showcase => ({ showcase, excerpt: false })),
  ].slice(0, max)
})
</script>

<template>
  <OdsSection
    id="promoted-showcases"
    :layout="layout"
    accent-color="100"
    text-color="600"
    :title="title"
  >
    <OdsShowcaseCard
      v-for="{ showcase, excerpt } in promotedShowcases"
      :key="showcase.id"
      :showcase="showcase"
      :no-excerpt="!excerpt"
    />
  </OdsSection>
</template>
