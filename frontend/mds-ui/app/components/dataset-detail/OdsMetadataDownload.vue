<template>
  <div
    v-if="hasJsonLD || hasTTL || hasXML"
    class="info-block column"
  >
    <h3 class="info-block__title">
      {{ t('message.dataset_detail.metadata') }}
    </h3>
    <div class="buttons">
      <OdsButton
        v-if="hasJsonLD"
        :title=" t('message.dataset_detail.metadata_download') + ' (JSON-LD)'"
        variant="outline"
        class="btn--back"
        size="sm"
        icon="Download"
        :href="piveauHubRepoUrl + 'datasets' + props.dataset.getLinkedData['jsonld']"
        target="_blank"
      >
        <span>JSON-LD</span>
      </OdsButton>
      <OdsButton
        v-if="hasTTL"
        :title=" t('message.dataset_detail.metadata_download') + ' (TTL)'"
        variant="outline"
        class="btn--back"
        size="sm"
        icon="Download"
        :href="piveauHubRepoUrl + 'datasets' + props.dataset.getLinkedData['ttl']"
        target="_blank"
      >
        <span>TTL</span>
      </OdsButton>
      <OdsButton
        v-if="hasXML"
        :title=" t('message.dataset_detail.metadata_download') + ' (XML)'"
        variant="outline"
        class="btn--back"
        size="sm"
        icon="Download"
        :href="piveauHubRepoUrl + 'datasets' + props.dataset.getLinkedData['rdf']"
        target="_blank"
      >
        <span>XML</span>
      </OdsButton>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from '#imports'
import type { DcatApChV2DatasetAdapter } from './model/dcat-ap-ch-v2-dataset-adapter'
import OdsButton from '../OdsButton.vue'

const { piveauHubRepoUrl } = useRuntimeConfig().public

const { t } = useI18n()

interface Props {
  dataset: DcatApChV2DatasetAdapter
}

const props = defineProps<Props>()

const hasXML = computed(() => {
  const metaurl = props.dataset.getLinkedData['rdf']
  if (!metaurl) {
    return false
  }
  if (metaurl.length === 0) {
    return false
  }
  return true
},
)

const hasJsonLD = computed(() => {
  const metaurl = props.dataset.getLinkedData['jsonld']
  if (!metaurl) {
    return false
  }
  if (metaurl.length === 0) {
    return false
  }
  return true
},
)

const hasTTL = computed(() => {
  const metaurl = props.dataset.getLinkedData['ttl']
  if (!metaurl) {
    return false
  }
  if (metaurl.length === 0) {
    return false
  }
  return true
},
)
</script>

<style lang="scss" scoped>
.column {
  display: flex;
  flex-direction: column;
}
.buttons {
  display:flex;
  flex-direction: row;
  gap:12px;
}
</style>
