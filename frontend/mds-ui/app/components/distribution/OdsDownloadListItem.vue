<template>
  <div>
    <a
      class="download-item"
      :href="props.url"
      target="_blank"
    >
      <SvgIcon
        :icon="icon"
        size="xl"
        class="download-item__icon"
      />
      <div>
        <h2 class="download-item__title">{{ props.name }}</h2>
        <p class="meta-info download-item__meta-info">
          <span
            v-if="props.format"
            class="meta-info__item"
          >{{ props.format }}</span>
          <span
            v-if="props.byteSize"
            class="meta-info__item dont-break-word"
          >{{ props.byteSize }}</span>
          <span
            v-for="(language, index) in props.languages ?? []"
            :key="`${language}-${index}`"
            class="meta-info__item"
            style="white-space: nowrap;"
          >
            {{ language }}
          </span>
        </p>
      </div>
    </a>
  </div>
</template>

<script setup lang="ts">
import SvgIcon from '../SvgIcon.vue'

interface OdsDownloadListItemProps {
  url: string
  name: string
  format?: string
  languages?: string[]
  byteSize?: string
  icon: string
}

const props = defineProps<OdsDownloadListItemProps>()
</script>

<style scoped lang="scss">
  a {
    overflow-x: hidden;
    word-break: break-word;
  }

  .download-item__title {
    text-overflow: hidden;
    overflow-x: hidden;
    word-break: break-word;
  }

  .dont-break-word {
    white-space: nowrap;
  }
</style>
