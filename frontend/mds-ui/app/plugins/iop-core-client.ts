import { defineNuxtPlugin, useRuntimeConfig } from '#imports'
import { setIopCoreBaseUrl } from '~~/api-client/iop-core-fetch'

// Points the generated iop-core client at the configured iop-core.
// In `nuxt dev`, the browser goes through the dev proxy (see nitro.devProxy in nuxt.config.ts).
export default defineNuxtPlugin(() => {
  const { iopCoreUrl } = useRuntimeConfig().public

  setIopCoreBaseUrl(import.meta.dev && import.meta.client ? '/iop-core' : iopCoreUrl)
})
