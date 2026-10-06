<template>
  <section :class="sectionClasses">
    <div class="container">
      <h2
        v-if="title"
        class="section__title"
      >
        <slot name="title">
          {{ title }}
        </slot>
      </h2>
      <div
        v-if="layout === 'slideshow'"
        class="carousel carousel--cards"
      >
        <swiper-container
          ref="swiperEl"
          init="false"
        >
          <template
            v-for="vnode in slideshowChildren"
            :key="vnode.key ?? undefined"
          >
            <component :is="vnode" />
          </template>
        </swiper-container>
        <div class="carousel__fonctions">
          <div
            :id="`carousel-pagination-${id}`"
            class="carousel__pagination"
          />
          <button
            :id="`carousel-prev-${id}`"
            class="carousel__prev"
          >
            <div class="sr-only">
              Previous image
            </div>
            <SvgIcon
              icon="ChevronLeft"
              role="presentation"
              aria-hidden="true"
            />
          </button>
          <button
            :id="`carousel-next-${id}`"
            class="carousel__next"
          >
            <div class="sr-only">
              Next image
            </div>
            <SvgIcon
              icon="ChevronRight"
              role="presentation"
              aria-hidden="true"
            />
          </button>
        </div>
      </div>
      <div
        v-else
        :class="`grid ${layout} gap--responsive`"
      >
        <slot />
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import type { SwiperContainer } from 'swiper/element'
import type { AutoplayOptions, SwiperOptions } from 'swiper/types'
import SvgIcon from '../SvgIcon.vue'
import { Fragment, isVNode, type VNodeChild, Comment, onMounted, ref } from 'vue'

export type SectionLayout = 'grid--items-1' | 'grid--items-2' | 'grid--items-3' | 'grid--items-4' | 'grid--items-5' | 'grid--responsive-cols-2' | 'grid--responsive-cols-3' | 'grid--responsive-cols-4' | 'slideshow'

type SlideshowOptions = Omit<SwiperOptions, 'autoplay'> & {
  autoplay?: AutoplayOptions & {
    enabled: boolean
  }
}

const {
  id,
  accentColor = '200',
  textColor = '400',
  layout = 'grid--responsive-cols-3',
  slideshowOptions,
} = defineProps<{
  id?: string
  title?: string
  layout?: SectionLayout
  accentColor?: '50' | '100' | '200' | '300' | '400' | '500' | '600' | '700' | '800' | '900'
  textColor?: '50' | '100' | '200' | '300' | '400' | '500' | '600' | '700' | '800' | '900'
  slideshowOptions?: SlideshowOptions
}>()

const { autoplay, speed = 500 } = slideshowOptions || {}

const sectionClasses = computed(() => {
  if (layout === 'slideshow') {
    return 'section section-default'
  }

  return `section section--default accent-${accentColor} text-${textColor}`
})

const breakpoints = {
  480: {
    slidesPerView: 1,
    spaceBetween: 28,
  },
  640: {
    slidesPerView: 1,
    spaceBetween: 36,
  },
  768: {
    slidesPerView: 2,
    slidesPerGroup: 2,
    spaceBetween: 36,
  },
  1024: {
    slidesPerView: 3,
    slidesPerGroup: 3,
    spaceBetween: 40,
  },
  1280: {
    slidesPerView: 3,
    slidesPerGroup: 3,
    spaceBetween: 48,
  },
  1800: {
    slidesPerView: 3,
    slidesPerGroup: 3,
    spaceBetween: 64,
  },
}

const slots = useSlots()

// Reference to the web component instance
const swiperEl = ref<HTMLElement | null>(null)

function flatten(children: VNodeChild[]): VNode[] {
  const out: VNode[] = []
  for (const c of children ?? []) {
    if (c == null || c === '' || c === false) continue
    if (!isVNode(c)) continue
    if (c.type === Comment) continue
    if (c.type === Fragment && Array.isArray(c.children)) out.push(...flatten(c.children))
    else out.push(c)
  }
  return out
}

const slideshowChildren = computed(() => {
  const raw = slots.default?.() ?? []
  return flatten(raw).map((v, i) =>
    h('swiper-slide', { key: v.key ?? `auto-${i}` }, { default: () => [v] }),
  )
})

// Initialize Swiper Web Component on the client with object options via properties
onMounted(() => {
  const el = swiperEl.value as SwiperContainer
  if (!el) return
  console.debug('[OdsSection] SSR children before init:',
    Array.from(el.children).map(n => ({ tag: n.tagName, html: n.innerHTML.slice(0, 120) + '…' })))
  const autoplayProp = autoplay?.enabled ? { delay: (autoplay.delay || 2.5) * 1000 } : false
  Object.assign(el, {
    speed,
    autoHeight: false,
    loop: false,
    slidesPerView: 1,
    spaceBetween: 20,
    breakpoints,
    simulateTouch: true,
    slideToClickedSlide: false,
    keyboard: {
      enabled: true,
      onlyInViewport: false,
    },
    navigation: {
      nextEl: `#carousel-next-${id}`,
      prevEl: `#carousel-prev-${id}`,
    },
    pagination: {
      type: 'bullets',
      el: `#carousel-pagination-${id}`,
      clickable: true,
      bulletClass: 'carousel__bullet',
      bulletActiveClass: 'carousel__bullet--active',
    },
    autoplay: autoplayProp,
  })

  if (typeof el.initialize === 'function') {
    el.initialize()
  }
})
</script>
