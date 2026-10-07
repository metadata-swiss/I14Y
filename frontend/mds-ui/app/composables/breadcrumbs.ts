import type { CollectionQueryBuilder } from '@nuxt/content'
import type { BreadcrumbItem } from '~/components/OdsBreadcrumbs.vue'

export interface LoadBreadcrumbContent {
  (arg: { path: string }, index: number): CollectionQueryBuilder<{
    id: string
    title?: string
    breadcrumb_title?: string
  }>
}

interface Options {
  route: ReturnType<typeof useRoute>
  locale: ReturnType<typeof useI18n>['locale']
  loadContent: LoadBreadcrumbContent
}

export async function useBreadcrumbs({ route, locale, loadContent }: Options) {
  const breadcrumbs: BreadcrumbItem[] = []
  const segments = route.path.split('/')
    .filter(segment => segment)
    .slice(1)

  while (segments.length) {
    const path = '/' + segments.join('/')
    const { id, breadcrumb_title, title } = await loadContent({ path }, segments.length - 1).first() || {}
    breadcrumbs.unshift({
      id, title: breadcrumb_title || title || path, path,
    })
    segments.pop()
  }

  breadcrumbs.unshift(await homePageBreadcrumb(locale))

  return breadcrumbs.length > 1 ? breadcrumbs : []
}

export async function homePageBreadcrumb(locale: ReturnType<typeof useI18n>['locale']): Promise<BreadcrumbItem> {
  const index = await queryPublishedContent('pages')
    .where('path', 'LIKE', `%/index.${locale.value}`)
    .first()

  return {
    id: index?.id,
    title: index?.title ?? '',
    path: '/',
  }
}
