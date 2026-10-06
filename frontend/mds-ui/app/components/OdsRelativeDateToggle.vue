<template>
  <NuxtTime
    :datetime="props.date"
    :locale="locale"
    :relative="showRelative"
    :title="showRelative ? t('message.dataset_detail.show_raw') : t('message.dataset_detail.show_relative')"
    :aria-label="showRelative ? t('message.dataset_detail.show_raw') : t('message.dataset_detail.show_relative')"
    :year="'numeric'"
    :month="'2-digit'"
    :day="'2-digit'"
    class="date-toggle"
    @click="toggle($event)"
  />
</template>

<script setup lang="ts">
import { useI18n } from '#imports'

const { locale, t } = useI18n()
const showRelative = ref(false)

interface Props {
  date: Date
}

const props = defineProps<Props>()

function toggle(event: PointerEvent) {
  event.preventDefault()
  event.stopPropagation()
  showRelative.value = !showRelative.value
}
</script>

<style lang="scss" scoped>
.date-toggle {
  cursor: pointer;
}
</style>
