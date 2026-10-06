<template>
  <li class="accordion__item">
    <button
      :id="`accordion-control-${id}`"
      class="accordion__button"
      :class="{ active: isOpen }"
      :aria-expanded="isOpen"
      :aria-controls="`content-${id}`"
      @click="toggle"
    >
      <component
        :is="tag"
        class="accordion__title"
      >
        {{ title }}
      </component>
      <SvgIcon
        icon="ChevronDown"
        size="xl"
        class="accordion__arrow"
      />
    </button>
    <div
      :id="`content-${id}`"
      ref="drawer"
      class="accordion__drawer"
      :aria-hidden="!isOpen"
      :style="[drawerStyle, { overflow: 'hidden', transition: 'max-height 0.3s ease-out' }]"
    >
      <div class="accordion__content vertical-spacing">
        <slot />
      </div>
    </div>
  </li>
</template>

<script setup lang="ts">
import { computed, ref, watch, onMounted } from 'vue'
import SvgIcon from './SvgIcon.vue'

const { headingLevel = 'div', ...props } = defineProps<{
  id: string
  title: string
  headingLevel?: 'h1' | 'h2' | 'h3' | 'h4' | 'div'
  open?: boolean
}>()

const emit = defineEmits<{
  (e: 'update:open', value: boolean): void
}>()

const isOpen = ref(props.open || false)
const drawer = ref<HTMLElement | null>(null)

watch(() => props.open, (val) => {
  if (val !== undefined) {
    isOpen.value = val
  }
})

const tag = computed(() => {
  return headingLevel
})

const drawerStyle = computed(() => {
  if (isOpen.value && drawer.value) {
    return {
      maxHeight: `${drawer.value.scrollHeight}px`,
    }
  }
  return {
    maxHeight: '0px',
  }
})

function getFocusableElements(element: HTMLElement) {
  return [
    ...element.querySelectorAll(
      'a[href], button, input, textarea, select, details, [tabindex]:not([tabindex="-1"])',
    ),
  ].filter(
    el => !el.hasAttribute('disabled') && !el.getAttribute('aria-hidden'),
  ).filter(el => 'tabIndex' in (el as HTMLElement)) as HTMLElement[]
}

function updateFocusability() {
  if (!drawer.value) return
  const focusableElements = getFocusableElements(drawer.value)
  focusableElements.forEach((item) => {
    item.tabIndex = isOpen.value ? 0 : -1
  })
}

function toggle() {
  isOpen.value = !isOpen.value
  emit('update:open', isOpen.value)
}

watch(isOpen, () => {
  updateFocusability()
})

onMounted(() => {
  updateFocusability()
})
</script>
