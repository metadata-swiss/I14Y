<template>
  <OdsDetailTableInfoBlock
    v-for="entry in props.tableEntries"
    :key="entry.label"
    :title="entry.label"
    :type="props.type"
  >
    <OdsDetailsTableValue
      v-for="(data, index) in entry.value"
      :key="index"
      :value="data"
    />
    <div
      v-if="entry.subFields.length > 0"
    >
      <OdsDetailsTable
        :table-entries="entry.subFields"
        :type="entry.blockType()"
      />
    </div>
  </OdsDetailTableInfoBlock>
</template>

<script setup lang="ts">
import OdsDetailTableInfoBlock from './OdsDetailTableInfoBlock.vue'
import type { OdsTableEntry } from './model/table-entry'
import OdsDetailsTableValue from './OdsDetailsTableValue.vue'

interface OdsDetailsTableProps {
  tableEntries: OdsTableEntry[]
  type: 'row' | 'block' | 'tree'
}

const props = defineProps<OdsDetailsTableProps>()
</script>
