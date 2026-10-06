<template>
  <span v-if="value.nodeType === odsTableEntryType.String">{{ props.value.label }}&nbsp;</span>
  <span v-if="value.nodeType === odsTableEntryType.Date"><OdsRelativeDateToggle :date="new Date(props.value.label)" /></span>
  <span v-if="value.nodeType === odsTableEntryType.Href">
    <a
      :href="props.value.id"
      target="_blank"
      rel="noopener noreferrer"
      class="link--external"
    >{{ props.value.label }}
    </a>
  </span>
  <span
    v-if="value.nodeType === odsTableEntryType.Email"
    class="line"
  >
    <a :href="`mailto:${props.value.label}`">&lt;{{ props.value.label }}&gt;</a>
  </span>
  <span
    v-if="value.nodeType === odsTableEntryType.Telephone"
    class="line"
  ><a :href="`tel:${props.value.label}`">{{ props.value.label }}</a>
  </span>
</template>

<script setup lang="ts">
import { OdsTableEntryType, type OdsTableEntry } from './model/table-entry'
import OdsRelativeDateToggle from '../OdsRelativeDateToggle.vue'

interface OdsDetailsTableValueProps {
  value: OdsTableEntry
}

const props = defineProps<OdsDetailsTableValueProps>()

// Make enum available in template
const odsTableEntryType = OdsTableEntryType
</script>

<style lang="scss">
  .line {
    display: flex;
    align-items: center;
    flex-direction: row;
  }
</style>
