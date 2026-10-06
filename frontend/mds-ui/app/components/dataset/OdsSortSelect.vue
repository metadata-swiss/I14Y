<template>
  <div class="form__group__select">
    <!---->
    <div class="select select--bare">
      <select
        v-model="selectedSort"
        class="input--outline input--sm"
        name="select-name"
      >
        <option :disabled="true" :value="''">
          {{ t('message.dataset_search.sort') }}
        </option>
        <option
          v-for="option in props.options"
          :key="option.value"
          :value="option.value"
        >
          {{ option.text }}
        </option>
      </select>
      <div class="select__icon">
        <svg role="presentation" aria-hidden="true" viewBox="0 0 24 24">
          <path d="m5.706 10.015 6.669 3.85 6.669-3.85.375.649-7.044 4.067-7.044-4.067z" />
        </svg>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from '#imports'

const { t } = useI18n()

export interface SortOption {
  value: string
  text: string
}
interface Props {
  options: SortOption[]
  modelValue?: string
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: '',
})

const emit = defineEmits(['update:modelValue'])

const selectedSort = computed({
  get: () => props.modelValue,
  set: (val: string) => emit('update:modelValue', val),
})
</script>
