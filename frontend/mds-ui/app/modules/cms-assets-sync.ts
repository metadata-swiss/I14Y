import { defineNuxtModule } from '@nuxt/kit'
import * as fs from 'node:fs/promises'
import * as path from 'node:path'

interface ModuleOptions {
  contentPath: string
  buildPath: string
}

export default defineNuxtModule<ModuleOptions>({
  meta: {
    name: 'cms-assets-sync',
    configKey: 'cmsAssets',
  },
  async setup({ buildPath, contentPath }, nuxtApp) {
    async function copyAssets() {
      try {
        // Check if contentPath exists before copying
        await fs.cp(contentPath, buildPath, { recursive: true })
      }
      catch (e: any) { // eslint-disable-line @typescript-eslint/no-explicit-any
        console.warn(`Failed to copy CMS assets: ${e.message}`)
      }
      finally {
        console.info('CMS assets copied')
      }
    }

    if (process.env.NODE_ENV === 'development') {
      const chokidar = await import('chokidar')
      chokidar
        .watch(contentPath, { ignoreInitial: true })
        .on('all', async (event, filePath) => {
          const relativePath = path.relative(contentPath, filePath)
          const destPath = path.join(buildPath, relativePath)
          switch (event) {
            case 'add':
            case 'change':
              await fs.cp(filePath, destPath).catch(console.error)
              break
            case 'unlink':
              await fs.rm(destPath).catch(console.error)
              break
          }
        })
    }

    nuxtApp.hook('build:before', async () => {
      await copyAssets()
    })
  },
})
