<template>
  <NuxtLinkLocale
    type="button"
    :to="link"
    :class="computedClasses"
    :aria-label="label"
    :disabled="disabled ? 'disabled' : undefined"
  >
    <SvgIcon
      v-if="icon"
      :icon="icon"
      class="btn__icon"
    />
    <span
      v-if="label"
      class="btn__text"
    >
      {{ label }}
    </span>
  </NuxtLinkLocale>
</template>

<script setup lang="ts">
import SvgIcon from './SvgIcon.vue'
import type { RouteLocationNamedI18n } from 'vue-router'
import { computed } from 'vue'

const props = defineProps({
  type: {
    type: String,
    default: () => '',
    validator: prop =>
      ['outline', 'outline-negative'].includes(prop as string),
  },
  icon: {
    type: String,
    default: () => '',
  },
  link: {
    type: [Object, String] as PropType<RouteLocationNamedI18n<string | symbol>>,
    required: true,
  },
  label: {
    type: String,
    default: () => '',
  },
  disabled: {
    type: Boolean,
    default: () => false,
  },
})

const computedClasses = computed(() => {
  let base = 'btn '
  if (props.type) base += `btn--${props.type} `
  if (props.icon) base += 'btn--icon-only '
  return base
})
</script>
