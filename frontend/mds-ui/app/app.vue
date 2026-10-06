<template>
  <main id="main-content">
    <NuxtLayout>
      <Transition name="shrink-header">
        <OdsTopHeader
          v-if="!isMobileMenuOpen"
          enable-authentication
          @login="login"
          @logout="logout"
        />
      </Transition>
      <OdsHeader
        :navigation-items="navigationItems"
        @mobile-menu-state-change="mobileMenuOpened"
      />
      <Transition name="fade-content">
        <div
          v-if="!isMobileMenuOpen"
          style="min-height: calc(100dvh - 128px);"
        >
          <OdsNotificationBanner
            v-if="message"
            type="success"
          >
            {{ t(`message.${message}`) }}

            <template #buttons>
              <OdsButton
                variant="outline"
                :title="t('message.button.close')"
                icon-right
                icon="Checkmark"
                @click="closeMessages"
              />
            </template>
          </OdsNotificationBanner>
          <OdsNotificationBanner
            v-if="errorMessage"
            type="error"
          >
            {{ t(`message.${errorMessage}`) }}

            <template #buttons>
              <OdsButton
                variant="outline"
                :title="t('message.button.close')"
                icon-right
                icon="Checkmark"
                @click="closeErrorMessages()"
              />
            </template>
          </OdsNotificationBanner>
          <NuxtPage :page-key="route => route.path" />
        </div>
      </Transition>
      <OdsFooter />
      <OdsBottomFooter />
    </NuxtLayout>
  </main>
</template>

<script setup lang="ts">
import OdsTopHeader from './components/headers/OdsTopHeader.vue'
import OdsHeader from './components/headers/OdsHeader.vue'
import OdsBottomFooter from '@/components/footer/OdsBottomFooter.vue'
import OdsFooter from './components/footer/OdsFooter.vue'
import { useI18n } from '#imports'
import { useLocale as piveauLocale } from '@piveau/sdk-vue'
import { onMounted, ref } from 'vue'
import { useLoginWithRedirect } from '@/composables/login'
import { initMatomo } from '@certible/use-matomo'
import OdsNotificationBanner from '~/components/OdsNotificationBanner.vue'
import OdsButton from './components/OdsButton.vue'
import { useMessages, useAutoClearMessagesOnRouteChange } from '~/composables/messages'

const app = useNuxtApp()
const router = useRouter()
const config = useRuntimeConfig()
const { clear } = useUserSession()
const login = useLoginWithRedirect()

function logout() {
  clear()
  router.push('/logged-out')
}

const navigationItems = await useNavigationItems()
const isMobileMenuOpen = ref(false)

const { t, locale } = useI18n()

const { setLocale, currentLocale } = piveauLocale()

watch(locale, (newLocale) => {
  if (newLocale !== (currentLocale as Ref<string>).value) {
    setLocale(newLocale) // set the piveau locale
  }
}, { immediate: true },
)

onMounted(() => {
  const matomoHost = config.public.matomo.url
  const matomoSiteId = config.public.matomo.siteId

  if (matomoHost && matomoSiteId) {
    const matomo = initMatomo({
      host: matomoHost,
      siteId: matomoSiteId,
      trackRouter: true,
    })

    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-ignore
    app.vueApp.use(matomo)
  }
  else {
    console.warn('Matomo configuration missing: host and/or siteId')
  }
})

useHead({
  htmlAttrs: {
    lang: locale.value,
  },
  bodyAttrs: {
    class: 'body--ods-brand',
  },
  link: [
    { rel: 'icon', type: 'image/png', sizes: '96x96', href: '/favicon-96x96.png' },
    { rel: 'icon', type: 'image/svg+xml', href: '/favicon.svg' },
    { rel: 'shortcut icon', href: '/favicon.ico' },
    { rel: 'apple-touch-icon', sizes: '180x180', href: '/apple-touch-icon.png' },
    { rel: 'manifest', href: '/site.webmanifest' },
  ],
  meta: [
    { name: 'apple-mobile-web-app-title', content: 'opendata.swiss' },
    // you can add other default meta tags, e.g. description, viewport, etc
  ],
})

function mobileMenuOpened(value: boolean) {
  isMobileMenuOpen.value = value
}

function handleResize() {
  // Close the mobile menu if the window is resized to a width greater than or equal to 1024px
  if (window.innerWidth >= 1024) {
    isMobileMenuOpen.value = false
  }
}

const {
  message,
  errorMessage,
  closeMessages,
  closeErrorMessages,
} = useMessages()

useAutoClearMessagesOnRouteChange()

onMounted(() => {
  window.addEventListener('resize', handleResize)
  handleResize()
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
})
</script>

<style lang="scss" scoped>
.shrink-header-enter-active, .shrink-header-leave-active {
  transition: max-height 1.0s cubic-bezier(0.4,0,0.2,1);
  overflow: hidden;
}
.shrink-header-enter-from, .shrink-header-leave-to {
  max-height: 0 !important;
}
.shrink-header-enter-to, .shrink-header-leave-from {
  max-height: 300px;
}

.fade-content-enter-active, .fade-content-leave-active {
  transition: opacity 0.6s cubic-bezier(0.4,0,0.2,1);
}
.fade-content-enter-from, .fade-content-leave-to {
  opacity: 0;
}
.fade-content-enter-to, .fade-content-leave-from {
  opacity: 1;
}
</style>
