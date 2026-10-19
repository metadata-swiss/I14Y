<template>
  <div class="ods-license">
    <img
      :src="imageSrc"
      :alt="t(`message.terms_of_use.ods_${termsName}.title`)"
      :title="t(`message.terms_of_use.ods_${termsName}.title`)"
    >
  </div>
  <ul>
    <li>{{ t(`message.terms_of_use.ods_${termsName}.condition_1`) }}</li>
    <li>{{ t(`message.terms_of_use.ods_${termsName}.condition_2`) }}</li>
    <li>{{ t(`message.terms_of_use.ods_${termsName}.condition_3`) }}</li>
  </ul>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from '#imports'
import { termsOfUseName } from '~/utils/terms-of-use'

const { t } = useI18n()

const props = defineProps({
  name: {
    type: String,
    required: true,
  },
})

const termsName = computed(() => termsOfUseName(props.name))

const imageSrc = computed(() => `/img/terms-of-use/terms_${termsName.value}.svg`)
</script>

<style lang="scss" scoped>
.ods-license {
   height: 62px;
}
img {
    height: 100%;
    width: auto;
    margin: 0 auto;
}
</style>
