<template>
  <component
    :is="tag"
    v-bind="tagAttrs"
    :class="['btn', 'btn--base', classes]"
    :aria-label="title"
    :title="title"
  >
    <slot name="icon">
      <SvgIcon
        v-if="icon"
        :icon="icon"
        :size="size"
        class="btn__icon"
      />
    </slot>
    <span class="btn__text">
      <slot>{{ title }}</slot>
    </span>
  </component>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import SvgIcon from './SvgIcon.vue'

const localePath = useLocalePath()

const { title, iconOnly = false, ...props } = defineProps<{
  title?: string
  variant?: 'outline' | 'bare' | 'filled' | 'outline-negative' | 'bare-negative' | 'link' | 'link-negative'
  size?: 'sm' | 'md' | 'lg'
  iconOnly?: boolean
  iconRight?: boolean
  icon?: string
  href?: string
  submit?: boolean
  target?: '_blank' | '_self'
}>()

const classes = computed(() => {
  const classes = []

  if (iconOnly) {
    classes.push('btn--icon-only')
  }

  if (props.size) {
    classes.push(`btn--${props.size}`)
  }

  if (props.variant) {
    classes.push(`btn--${props.variant}`)
  }

  if (props.iconRight) {
    classes.push('btn--icon-right')
  }
  else if (props.icon) {
    classes.push('btn--icon-left')
  }

  return classes
})

const tag = computed(() => {
  return props.href ? 'a' : 'button'
})

const linkTarget = computed(() => {
  return props.target ?? '_self'
})

const tagAttrs = computed(() => {
  if (props.href) {
    return {
      href: localePath(props.href),
      target: linkTarget.value,
      rel: linkTarget.value === '_blank' ? 'noopener noreferrer' : undefined,
    }
  }

  return {
    type: !props.submit ? 'button' : undefined,
  }
})
</script>

<style scoped>
/* Fixes styles being overridden. I could not figure out where */
.btn--outline {
  color: var(--color-primary-600) !important;
}

.btn--outline-negative {
  color: rgb(255 255 255 / var(--tw-text-opacity, 1));
}
a {
  text-decoration: none;
}
/* end fixes */
</style>
