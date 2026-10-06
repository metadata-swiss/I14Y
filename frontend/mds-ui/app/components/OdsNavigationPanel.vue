<template>
  <Transition
    :name="transitionName"
    mode="out-in"
  >
    <div :key="menuKey">
      <div id="mobile-menu">
        <nav
          aria-label="Main"
          class="main-navigation main-navigation--mobile"
        >
          <ul>
            <li
              v-if="menuStack.length > 0"
              style="border-bottom: none;"
              @click="goBack"
            >
              <a style="padding-top: 12px; padding-bottom: 12px;">
                <span>
                  <SvgIcon
                    icon="ArrowLeft"
                    size="lg"
                  />
                </span>
              </a>
            </li>
            <li
              v-if="menuStack.length > 0"
              style="border-bottom: none;"
            >
              <h2 class="navy__title">
                {{ parentLabel }}
              </h2>
            </li>

            <template
              v-for="item in currentMenu"
              :key="item.label"
            >
              <li v-if="item.to">
                <NuxtLinkLocale
                  :to="item.to"
                  active-class="active"
                  :aria-label="item.label"
                  @click="emitRequestClose"
                >
                  <span>{{ item.label }}</span>
                </NuxtLinkLocale><div />
              </li>
              <li
                v-else-if="(item.subMenu?.length ?? -1) > 0"
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
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import type { OdsNavTabItem } from './headers/model/ods-nav-tab-item'
import SvgIcon from '~/components/SvgIcon.vue'

const emit = defineEmits(['requestClose'])

interface Props {
  items: OdsNavTabItem[]
}

const props = defineProps<Props>()

function emitRequestClose() {
  emit('requestClose')
}

const parentLabel = ref<string | undefined>(undefined)

const menuStack = ref<OdsNavTabItem[][]>([])
const menuKey = computed(() => menuStack.value.length)
const currentMenu = computed(() => {
  if (menuStack.value.length === 0) return props.items
  return menuStack.value[menuStack.value.length - 1]
})
const direction = ref<'forward' | 'backward'>('forward')
const transitionName = computed(() => direction.value === 'forward' ? 'slide-menu-right' : 'slide-menu-left')
function drillDown(item: OdsNavTabItem) {
  if (item.subMenu && item.subMenu.length > 0) {
    direction.value = 'forward'
    menuStack.value.push(item.subMenu)
    parentLabel.value = item.label
  }
}
function goBack() {
  if (menuStack.value.length > 0) {
    direction.value = 'backward'
    menuStack.value.pop()
  }
}
</script>

<style scoped lang="scss">
// list styles

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

//

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
