import { defineNuxtPlugin, useRuntimeConfig } from '#imports'
import { setIopCoreBaseUrl } from '~~/api-client/iop-core-fetch'

// Points the generated iop-core client at the configured iop-core.
// The browser goes through the proxy route of the server (server/routes/input-backend/), the server calls iop-core directly.
export default defineNuxtPlugin(() => {
  const { iopCoreUrl } = useRuntimeConfig().public

  setIopCoreBaseUrl(import.meta.client ? '/input-backend' : iopCoreUrl)
})
