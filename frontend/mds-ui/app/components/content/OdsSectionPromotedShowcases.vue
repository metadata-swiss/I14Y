<script setup lang="ts">
import OdsSection, { type SectionLayout } from './OdsSection.vue'
import OdsShowcaseCard from '~/components/showcases/OdsShowcaseCard.vue'
import { useShowcaseSearch } from '~/piveau/showcases'

const { max: limit } = defineProps<{
  title: string
  max: number
  layout: SectionLayout
}>()

const { useSearch } = useShowcaseSearch()

const { query: latestShowcasesQuery, getSearchResultsEnhanced: latestShowcases } = useSearch({
  queryParams: {
    limit,
    sort: 'issued+desc',
  },
  additionalParams: {
    resource: 'showcase',
  },
})

const { query: pinnedShowcasesQuery, getSearchResultsEnhanced: pinnedShowcases } = useSearch({
  queryParams: {
    sort: 'issued+desc',
  },
  additionalParams: {
    resource: 'showcase',
  },
})

await Promise.all([
  latestShowcasesQuery.suspense(),
  pinnedShowcasesQuery.suspense(),
])
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
      v-for="showcase in pinnedShowcases"
      :key="showcase.id"
      :showcase="showcase"
    />
    <OdsShowcaseCard
      v-for="showcase in latestShowcases"
      :key="showcase.id"
      :showcase="showcase"
      no-excerpt
      :slideshow-card="layout === 'slideshow'"
    />
  </OdsSection>
</template>
