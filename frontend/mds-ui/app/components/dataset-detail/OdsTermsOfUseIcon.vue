<template>
  <img
    :src="imageSrc"
    :alt="t(`message.terms_of_use.ods_${termsName}.title`)"
    :title="t(`message.terms_of_use.ods_${termsName}.title`)"
    class="ods-terms-of-use_image"
  >
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from '#imports'
import type { OdsLicense } from '~/piveau/get-ods-licenses'

const { t } = useI18n()

interface Props {
  license: OdsLicense
}

const props = defineProps<Props>()

const termsName = computed(() => {
  if (!props.license) {
    console.warn('License is undefined')
    return 'ask'
  }
  const licnsesId = props.license.id
  switch (licnsesId) {
    case 'http://dcat-ap.ch/vocabulary/licenses/terms_by':
    case 'https://opendata.swiss/en/terms-of-use/#terms_by':
      return 'by'
    case 'http://dcat-ap.ch/vocabulary/licenses/terms_open':
    case 'https://opendata.swiss/en/terms-of-use/#terms_open':
      return 'open'
    case 'http://dcat-ap.ch/vocabulary/licenses/terms_ask':
    case 'https://opendata.swiss/en/terms-of-use/#terms_ask':
      return 'ask'
    case 'http://dcat-ap.ch/vocabulary/licenses/terms_by_ask':
    case 'https://opendata.swiss/en/terms-of-use/#terms_by_ask':
      return 'by_ask'
    default:
      return props.license.id
  }
})

const imageSrc = computed(() => `/img/terms-of-use/terms_${termsName.value}.svg`)
</script>

<style lang="scss" scoped>
.ods-terms-of-use_image {
  height: inherit;
}
</style>
