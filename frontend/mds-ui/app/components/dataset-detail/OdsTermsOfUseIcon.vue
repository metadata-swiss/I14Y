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
import type { OdsLicense } from '~/model/dataset/dcat-ap-ch-v2-dataset-adapter'
import { termsOfUseName } from '~/utils/terms-of-use'

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
  return termsOfUseName(props.license.id)
})

const imageSrc = computed(() => `/img/terms-of-use/terms_${termsName.value}.svg`)
</script>

<style lang="scss" scoped>
.ods-terms-of-use_image {
  height: inherit;
}
</style>
