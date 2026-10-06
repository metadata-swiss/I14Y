<script setup lang="ts">
import OdsCard from './content/OdsCard.vue'
import SvgIcon from '~/components/SvgIcon.vue'

const { t } = useI18n()

const { toc } = defineProps({
  toc: {
    type: Object,
    required: true,
  },
})

const headings = ref<{ id: string, text: string }[]>([])
const activeHeadingId = ref<string | null>(null)

function updateActiveHeading() {
  const h2s = Array.from(document.querySelectorAll('#main-content h2[id]')) as HTMLElement[]
  if (!h2s.length) return
  const centerOffset = -100 // px, adjust for later switching
  const viewportCenter = window.scrollY + window.innerHeight / 2 + centerOffset

  const closestH2 = h2s[0]
  if (!closestH2) {
    activeHeadingId.value = null
    return
  }
  let closestId = closestH2.id
  let minDist = Math.abs(closestH2.getBoundingClientRect().top + window.scrollY - viewportCenter)
  for (const h2 of h2s) {
    const rect = h2.getBoundingClientRect()
    const h2Center = rect.top + window.scrollY + rect.height / 2
    const dist = Math.abs(h2Center - viewportCenter)
    if (dist < minDist) {
      minDist = dist
      closestId = h2.id
    }
  }
  activeHeadingId.value = closestId
}

onMounted(async () => {
  await nextTick()
  const mainContent = document.getElementById('main-content')
  if (mainContent) {
    const h2s = mainContent.querySelectorAll('h2[id]')
    headings.value = Array.from(h2s).map(h2 => ({
      id: h2.id,
      text: h2.textContent || '',
    }))
  }
  updateActiveHeading()
  window.addEventListener('scroll', updateActiveHeading, { passive: true })
})

onUnmounted(() => {
  window.removeEventListener('scroll', updateActiveHeading)
})
</script>

<template>
  <OdsCard :title="t('message.ods_toc.contents')">
    <ul class="menu text--sm">
      <li
        v-for="heading in toc.links"
        :key="heading.id"
      >
        <a
          :href="`#${heading.id}`"
          :class="['menu__item', 'menu__item--border', 'menu__item--condensed', { 'menu__item--active': heading.id === activeHeadingId }]"
        >
          <div>{{ heading.text }}</div>
          <SvgIcon
            icon="ArrowAngleBottomLeft"
            role="menu__item"
          />
        </a>
      </li>
    </ul>
  </OdsCard>
</template>

<style scoped>
h2 {
  font-size: unset;
  margin-top: unset;
}
</style>
