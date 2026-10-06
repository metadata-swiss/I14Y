<template>
  <NuxtLink v-bind="linkProps">
    <slot />
  </NuxtLink>
</template>

<script setup lang="ts">
import type { PropType } from 'vue'
import isAbsolute from 'is-absolute-url'

const props = defineProps({
  href: {
    type: String,
    default: '',
  },
  target: {
    type: String as PropType<'_blank' | '_parent' | '_self' | '_top' | (string & object) | null | undefined>,
    default: undefined,
    required: false,
  },
})

const isExternal = (() => {
  if (!props.href) {
    return false
  }

  if (props.href.startsWith('mailto')) {
    return false
  }

  if (isAbsolute(props.href)) {
    const base = import.meta.client ? window.location : useRequestURL()

    return new URL(props.href).host !== base.host
  }

  return false
})()

const linkProps = computed(() => {
  if (!isExternal) {
    return props
  }

  return {
    ...props,
    rel: 'noopener noreferrer',
    class: 'link--external',
  }
})
</script>
