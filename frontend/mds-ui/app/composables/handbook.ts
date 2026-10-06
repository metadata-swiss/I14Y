import type { HandbookCollectionItem } from '@nuxt/content'

function getPathSegments(article: HandbookCollectionItem, articles: HandbookCollectionItem[], locale: string): string[] {
  const segments = [article.slug]
  let current = article
  while (current.parent) {
    const currentParent = current.parent
    const parent = articles.find((a) => {
      if (!currentParent) return false
      return a.path.endsWith(`handbook/${currentParent}.${locale}`)
        || (locale !== 'de' && a.path.endsWith(`handbook/${currentParent}.de.md`))
    })
    if (!parent) break
    segments.unshift(parent.slug)
    current = parent
  }
  return segments
}

export function queryHandbook() {
  return queryPublishedContent('handbook')
    .orWhere(sub =>
      sub.where('active', 'IS NULL').where('active', '=', true),
    )
}

export async function useGetArticleUrl() {
  const { locale } = useI18n()
  const { data: articles } = await useAsyncData('handbook-articles', () => queryHandbook().all())

  return (article: HandbookCollectionItem) => {
    return `/handbook/${getPathSegments(article, articles.value || [], locale.value).join('/')}`
  }
}
