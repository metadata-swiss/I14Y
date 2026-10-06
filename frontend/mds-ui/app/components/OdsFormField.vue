<template>
  <div :class="`form-group form__group__${type}`">
    <label
      v-if="label"
      :for="props.for"
      :class="labelClasses"
    >
      {{ label }}
    </label>
    <slot />
  </div>
</template>

<script setup lang="ts">
const { type = 'input', ...props } = defineProps<{
  for: string
  label?: string
  required?: boolean
  size?: 'sm' | 'base' | 'lg'
  variant?: 'outline' | 'negative'
  type?: 'input' | 'select'
}>()

const labelClasses = computed(() => {
  let base = ''
  if (props.variant === 'negative') base += 'text--negative '
  if (props.size) base += `text--${props.size} `
  if (props.required) base += 'text--asterisk '
  return base
})
</script>
