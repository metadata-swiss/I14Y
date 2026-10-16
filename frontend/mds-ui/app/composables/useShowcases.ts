import type { ShowcasesCollectionItem } from '@nuxt/content'
import type { Ref } from 'vue'
import { computed } from 'vue'
import type { FacetGroup } from '~/composables/useFacets'
import { useThemes } from '~/composables/useDatasets'
import { localize } from '~/utils/getCurrentTranslation'
import { showcaseTypeLabel } from '~/utils/showcase-type'

// Until V4 the showcases are the Markdown files of the content repository, read with Nuxt Content.
// In V4 they move into iop-core (iop-core-gaps.md, G14); then only usePublishedShowcases changes.

export type Showcase = ShowcasesCollectionItem

export const showcaseFacets = ['categories', 'keywords', 'type'] as const
type ShowcaseFacet = typeof showcaseFacets[number]

const facetValues: Record<ShowcaseFacet, (showcase: Showcase) => string[]> = {
  categories: showcase => showcase.themes ?? [],
  keywords: showcase => showcase.keywords ?? [],
  type: showcase => [showcase.type],
}

/**
 * The active showcases in the current language, by title. They are loaded with the page.
 */
export async function usePublishedShowcases() {
  const { locale } = useI18n()

  const { data } = await useAsyncData(() => `showcases:${locale.value}`, () => queryPublishedContent('showcases')
    .where('stem', 'LIKE', `%.${locale.value}`)
    .where('active', '=', true)
    .order('title', 'ASC')
    .all())

  return computed(() => data.value ?? [])
}

/**
 * The id of a showcase in its address (/showcase/{id}): the file name without the language.
 */
export function showcaseId(showcase: Pick<Showcase, 'stem'>) {
  return (showcase.stem.split('/').pop() ?? '').replace(/\.\w\w$/, '')
}

interface ShowcaseSearchArgs {
  showcases: Ref<Showcase[]>
  q: Ref<string>
  sort: Ref<string>
  page: Ref<number>
  limit: number
  facetRefs: Record<ShowcaseFacet, Ref<string[]>>
}

/**
 * Search, filters, sorting and pages of the showcase list. There are few showcases, so this runs on the loaded list.
 *
 * - Search: every word must appear in the title, the description or the keywords.
 * - Filters: a showcase matches a filter when it has one of the selected values.
 * - Sort: `title.{lang}+asc` or `title.{lang}+dsc`. The showcase content has no dates; sorting by date comes with
 *   iop-core in V4.
 */
export async function useShowcaseSearch({ showcases, q, sort, page, limit, facetRefs }: ShowcaseSearchArgs) {
  const { t, locale } = useI18n()
  const { query: themesQuery, themes } = useThemes()
  await themesQuery.suspense().catch(() => undefined)

  const results = computed(() => {
    const words = q.value.toLowerCase().split(/\s+/).filter(Boolean)
    const order = sort.value.endsWith('+dsc') ? -1 : 1

    return showcases.value
      .filter((showcase) => {
        const text = [showcase.title, showcase.description, ...(showcase.keywords ?? [])].join(' ').toLowerCase()
        return words.every(word => text.includes(word))
      })
      .filter(showcase => showcaseFacets.every((facet) => {
        const selected = facetRefs[facet].value
        return selected.length === 0 || facetValues[facet](showcase).some(value => selected.includes(value))
      }))
      .sort((a, b) => order * a.title.localeCompare(b.title, locale.value))
  })

  const searchResults = computed(() => results.value.slice(page.value * limit, (page.value + 1) * limit))
  const searchResultsCount = computed(() => results.value.length)
  const searchResultsPagesCount = computed(() => Math.ceil(results.value.length / limit))

  const facetTitles: Record<ShowcaseFacet, (value: string) => string> = {
    categories: code => localize(themes.value.find(theme => theme.code === code)?.name, locale.value) || code,
    keywords: keyword => keyword,
    type: uri => showcaseTypeLabel(uri, t),
  }

  const facetGroupTitles: Record<ShowcaseFacet, string> = {
    categories: 'message.showcase.categories',
    keywords: 'message.showcase.keywords',
    type: 'message.showcase.type.header',
  }

  // The values found in the results, with how many results have each value.
  const availableFacets = computed<FacetGroup[]>(() => showcaseFacets.map((facet) => {
    const counts = new Map<string, number>()
    results.value.forEach(showcase => facetValues[facet](showcase)
      .forEach(value => counts.set(value, (counts.get(value) ?? 0) + 1)))

    return {
      id: facet,
      title: t(facetGroupTitles[facet]),
      items: [...counts].map(([id, count]) => ({ id, title: facetTitles[facet](id), count })),
    }
  }))

  return { searchResults, searchResultsCount, searchResultsPagesCount, availableFacets }
}
