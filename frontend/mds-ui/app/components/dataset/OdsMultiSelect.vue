<template>
  <OdsFormField
    :for="getUniqueId('multi-select')"
    :label="label"
    type="select"
    :required="required"
  >
    <div :class="selectWrapperClasses">
      <VSelect
        :id="getUniqueId('multi-select')"
        v-model="selected"
        :multiple="multiple"
        :placeholder="placeholder"
        :class="selectClasses"
        :disabled="disabled"
        :components="{ Deselect, OpenIndicator }"
        :options="options"
        :name="name"
        :taggable="taggable"
        :no-drop="taggable"
        label="title"
        :filterable="!!loadOptions"
        :close-on-select="closeOnSelect"
        @search="loadOptions!"
      >
        <template #no-options>
          <slot name="no-options" />
        </template>
        <!-- Workaround for required validation -->
        <template #search="{ attributes, events }">
          <input
            class="vs__search"
            :required="
              required && (!selected || selected.length === 0)
            "
            v-bind="attributes as any"
            @keydown="onSearchKeydown"
            v-on="events"
          >
        </template>
        <template #option="option">
          <slot
            name="option"
            v-bind="option"
          />
        </template>
        <template #selected-option="option">
          <slot
            name="selected-option"
            v-bind="option"
          />
        </template>
      </VSelect>
      <div class="select__icon">
        <svg
          role="presentation"
          aria-hidden="true"
          viewBox="0 0 24 24"
        >
          <path
            d="m5.706 10.015 6.669 3.85 6.669-3.85.375.649-7.044 4.067-7.044-4.067z"
          />
        </svg>
      </div>
    </div>
    <div
      v-if="message"
      class="badge badge--sm"
      :class="`badge--${messageType}`"
    >
      {{ message }}
    </div>
  </OdsFormField>
</template>

<script setup lang="ts">
import VSelect from 'vue-select'
import { ref, computed, onMounted, h } from 'vue'
import { v4 as uuidv4 } from 'uuid'
import OdsFormField from '~/components/OdsFormField.vue'

const selectId = ref('')
const selected = defineModel<unknown[]>({
  default: () => [],
})
const Deselect = { render: () => h('span', '×') }
const OpenIndicator = { render: () => h('span') }

const props = defineProps({
  loadOptions: {
    type: Function,
    default: () => undefined,
  },
  closeOnSelect: {
    type: Boolean,
    default: () => true,
  },
  bare: {
    type: Boolean,
    default: () => false,
  },
  variant: {
    type: String,
    validator: prop => ['outline', 'negative'].includes(prop as string),
    default: () => 'outline',
  },
  size: {
    type: String,
    validator: prop => ['sm', 'base', 'lg'].includes(prop as string),
    default: () => 'base',
  },
  disabled: {
    type: Boolean,
    default: () => false,
  },
  hideLabel: {
    type: Boolean,
    default: () => false,
  },
  required: {
    type: Boolean,
    default: () => false,
  },
  label: {
    type: String,
    default: () => undefined,
  },
  name: {
    type: String,
    default: () => undefined,
  },
  message: {
    type: String,
    default: () => undefined,
  },
  messageType: {
    type: String,
    validator: prop =>
      ['error', 'warning', 'success', 'info'].includes(prop as string),
    default: () => undefined,
  },
  options: {
    type: Array<unknown>,
    default: () => [],
  },
  multiple: {
    type: Boolean,
    default: () => true,
  },
  placeholder: {
    type: String,
    default: () => undefined,
  },
  excluded: {
    type: Array<string>,
    default: () => [],
  },
  selectLimit: {
    type: Number,
    default: () => undefined,
  },
  taggable: {
    type: Boolean,
    default: () => false,
  },
})

const selectWrapperClasses = computed(() => {
  const classes = ['select', 'shadow-lg']
  if (props.bare) {
    classes.push('select--bare')
  }
  return classes.join(' ')
})

const selectClasses = computed(() => {
  const classes = []

  if (props.variant) {
    classes.push(`input--${props.variant}`)
  }
  if (props.size) {
    classes.push(`input--${props.size}`)
  }
  if (props.disabled) {
    classes.push('input--disabled')
  }
  if (props.messageType) {
    classes.push(`input--${props.messageType}`)
  }

  return classes.join(' ')
})

const getUniqueId = function (text = '') {
  return `${text}-${selectId.value}`
}

function onSearchKeydown(event: KeyboardEvent) {
  if (event.key === 'Enter') {
    nextTick(() => {
      (event.target as HTMLInputElement).focus()
    })
  }
}

onMounted(() => {
  selectId.value = uuidv4()
})
</script>

<style scoped>
input.vs__selected {
  cursor: default;
}
</style>
