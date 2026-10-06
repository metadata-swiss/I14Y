export default defineNuxtRouteMiddleware((to) => {
  const { $i18n } = useNuxtApp()
  const locales = $i18n.localeCodes.value
  const path = to.path

  // Check if the path already starts with a locale
  const hasLocale = locales.some(locale => path.startsWith(`/${locale}/`) || path === `/${locale}`)

  // Skip if it's a known public asset or a technical path
  // Also skip if it's the admin path (from nuxt.config.ts devProxy)
  const skipPaths = ['/_nuxt', '/api', '/img', '/admin']
  const skipFiles = ['/favicon.ico', '/favicon.svg', '/apple-touch-icon.png', '/site.webmanifest', '/robots.txt']
  if (skipPaths.some(p => path.startsWith(p)) || skipFiles.includes(path) || path.endsWith('.png')) {
    return
  }

  if (!hasLocale) {
    const locale = $i18n.locale.value || $i18n.defaultLocale
    return navigateTo(`/${locale}${path}`, { redirectCode: 302 })
  }
})
