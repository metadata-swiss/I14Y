<template>
  <fieldset class="form__group">
    <legend class="form__group__legend {{ required ? 'text--asterisk' : '' }}">
      {{ legend }}
      <span
        v-if="required"
        class="sr-only"
      >required</span>
    </legend>
    <slot />
  </fieldset>
</template>

<script setup lang="ts">
defineProps<{
  legend: string
  required?: boolean
}>()
</script>
