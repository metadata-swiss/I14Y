import { nodePolyfills } from 'vite-plugin-node-polyfills'
import { defineConfig } from 'vite'
import { viteStaticCopy } from 'vite-plugin-static-copy'
import { transform } from './vite/configTransform.js'

export default defineConfig({
  base: '/admin',
  build: {
    outDir: '../../public/admin',
  },
  envPrefix: 'NUXT_',
  plugins: [
    nodePolyfills(),
    viteStaticCopy({
      targets: [
        {
          src: 'config.yml',
          dest: '',
          transform,
        },
      ],
    }),
  ],
})
