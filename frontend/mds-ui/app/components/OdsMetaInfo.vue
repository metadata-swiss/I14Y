<template>
  <span class="meta-info__item">
    <slot>
      {{ text }}
    </slot>
  </span>
</template>

<script setup lang="ts">
defineProps<{
  text?: string
}>()
</script>
