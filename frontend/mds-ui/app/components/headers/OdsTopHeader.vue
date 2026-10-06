<script lang="ts" setup>
import LanguageSelector from '../LanguageSelector.vue'
import OdsButton from '../OdsButton.vue'

import { useI18n } from '#imports'

const { t } = useI18n()

interface Props {
  enableAuthentication?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  enableAuthentication: false,
  authenticated: false,
})

const { loggedIn, user } = useUserSession()

declare module '#auth-utils' {
  interface User {
    name: string
  }
}

const emit = defineEmits<{
  (e: 'login' | 'logout'): void
}>()
</script>

<template>
  <header
    id="top-bar-container"
    class="top-bar"
  >
    <div
      id="top-bar"
      class="top-bar__bar"
    >
      <div class="container container--flex ">
        <div class="top-bar__btn" />
        <div class="top-bar__right">
          <div v-if="props.enableAuthentication">
            <OdsButton
              v-if="loggedIn"
              icon="Logout"
              variant="bare-negative"
              size="sm"
              :title="t('message.top_header.logout')"
              :aria-label="t('message.top_header.logout')"
              icon-right
              @click="emit('logout')"
            >
              <span>{{ user ? user.name : t('message.top_header.logout') }}</span>
            </OdsButton>

            <OdsButton
              v-else
              icon="Login"
              variant="bare-negative"
              size="sm"
              :title="t('message.top_header.login')"
              :aria-label="t('message.top_header.login')"
              icon-right
              @click="emit('login')"
            >
              <span>{{ t('message.top_header.login') }}</span>
            </OdsButton>
          </div>

          <div class="language-selector">
            <LanguageSelector />
          </div>
        </div>
      </div>
    </div>
  </header>
</template>

<style lang="scss" scoped>

</style>
