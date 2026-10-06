<script setup lang="ts">
import OdsButton from '../OdsButton.vue'

const { title, type = 'default', clickable = false, href } = defineProps<{
  title: string
  type?: 'default' | 'highlight' | 'twitter' | 'flat' | 'universal' | 'list'
  clickable?: boolean
  href?: string
}>()

const classes = computed(() => {
  return {
    'card': true,
    [`card--${type}`]: true,
    'card--clickable': clickable,
  }
})
</script>

<template>
  <div :class="classes">
    <div
      v-if="$slots.image && type !== 'highlight' && type !== 'universal'"
      class="card__image"
    >
      <slot name="image" />
    </div>
    <div class="card__content">
      <div class="card__body">
        <div
          v-if="$slots['top-meta']"
          class="meta-info"
        >
          <slot name="top-meta" />
        </div>
        <slot name="title">
          <div class="card__title">
            <h3>{{ title }}</h3>
          </div>
        </slot>
        <div
          v-if="$slots.image && type === 'universal'"
          class="card__image"
        >
          <slot name="image" />
        </div>
        <slot />

        <div
          v-if="$slots['bottom-meta']"
          class="meta-info"
        >
          <slot name="bottom-meta" />
        </div>
        <div
          v-if="$slots.icons"
          class="card__content-icons"
        >
          <slot name="icons" />
        </div>
      </div>
      <div class="card__footer">
        <div class="card__footer__info">
          <div
            v-if="$slots['footer-info']"
            class="meta-info"
          >
            <slot name="footer-info" />
          </div>
        </div>
        <div
          v-if="$slots['footer-action'] || href"
          class="card__footer__action"
        >
          <slot name="footer-action">
            <OdsButton
              v-if="href"
              :href="href"
              icon-only
              icon="ArrowRight"
              variant="outline"
            />
          </slot>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.card {
  height: fit-content;
}
</style>
