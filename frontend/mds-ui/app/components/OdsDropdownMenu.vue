<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { useI18n } from '#imports'

import OdsButton from './OdsButton.vue'
import SvgIcon from './SvgIcon.vue'
import type { OdsNavTabItem } from './headers/model/ods-nav-tab-item'
import { NuxtLinkLocale } from '#components'

interface OdsDropdownMenuProps {
  label: string
  menu: OdsNavTabItem
  class?: string
}

const props = defineProps<OdsDropdownMenuProps>()

const { t } = useI18n()
const isOpen = ref(false)

const dropdownRef = ref<HTMLElement | null>(null)

function toggleDropdown() {
  isOpen.value = !isOpen.value
}

const parentLabelStack = ref<string[]>([props.menu.label])

const menuStack = ref<OdsNavTabItem[][]>([])

const menuKey = computed(() => menuStack.value.length)

const currentMenu = computed(() => {
  if (menuStack.value.length === 0) {
    return props.menu.subMenu ?? []
  }
  return menuStack.value[menuStack.value.length - 1]
})

const direction = ref<'forward' | 'backward'>('forward')

const transitionName = computed(() => direction.value === 'forward' ? 'slide-menu-right' : 'slide-menu-left')

function drillDown(item: OdsNavTabItem) {
  if (item.subMenu && item.subMenu.length > 0) {
    direction.value = 'forward'
    menuStack.value.push(item.subMenu)
    parentLabelStack.value.push(item.label)
  }
}

function goBack() {
  if (menuStack.value.length > 0) {
    direction.value = 'backward'
    menuStack.value.pop()
    parentLabelStack.value.pop()
  }
};

// Close dropdown when clicking outside
function handleClickOutside(event: MouseEvent) {
  if (dropdownRef.value && !dropdownRef.value.contains(event.target as Node)) {
    isOpen.value = false
  }
}

// start listening for clicks outside the dropdown
onMounted(() => {
  document.addEventListener('mousedown', handleClickOutside)
})

// stop listening when component is unmounted
onBeforeUnmount(() => {
  document.removeEventListener('mousedown', handleClickOutside)
})
</script>

<template>
  <a
    href="#"
    :class="props.class"
    @click.prevent="toggleDropdown"
    @mouseenter="isOpen = true"
  >
    <span class="activate-btn">{{ props.label }}</span>
  </a>
  <div
    ref="dropdownRef"
    class="ods-dropdown"
  >
    <div
      v-if="isOpen"
      id="desktop-menu__drawer"
      class="desktop-menu__drawer ods-drop-down-panel"
    >
      <div class="close-button-container">
        <SvgIcon
          v-if="menuStack.length > 0"
          icon="ArrowLeft"
          size="lg"
          @click.prevent="goBack"
        />
        <div v-else />
        <OdsButton
          icon="Cancel"
          :title="t('message.button.close_menu')"
          variant="bare"
          size="sm"
          icon-right
          class="ods-close"
          @click="isOpen = false"
        >
          <span class="ods-close">{{ t('message.button.close') }}</span>
        </OdsButton>
      </div>
      <Transition
        :name="transitionName"
        mode="out-in"
      >
        <div :key="menuKey">
          <div class="navy">
            <h2 class="navy__title">
              {{ parentLabelStack[parentLabelStack.length - 1] }}
            </h2>

            <nav class="navy__level-0 navy-menu__level-0">
              <ul>
                <template
                  v-for="item in currentMenu"
                  :key="item.label"
                >
                  <li v-if="item.to">
                    <NuxtLinkLocale
                      :to="item.to"
                      active-class="active"
                      :aria-label="item.label"
                      @click="isOpen = false"
                    >
                      <span>{{ item.label }}</span>
                    </NuxtLinkLocale>
                  </li>
                  <li
                    v-else
                    @click="drillDown(item)"
                  >
                    <a>{{ item.label }} <SvgIcon
                      size="lg"
                      icon="ArrowRight"
                    /></a>
                  </li>
                </template>
              </ul>
            </nav>
          </div>
        </div>
      </Transition>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.activate-btn {
  color: var(--color-primary-600);
}

