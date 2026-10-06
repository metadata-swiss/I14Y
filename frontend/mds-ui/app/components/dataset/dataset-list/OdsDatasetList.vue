<script setup lang="ts">
import OdsDatasetCardListItem from './card/OdsDatasetCardListItem.vue'
import OdsDatasetListItem from './list/OdsDatasetListItem.vue'
import type { LocationQueryRaw } from '#vue-router'
import type { DcatApChV2DatasetAdapter } from '../../dataset-detail/model/dcat-ap-ch-v2-dataset-adapter.js'
import { waitUntil } from 'async-wait-until'

declare global {
  interface Window {
    hyvorTalkCommentCounts?: {
      load(arg: { 'website-id': number }): void
    }
  }
}

interface Props {
  items: DcatApChV2DatasetAdapter[]
  listType: 'card' | 'list'
  searchParams?: LocationQueryRaw
}

const props = defineProps<Props>()

const searchParamsEncoded = computed(() => {
  // Convert LocationQueryRaw to Record<string, string>
  const raw = props.searchParams || {}
  const paramsObj: Record<string, string> = {}
  Object.entries(raw).forEach(([key, value]) => {
    if (Array.isArray(value)) {
      paramsObj[key] = value.join(',')
    }
    else if (typeof value === 'string') {
      paramsObj[key] = value
    }
    else if (value != null) {
      paramsObj[key] = String(value)
    }
  })
  const params = new URLSearchParams(paramsObj)
  return {
    search: params.toString(),
  }
})

async function loadCommentCounts() {
  await waitUntil(() => window.hyvorTalkCommentCounts, 1000)
  window.hyvorTalkCommentCounts!.load({ 'website-id': 15455 })
}

onMounted(loadCommentCounts)
onUpdated(loadCommentCounts)
</script>

<template>
  <div
    v-if="props.items && props.listType === 'card'"
    class="ods-card-list"
  >
    <ul class="search-results-list">
      <li
        v-for="dataset in props.items"
        :key="dataset.id"
      >
        <OdsDatasetCardListItem
          :dataset="dataset"
          :search-params="searchParamsEncoded"
        />
      </li>
    </ul>
  </div>

  <ul v-if="props.items && props.listType === 'list'">
    <li
      v-for="dataset in props.items"
      :key="dataset.id"
    >
      <OdsDatasetListItem
        :dataset="dataset"
        :search-params="searchParamsEncoded"
      />
    </li>
  </ul>
</template>

<style lang="scss" scoped>
ol, ul {
    list-style: none !important;
    margin: 0 !important;
    padding: 0 !important;
}

.ods-card-list {
        margin-top: 2.5rem !important;
}

@media (min-width: 1024px) {
  .ods-card-list {
        margin-top: 3rem !important;
  }
}
@media (min-width: 640px) {
  .ods-card-list {
        margin-top: 2.25rem !important;
  }
}
@media (min-width: 480px) {
    .ods-card-list {
        margin-top: 1.75rem !important;
  }
}
</style>
