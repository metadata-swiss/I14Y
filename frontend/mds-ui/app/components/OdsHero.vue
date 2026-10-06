<template>
  <h1
    v-if="slots.title && type == 'sr-only'"
    class="sr-only"
  >
    <slot name="type" />
    <slot name="date" />
    <slot name="title" />
    <slot name="description" />
  </h1>
  <section
    v-else
    :class="computedClasses"
  >
    <div class="container container--grid gap--responsive">
      <div class="hero__content">
        <p
          v-if="slots['#meta-info']"
          class="meta-info"
        >
          <slot name="meta-info" />
        </p>
        <h1
          v-if="slots.title"
          class="hero__title"
        >
          <slot name="title" />
        </h1>
        <h2
          v-if="slots.subtitle"
          class="hero__subtitle"
        >
          <slot name="subtitle" />
        </h2>
        <div
          v-if="slots.description"
          class="hero__description"
        >
          <slot name="description" />
        </div>
        <div
          v-if="slots.cta"
          class="hero__cta"
        >
          <slot name="cta" />
        </div>
        <aside
          v-if="slots.authors"
          class="authors"
        >
          <slot name="authors" />
        </aside>
      </div>
      <div
        v-if="slots.image"
        class="hero__image"
      >
        <slot name="image" />
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, useSlots } from 'vue'

const slots = useSlots()

const props = defineProps({
  type: {
    type: String,
    validator: prop =>
      [
        'default',
        'main',
        'main-image',
        'hub',
        'title-only',
        'sr-only',
        'overview',
      ].includes(prop as string),
    default: () => undefined,
  },
  floating: {
    type: Boolean,
    default: false,
  },
})

const computedClasses = computed(() => {
  let base = 'hero '
  if (props.type) base += `hero--${props.type} `
  if (props.floating) base += 'hero--floating '
  return base
})
</script>

<style lang="scss" scoped>
@use '@/assets/ods/ods_breakpoints.scss' as media;

.hero--floating {
  position: relative;
  z-index: 1;
  margin-top: 0;
  padding-top: 0 !important;

  @include media.respond-to-md {
    margin-top: 0px;

    .hero__content {
      background-color: rgb(255, 255, 255);
      padding: 1.5rem;
    }
  }

  @include media.respond-to-lg {
    margin-top: -70px;

    .hero__content {
      background-color: rgb(255, 255, 255);
      padding: 2rem;
    }
  }
}
</style>
