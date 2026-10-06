<template>
  <div
    v-for="path in props.dataset.getLinkedData"
    :key="path"
  >
    <a
      :href="piveauHubRepoUrl + 'datasets' + path"
      target="_blank"
      :title="t(`message.dataset_detail.metadata_download`)"
      :aria-label="t(`message.dataset_detail.metadata_download`)"
      class="download-item"
    >
      <SvgIcon
        icon="Download"
        size="xl"
        class="download-item__icon"
      />
      <div>
        <h2 class="download-item__title">{{ (t(`message.dataset_detail.metadata`)).toLowerCase() + '.' + path.split('.').pop() }}</h2>
        <p class="meta-info download-item__meta-info">
          <span class="meta-info__item">{{ path.split('.').pop() }}</span>
        </p>
      </div>
    </a>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from '#imports'
import SvgIcon from '../SvgIcon.vue'
import type { DcatApChV2DatasetAdapter } from './model/dcat-ap-ch-v2-dataset-adapter'

const { piveauHubRepoUrl } = useRuntimeConfig().public
const { t } = useI18n()

interface Props {
  dataset: DcatApChV2DatasetAdapter
}

const props = defineProps<Props>()
</script>

<style scoped lang="scss">
  a {
    overflow-x: hidden;
    word-break: break-all;
  }

  .download-item__title {
    text-overflow: hidden;
    overflow-x: hidden;
    word-break: break-all;
  }
</style>
