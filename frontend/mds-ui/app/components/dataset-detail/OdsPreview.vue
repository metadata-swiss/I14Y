<template>
  <div
    v-if="hasPreview"
    class="box"
  >
    <section>
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
            :api-base-url="previewServiceUrl"
            button-border-radius="0px"
          />
        </div>
      </div>
    </section>
  </div>
</template>

<script lang="ts">
import { computed, ref, watchEffect } from 'vue'
import { DistributionVisualisation, registerCharts } from 'piveau-preview-plugin'
// The preview's styles and chart library are only loaded with this component, which the distribution page loads
// lazily, so the other pages don't download them.
import 'piveau-preview-plugin/style.css'
import 'ag-grid-community/styles/ag-grid.css'
import 'ag-grid-community/styles/ag-theme-quartz.css'

let chartsRegistered = false
</script>

<script setup lang="ts">
if (import.meta.client && !chartsRegistered) {
  registerCharts()
  chartsRegistered = true
}

// iop-core gap [G15]: the preview needs a service that reads the file: GET {url}?urlString=…&fileFormat=…. None is
// configured by default; the original team's service (piveau-hub-data-preview) can't be used. Set
// NUXT_PUBLIC_DATA_PREVIEW_URL once iop-core provides the endpoint (V2).
const previewServiceUrl = useRuntimeConfig().public.dataPreviewUrl

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
  'text/csv': 'csv',
  'text/tab-separated-values': 'tsv',
  'application/vnd.oasis.opendocument.spreadsheet': 'ods',
  'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': 'xlsx',
  'application/vnd.ms-excel': 'xls',
}

// The format can be a name ("CSV"), a media type ("text/csv") or a URI ending in the format
// (".../file-type/CSV").
const normalizedPreviewFormat = computed<SupportedPreviewFormat | null>(() => {
  const normalized = props.fileFormat.trim().toLowerCase()
  if (!normalized) {
    return null
  }

  const formatParts = normalized.split(/[/#]/)
  const lastPart = formatParts[formatParts.length - 1] || normalized
  const alias = FORMAT_ALIASES[normalized] ?? FORMAT_ALIASES[lastPart]
  if (alias) {
    return alias
  }

  return SUPPORTED_PREVIEW_FORMATS.includes(lastPart as SupportedPreviewFormat)
    ? lastPart as SupportedPreviewFormat
    : null
})

const previewFormat = ref<SupportedPreviewFormat>('csv')

watchEffect(() => {
  if (normalizedPreviewFormat.value) {
    previewFormat.value = normalizedPreviewFormat.value
  }
})

const hasPreview = computed(() => {
  return Boolean(
    previewServiceUrl
    && props.downloadUrl
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
