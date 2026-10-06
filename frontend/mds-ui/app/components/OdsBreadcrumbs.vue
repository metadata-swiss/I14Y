<script setup lang="ts">
import type { PropType } from 'vue'
import type { RouteLocationNamedI18n } from 'vue-router'
import SvgIcon from '~/components/SvgIcon.vue'

export interface BreadcrumbItem {
  id: string
  title: string
  route?: RouteLocationNamedI18n<string | symbol>
  path?: string
}

const { breadcrumbs } = defineProps({
  breadcrumbs: {
    type: Array as PropType<BreadcrumbItem[]>,
    required: true,
  },
})
</script>

<template>
  <div
    id="breadcrumb"
    class="breadcrumb container container--flex"
  >
    <nav class="breadcrumb-navigation">
      <ul>
        <li
          v-for="(item, index) in breadcrumbs"
          :key="item.id"
        >
          <NuxtLinkLocale :to="item.route || item.path">
            <SvgIcon
              v-if="index > 0"
              icon="ChevronRight"
              class="breadcrumb__include-icon"
              aria-hidden="true"
            />
            <span>{{ item.title }}</span>
          </NuxtLinkLocale>
        </li>
      </ul>
    </nav>
  </div>
</template>

<style scoped>
li {
  margin: 0 !important;
}
</style>
