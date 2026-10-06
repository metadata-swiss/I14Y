<template>
  <section v-if="hasPreview">
    <div class="container container--grid ">
      <div class="container__preview container__main vertical-spacing">
        <h2 class="h2">
          Preview
        </h2>
        <DistributionVisualisation
          :download-url="downloadUrl"
          :file-format="previewFormat"
          :title="title"
          :show-ai-tab="false"
          primary-color="var(--color-primary-600)"
          api-base-url="https://piveau-hub-data-preview.abn.ods.zazukoians.org/dataPreview"
          button-border-radius="0px"
        />
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, ref, watchEffect } from 'vue'
import { DistributionVisualisation } from 'piveau-preview-plugin'

const props = withDefaults(defineProps<{
  downloadUrl: string
  fileFormat: string
  title?: string
}>(), {
  title: 'Data Preview',
})

const SUPPORTED_PREVIEW_FORMATS = ['csv', 'tsv', 'ods', 'xlsx', 'xls'] as const
type SupportedPreviewFormat = typeof SUPPORTED_PREVIEW_FORMATS[number]

const FORMAT_ALIASES: Record<string, SupportedPreviewFormat> = {
  'excel xlsx': 'xlsx',
  'excel xls': 'xls',
}

const normalizedFormat = computed(() => props.fileFormat.trim().toLowerCase())

const normalizedPreviewFormat = computed<SupportedPreviewFormat | null>(() => {
  const alias = FORMAT_ALIASES[normalizedFormat.value]
  if (alias) {
    return alias
  }

  const format = normalizedFormat.value as SupportedPreviewFormat
  return SUPPORTED_PREVIEW_FORMATS.includes(format) ? format : null
})

const previewFormat = ref<SupportedPreviewFormat>('csv')

watchEffect(() => {
  if (normalizedPreviewFormat.value) {
    previewFormat.value = normalizedPreviewFormat.value
  }
})

const hasPreview = computed(() => {
  return Boolean(
    props.downloadUrl
    && normalizedPreviewFormat.value,
  )
})

const { downloadUrl, title } = props
</script>

<style scoped>
@media (min-width: 1024px) {
  .container__preview {
    font-variant-east-asian: jis90;
    grid-column: 2 / -2;
    width: 100%;
  }
}
</style>
