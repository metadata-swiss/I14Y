<template>
  <div :class="computedClass">
    <label for="lang-switcher" class="sr-only">Select language</label>
    <Select

      id="lang-switcher"
      v-model="selectedLocale"
      size="sm"
      bare
      :variant="type"
    >
      <option
        v-for="availableLocale in locales"
        :key="availableLocale.code"
        :value="availableLocale.code"
        :selected="selectedLocale == availableLocale.code">
        {{ availableLocale.code.toUpperCase() }}
      </option>
    </Select>
  </div>
</template>

<script setup lang="ts">
import Select from './OdsSelect.vue'
import { ref, computed } from 'vue'
import { useI18n } from '#imports'

const switchLocalePath = useSwitchLocalePath()
const { locale, locales } = useI18n()

// Props
defineProps({
  type: {
    type: String,
    default: () => 'negative',
  },
})

// CSS class
const computedClass = computed(() => 'language-switcher')

// Reactive selected locale (safe for hydration)
const selectedLocale = ref(locale.value)

// Handle language switching
watch(selectedLocale, (newLocale) => {
  if (newLocale !== locale.value) {
    const newPath = switchLocalePath(newLocale)
    navigateTo(newPath)
  }
})

useLoadingIndicator()
</script>
