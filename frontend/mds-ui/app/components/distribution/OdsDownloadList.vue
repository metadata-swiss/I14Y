<template>
  <ul class="download-items">
    <li
      v-for="url in props.urls"
      :key="url"
    >
      <OdsDownloadListItem
        :url="url"
        :name="props.name"
        :format="props.format"
        :languages="props.languages"
        :byte-size="props.byteSize"
        :icon="props.icon"
      />
    </li>
  </ul>
</template>

<script setup lang="ts">
import OdsDownloadListItem from './OdsDownloadListItem.vue'

interface OdsDownloadListProps {
  name: string
  urls: string[]
  format?: string
  languages?: string[]
  byteSize?: string
  icon: string
}
const props = defineProps<OdsDownloadListProps>()
</script>
