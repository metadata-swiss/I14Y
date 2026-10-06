<template>
  <Icon
    :name="iconName"
    :class="['icon', 'icon--base', ...iconClasses]"
  />
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface PropTypes {
  icon: string
  size?: 'sm' | 'base' | 'md' | 'lg' | 'xl' | '2xl' | '3xl' | '4xl' | 'full'
  spin?: boolean
  role?: 'btn' | 'menu__item' | 'navigation' | 'presentation'
}

const props = defineProps<PropTypes>()

const iconClasses = computed(() => {
  function* generate() {
    if (props.size) yield `icon--${props.size}`
    if (props.icon) yield `icon--${props.icon}`
    if (props.spin) yield 'icon--spin'
    if (props.role) yield `${props.role}__icon`
  }

  return [...generate()]
})

const iconName = computed(() => {
  const name = props.icon ? `ods:${props.icon}` : ''
  return name
})
</script>
