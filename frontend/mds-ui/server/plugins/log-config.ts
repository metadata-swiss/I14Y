export default defineNitroPlugin(() => {
  if (process.env.LOG_CONFIG || import.meta.dev) {
    const {
      app,
      showcases,
      listmonk,
      'public': publicConfig,
      hyvor,
      subscription,
    } = useRuntimeConfig()
    console.info('Runtime configuration:', JSON.stringify({
      app,
      showcases,
      listmonk,
      public: publicConfig,
      hyvor,
      subscription,
    }, null, 2))
  }
})
