<template>
  <div class="app-layout">
    <OdsTopHeader @login="login" />
    <OdsHeader :navigation-items="navigationItems" />
    <main
      id="main-content"
      style="min-height: calc(100dvh - 128px);"
    >
      <OdsPage :hero="{ title: notFound ? t('message.error_page.not_found_title') : t('message.error_page.unavailable_title') }">
        <template #hero-content>
          <p>
            {{ notFound ? t('message.error_page.not_found_text') : t('message.error_page.unavailable_text') }}
          </p>
        </template>
        <section class="section section--py">
          <div class="container">
            <p class="error-page__links">
              <OdsButton
                variant="outline"
                @click="goTo('/')"
              >
                {{ t('message.error_page.home') }}
              </OdsButton>
              <OdsButton
                variant="outline"
                @click="goTo('/datasets')"
              >
                {{ t('message.error_page.search') }}
              </OdsButton>
            </p>
          </div>
        </section>
      </OdsPage>
    </main>
    <OdsFooter />
    <OdsBottomFooter />
  </div>
</template>

<script setup lang="ts">
import type { NuxtError } from '#app'
import OdsTopHeader from './components/headers/OdsTopHeader.vue'
import OdsHeader from './components/headers/OdsHeader.vue'
import OdsFooter from './components/footer/OdsFooter.vue'
import OdsBottomFooter from './components/footer/OdsBottomFooter.vue'
import OdsPage from './components/OdsPage.vue'
import OdsButton from './components/OdsButton.vue'

// Shown instead of app.vue when a page throws, for example through ensureFound (app/utils/ensureFound.ts).
const { error } = defineProps<{ error: NuxtError }>()

const { t, locale } = useI18n()
const localePath = useLocalePath()
const config = useRuntimeConfig()

const notFound = computed(() => error.statusCode === 404)
const navigationItems = await useNavigationItems()

function login() {
  navigateTo(config.public.loginUrl, { external: true })
}

function goTo(path: string) {
  clearError({ redirect: localePath(path) })
}

useHead({
  htmlAttrs: { lang: locale.value },
  bodyAttrs: { class: 'body--ods-brand' },
})

useSeoMeta({
  title: () => `${notFound.value ? t('message.error_page.not_found_title') : t('message.error_page.unavailable_title')} | opendata.swiss`,
  robots: 'noindex',
})
</script>

<style lang="scss" scoped>
.error-page__links {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
}
</style>
