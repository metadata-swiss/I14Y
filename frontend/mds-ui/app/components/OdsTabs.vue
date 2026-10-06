<template>
  <div ref="root" class="tabs">
    <div class="tab__controls-container">
      <div class="tab__controls" role="tablist" aria-label="Additional Content">
        <button
          v-for="(tab, index) in tabs" :key="tab.name"
          :class="`tab__control ${activeIndex === index ? 'tab__control--active' : ''}`"
          type="button"
          role="tab"
          aria-selected="true"
          :aria-controls="`tab-${index}`"
          @click="setActiveIndex(index)"
        >
          {{ tab.label }}
        </button>
      </div>
    </div>

    <slot />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

interface Tab {
  vnode: VNode
  name: string
  label: string
}

const root = ref<HTMLElement | null>(null)

const tabs = computed(() => {
  return getCurrentInstance()?.slots.default?.()
    ?.map((vnode, index) => {
      const props = vnode.props || {}
      return {
        vnode,
        name: `tab-${index}`,
        label: props.title || `Tab ${index + 1}`,
      }
    })
    .filter(tab => tab !== null) as Tab[]
})

const activeIndex = ref(0)

const setActiveIndex = function (index: number) {
  activeIndex.value = index
}

watch(activeIndex, (current, previous) => {
  const tabsElements = [...root.value?.querySelectorAll('[role=tabpanel]') || []]

  tabsElements[previous]?.removeAttribute('active')
  tabsElements[current]?.setAttribute('active', 'active')
})

onMounted(() => {
  [...root.value?.querySelectorAll('[role=tabpanel]') || []][0]?.setAttribute('active', 'active')
})
</script>

<style scoped>
.tabs [role=tabpanel] {
  display: none;
}
</style>
