<template>
  <ul class="list list--flex list--wrap">
    <li
      v-for="c in props.tags"
      :key="c.id"
    >
      <OdsTagItem
        :id="c.id"
        :label="c.label"
        :icon="c.icon"
        :size="c.size"
        :variant="c.variant"
        @click="emitClickedTag(c)"
      />
    </li>
  </ul>
</template>

<script setup lang="ts">
import type { TagItem } from '../OdsTagItem.vue'
import OdsTagItem from '../OdsTagItem.vue'

interface Props {
  tags: TagItem[]
}

const props = defineProps<Props>()
const emit = defineEmits<{
  (e: 'tag-clicked', tag: TagItem): void
}>()

function emitClickedTag(e: TagItem) {
  emit('tag-clicked', e)
}
</script>
