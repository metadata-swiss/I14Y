<template>
  <ul
    :id="`accordion-${id}`"
    class="accordion"
    :class="spaced ? 'accordion--spaced' : ''"
  >
    <slot />
  </ul>
</template>

<script setup lang="ts">
defineProps({
  id: {
    type: String,
    required: true,
  },
  spaced: {
    type: Boolean,
    default: () => false,
  },
})
</script>
