import { dirname, resolve } from 'node:path'

const __dirname = dirname(new URL(import.meta.url).pathname)

declare module 'nitropack/types' {
  interface NitroRouteConfig {
    basicAuth?: string[]
    anonymous?: boolean
  }
}

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: [
    '@nuxt/eslint',
    '@nuxt/content',
    '@pinia/nuxt',
    '@nuxtjs/i18n',
    '@nuxt/image',
    '@nuxt/icon',
    'nuxt-auth-utils',
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
      piveauHubRepoUrl: 'https://piveau-hub-repo.ref.ods.zazukoians.org/',
      piveauHubSearchUrl: 'https://piveau-hub-search.ref.ods.zazukoians.org/',
      matomo: {
        url: '',
        siteId: '',
      },
      comments: {
        websiteId: 15455,
      },
    },
    appUrl: 'http://localhost:3000/',
    showcases: {
      maxImageWidth: 900,
      catalogId: 'showcases-ods',
      resourceType: 'showcase',
      submissionNotification: {
        templateId: 10,
        sendTo: 'noreply@zazuko.com',
      },
    },
    oauth: {
      keycloak: {
        serverUrl: 'https://keycloak.zazukoians.org',
        realm: 'lindas-next-ref',
        clientId: 'piveau-hub-ui',
        clients: {
          hubRepo: {
            clientId: 'piveau-hub-repo',
            clientSecret: '',
          },
        },
      },
    },
    apiTunerTests: false,
    listmonk: {
      api: {
        url: '',
        user: '',
        token: process.env.LISTMONK_ADMIN_API_TOKEN,
      },
      preferences: {
        hmac_key: '',
      },
      template: {
        ids: {
          de: 6,
          fr: 7,
          it: 8,
          en: 9,
        },
      },
    },
    hyvor: {
      webhooksEnabled: false,
      webhookSecret: '',
      publisherNotificationTemplateId: 5,
    },
    subscription: {
      datasetQueryBatchSize: 100,
      maxDatasetsPerEmail: 100,
    },
  },
  dir: {
    pages: resolve(import.meta.dirname, 'pages'),
  },
  build: {
    transpile: [
      'form-data',
      '@hyvor/hyvor-talk-vue',
      '@hyvor/hyvor-talk-base',
    ],
  },
  routeRules: {
    '/api/showcases': { anonymous: true, basicAuth: ['POST'] },
    '/api/subscribe/*': { basicAuth: ['POST'] },
    '/api/subscription/preferences': { basicAuth: ['PUT', 'GET'] },
    '*/showcases/submit': { anonymous: true, ssr: false },
  },
  compatibilityDate: '2025-07-15',
  nitro: {
    devProxy: {
      '/admin/': 'http://localhost:5173/admin/',
    },
    plugins: [
      '~~/server/plugins/zod-locale',
      '~~/server/plugins/showcase-harvesting-trigger',
      '~~/server/plugins/log-config',
    ],
    hooks: {
      'dev:reload': () => import('sharp'),
    },
    typescript: {
      tsConfig: {
        compilerOptions: {
          skipLibCheck: true,
          types: [
            'mocha',
            'chai',
            'chai-as-promised',
            'sinon-chai',
          ],
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
