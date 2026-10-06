<template>
  <OdsButton
    variant="bare"
    :title="t('message.ods-list-card-toggle.card_view')"
    :aria-label="t('message.ods-list-card-toggle.card_view')"
    size="sm"
    icon="List"
    icon-only
    @click="updateListType('list')"
  />
  <OdsButton
    variant="bare"
    :title="t('message.ods-list-card-toggle.card_view')"
    :aria-label="t('message.ods-list-card-toggle.card_view')"
    size="sm"
    icon="Apps"
    icon-only
    @click="updateListType('card')"
  />
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import OdsButton from '../../OdsButton.vue'

const { t } = useI18n()

const props = defineProps<{ modelValue: 'card' | 'list' }>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: 'card' | 'list'): void
}>()

const listType = ref(props.modelValue)

watch(() => props.modelValue, (newVal) => {
  listType.value = newVal
})

function updateListType(type: 'card' | 'list') {
  if (listType.value !== type) {
    listType.value = type
    emit('update:modelValue', type)
  }
}
</script>
