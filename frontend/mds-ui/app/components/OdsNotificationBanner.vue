<template>
  <div
    :open="open || undefined"
    :class="['notification-banner', 'notification', `notification--${type}`]"
    :style="{
      transitionDuration: `${closeDuration}`,
    }"
  >
    <div
      class="notification-banner__wrapper"
      :style="{
        transitionDelay: `${closeDuration}`,
      }"
    >
      <p class="notification-banner__infos">
        <slot />
      </p>
      <slot name="buttons" />
    </div>
  </div>
</template>

<script setup lang="ts">
const { open = true, closeDuration = '0.3s' } = defineProps<{
  type: 'info' | 'success' | 'warning' | 'error'
  open?: boolean
  closeDuration?: string
}>()
</script>

<style scoped>
.notification-banner {
  display: block;
  transition-property: padding, opacity;
  transition-timing-function: ease-in-out, ease-in-out;
}
.notification-banner:not([open]) {
  padding: 0;
  opacity: 0;
  overflow: hidden;
}
.notification-banner__wrapper {
  transition-property: height;
  transition-duration: 0s;
}
.notification-banner:not([open]) .notification-banner__wrapper {
  height: 0;
}
</style>
