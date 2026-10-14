import { dirname, resolve } from 'node:path'

const __dirname = dirname(new URL(import.meta.url).pathname)

// iop-core used by the generated client (api-client/). Override at runtime with NUXT_PUBLIC_IOP_CORE_URL.
const iopCoreUrl = process.env.NUXT_PUBLIC_IOP_CORE_URL || 'https://core.i14y.d.c.bfs.admin.ch'

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: [
    '@nuxt/eslint',
    '@nuxt/content',
    '@pinia/nuxt',
    '@nuxtjs/i18n',
    '@nuxt/image',
    '@nuxt/icon',
    './app/modules/cms-assets-sync',
  ],
  plugins: [],
  pages: {
    enabled: true,
  },
  components: {
    global: true,
    dirs: ['~/components/content'],
  },
  devtools: { enabled: true },
  css: [
    '~/assets/main.css',
    'piveau-preview-plugin/style.css',
    'ag-grid-community/styles/ag-grid.css',
    'ag-grid-community/styles/ag-theme-quartz.css',
  ],
  vue: {
    compilerOptions: {
      isCustomElement: tag => tag.startsWith('swiper-'),
    },
  },
  content: {
    experimental: { nativeSqlite: true },
    build: {
      transformers: [
        '~~/src/transformers/publication-date.ts',
      ],
      markdown: {
        toc: {
          depth: 3,
          searchDepth: 3,
        },
      },
    },
  },
  mdc: {
    components: {
      map: {
        a: 'OdsProseA',
        h2: 'OdsProseH2',
        h3: 'OdsProseH3',
        h4: 'OdsProseH4',
      },
    },
  },
  runtimeConfig: {
    public: {
      rootDir: __dirname,
      iopCoreUrl,
      // The login button of the top header opens the I14Y input portal. Override at runtime with NUXT_PUBLIC_LOGIN_URL.
      loginUrl: 'https://input.i14y.d.c.bfs.admin.ch',
      piveauHubRepoUrl: 'https://piveau-hub-repo.ref.ods.zazukoians.org/',
      piveauHubSearchUrl: 'https://piveau-hub-search.ref.ods.zazukoians.org/',
      matomo: {
        url: '',
        siteId: '',
      },
    },
  },
  compatibilityDate: '2025-07-15',
  nitro: {
    devProxy: {
      '/admin/': 'http://localhost:5173/admin/',
      // iop-core has no CORS rule for localhost, so in `nuxt dev` the browser calls it through this proxy.
      '/iop-core/': { target: `${iopCoreUrl}/`, changeOrigin: true },
    },
    typescript: {
      tsConfig: {
        compilerOptions: {
          skipLibCheck: true,
        },
      },
    },
  },
  cmsAssets: {
    contentPath: resolve(import.meta.dirname, 'content/assets'),
    buildPath: resolve(import.meta.dirname, 'public/cms'),
  },
  eslint: {
    config: {
      stylistic: true,
    },
  },
  i18n: {
    defaultLocale: 'de',
    strategy: 'prefix',
    locales: [
      { code: 'de', name: 'Deutsch', file: 'de.json' },
      { code: 'en', name: 'English', file: 'en.json' },
      { code: 'fr', name: 'Francais', file: 'fr.json' },
      { code: 'it', name: 'Itlaliano', file: 'it.json' },
    ],
    experimental: {
      localeDetector: 'localeDetector.ts',
    },
  },
  icon: {
    mode: 'svg',
    customCollections: [
      {
        prefix: 'ods',
        dir: './app/assets/icons',
        normalizeIconName: false,
      },
    ],
  },
})
