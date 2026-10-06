<template>
  <div class="ods-distribution-list">
    <div
      v-if="props.distributions && props.distributions.length > 1"
      class="ods-distribution-list__header"
    >
      <OdsSortSelect
        v-model="sortBy"
        :options="sortOptions"
        class="sort-select"
      />
    </div>
    <div class="ods-distribution-list__items">
      <OdsDistributionListItem
        v-for="dist in sortedDistributions"
        :key="dist.id"
        :distribution="dist"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useI18n } from '#imports'
import OdsSortSelect from '../dataset/OdsSortSelect.vue'
import OdsDistributionListItem from './OdsDistributionListItem.vue'
import type { DcatApChV2DistributionAdapter } from './model/dcat-ap-ch-v2-distribution-adapter'

interface Props {
  distributions: DcatApChV2DistributionAdapter[]
}

const props = defineProps<Props>()
const { locale, t } = useI18n()

const sortBy = ref('name_asc')

const sortOptions = computed(() => [
  { value: 'name_asc', text: t('message.dataset_search.sort_by.title_asc') },
  { value: 'name_desc', text: t('message.dataset_search.sort_by.title_desc') },
])

const sortedDistributions = computed(() => {
  const list = [...props.distributions]
  list.sort((a, b) => {
    const titleA = a.title || ''
    const titleB = b.title || ''
    const cmp = titleA.localeCompare(titleB, locale.value)
    return sortBy.value === 'name_asc' ? cmp : -cmp
  })
  return list
})
</script>

<style lang="scss" scoped>
.ods-distribution-list {
  display: flex;
  flex-direction: column;
}
.ods-distribution-list__header {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
  border-bottom-width: 1px;
  border-bottom-style: solid;
  border-color: var(--color-secondary-200);
}
.ods-distribution-list__items {
  display: flex;
  flex-direction: column;
}
.sort-select {
  width: inherit;

}
</style>
