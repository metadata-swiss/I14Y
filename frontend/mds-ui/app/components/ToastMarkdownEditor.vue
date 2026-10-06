<script setup>
import Editor from '@toast-ui/editor'
import '@toast-ui/editor/dist/toastui-editor.css'
import OdsFormField from '~/components/OdsFormField.vue'

const { autofocus } = defineProps({
  id: {
    type: String,
    default: '',
  },
  label: {
    type: String,
    required: false,
    default: '',
  },
  required: {
    type: Boolean,
    default: false,
  },
  modelValue: {
    type: String,
    required: false,
    default: '',
  },
  autofocus: {
    type: Boolean,
    default: false,
  },
})
const emit = defineEmits(['update:modelValue'])
const editor = ref()

const value = ref('')

onMounted(() => {
  const e = new Editor({
    el: editor.value,
    height: '500px',
    initialEditType: 'wysiwyg',
    previewStyle: 'vertical',
    autofocus,
    events: {
      change: () => {
        const markdown = e.getMarkdown()
        value.value = markdown
        emit('update:modelValue', markdown)
      },
    },
  })
})
</script>

<template>
  <OdsFormField :for="id" :label="label" :required="required">
    <div ref="editor" />
    <input v-if="id" :name="id" type="hidden" :value="value" :required="required">
  </OdsFormField>
</template>