.close-button-container {
  display: flex;
  justify-content: space-between;
}

.ods-close {
  white-space: nowrap;
  color: rgb(107 114 128 / var(--tw-text-opacity, 1));
}

.ods-hover:hover {
  background-color: var(--color-secondary-50);
  color: rgb(75 85 99 / var(--tw-text-opacity, 1));
}

.ods-drop-down-panel {
  min-width: 160px;
  position: absolute;
  top: -17px;
  left: 0;
  padding-left: 48px;
  padding-right: 48px;
  padding-bottom: 48px;
  padding-top: 24px;
  z-index: 100;
}
@media (min-width: 1280px) {
  .ods-drop-down-panel {
    top: -19px;
  }
}
@media (min-width: 1920px) {
  .ods-drop-down-panel {
    top: -21px;
  }
}
.ods-dropdown {
  position: relative;
  display: inline-block;
}

.ods-dropdown > a {
  cursor: pointer;
  display: flex;
  align-items: center;
}

.ods-dropdown__arrow {
  margin-left: 0.5em;
  font-size: 0.8em;
}

.ods-dropdown__menu {
  position: absolute;
  top: 100%;
  left: 0;
  background: white;
  border: 1px solid #ddd;
  min-width: 160px;
  z-index: 1000;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  padding: 0.5em 0;
}

.ods-dropdown__menu li {
  list-style: none;
}

.ods-dropdown__menu a {
  display: block;
  padding: 0.5em 1em;
  color: inherit;
  text-decoration: none;
}

.ods-dropdown__menu a:hover {
  background: #f5f5f5;
}

// list styles
.navy {
  position: unset !important;
  perspective: none !important;
}

.navy__level-0 {
  position: unset;
  ul {
    display: block;
  }
}
.navy__title {
  margin-top: 0;
  margin-left: 12px
}

li {
  border-bottom-width: 1px;
  font-size: 0.875rem;
  width: 100%;
  cursor: pointer;
  line-height: 24px;
  height: 46px;
  margin-top: 0 !important;

}
li:hover {
    background-color: var(--color-secondary-50);
}

#mobile-menu {
  width: 100vw;
}

ul {
  list-style: none !important;
  margin: 0 !important;
  padding: 0 !important;
}

a {
  text-decoration: none;
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 100%;
  padding-left: 16px;
  padding-right: 32px;
  color: black;
}

.submenu-arrow {
    float: right;
    font-size: 1.2em;
    margin-left: 8px;
}
.back-btn {
    font-weight: bold;
    color: #2ecc40;
    cursor: pointer;
}
.slide-menu-right-enter-active, .slide-menu-right-leave-active {
    transition: transform 0.3s cubic-bezier(0.4,0,0.2,1), opacity 0.3s cubic-bezier(0.4,0,0.2,1);
}
.slide-menu-right-enter-from {
    transform: translateX(100%);
    opacity: 0;
}
.slide-menu-right-enter-to {
    transform: translateX(0);
    opacity: 1;
}
.slide-menu-right-leave-from {
    transform: translateX(0);
    opacity: 1;
}
.slide-menu-right-leave-to {
    transform: translateX(-100%);
    opacity: 0;
}
.slide-menu-left-enter-active, .slide-menu-left-leave-active {
    transition: transform 0.3s cubic-bezier(0.4,0,0.2,1), opacity 0.3s cubic-bezier(0.4,0,0.2,1);
}
.slide-menu-left-enter-from {
    transform: translateX(-100%);
    opacity: 0;
}
.slide-menu-left-enter-to {
    transform: translateX(0);
    opacity: 1;
}
.slide-menu-left-leave-from {
    transform: translateX(0);
    opacity: 1;
}
.slide-menu-left-leave-to {
    transform: translateX(100%);
    opacity: 0;
}
</style>
