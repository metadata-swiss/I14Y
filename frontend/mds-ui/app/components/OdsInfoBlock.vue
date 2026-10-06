<script setup lang="ts">
const { title } = defineProps<{
  title: string
}>()
</script>

<template>
  <div
    class="info-block border-t"
    style="display: flex;
    flex-direction: column;"
  >
    <h3 class="info-block__title">
      {{ title }}
    </h3>
    <div>
      <slot />
    </div>
  </div>
</template>
