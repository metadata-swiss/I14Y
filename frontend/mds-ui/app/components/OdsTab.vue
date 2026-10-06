<template>
  <div
    role="tabpanel"
    class="tab__container"
  >
    <slot />
  </div>
</template>

<script setup lang="ts">
defineProps<{
  title: string
}>()
</script>

<style scoped>
:not([active]) {
  display: none;
}
</style>
