<template>
  <button class="burger" :title="t('message.button.toggle_menu')" @click="emit('update:modelValue', !props.modelValue)">
    <span
      class="burger__icon"
      :class="{ 'burger--is-open': props.modelValue }"
    >
      <span class="burger__bar"/>
      <span class="burger__bar"/>
      <span class="burger__bar"/>
    </span>
  </button>
</template>

<script setup lang="ts">
const { t } = useI18n()

const props = defineProps<{ modelValue: boolean }>()
const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
}>()

onUnmounted(() => {
  // Ensure the burger button is closed when the component is unmounted
  emit('update:modelValue', false)
})
</script>

<style scoped lang="scss">

</style>
