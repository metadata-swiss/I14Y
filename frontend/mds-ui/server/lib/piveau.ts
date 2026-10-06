import type { AppLanguage } from '~/constants/langages'
import type { NitroRuntimeConfig } from 'nitropack/types'
import { requestServiceAccountToken } from '../lib/auth'
import $rdf from '@zazuko/env-node'
import type { AnyPointer } from 'clownface'

interface Category {
  id: string
}

interface ContactPoint {
  name: string
  email: string
}

export interface Dataset {
  id: string
  categories?: Category[]
  title: Record<AppLanguage, string>
  contact_point?: ContactPoint[]
}

interface SearchResult<T> {
  result: {
    scrollId?: string
    results: T[]
  }
}

interface SearchArgs {
  sort?: string
  minDate?: Date
  limit?: number
  dateType?: 'issue' | 'modified' | 'temporal'
}

export const ns = $rdf.namespace('https://piveau.eu/ns/voc#')

export class HubSearch {
  constructor(private baseUrl: string, private _fetch = fetch) {
  }

  get datasets() {
    return {
      search: ({ sort, minDate, limit, dateType }: SearchArgs = {}) => {
        const searchUrl = new URL('search', this.baseUrl)

        searchUrl.searchParams.set('filters', 'dataset')
        if (sort) searchUrl.searchParams.set('sort', sort)
        if (limit) searchUrl.searchParams.set('limit', limit.toString())
        if (minDate) searchUrl.searchParams.set('minDate', minDate.toISOString())
        if (dateType) searchUrl.searchParams.set('dateType', dateType)

        return {
          all: async () => {
            const searchRes = await this._fetch(searchUrl)

            if (!searchRes.ok) {
              return new Error(`Failed to fetch datasets: ${searchRes.status} ${searchRes.statusText}`, {
                cause: await searchRes.text(),
              })
            }
            const searchResult: SearchResult<Dataset> = await searchRes.json()

            return searchResult.result.results
          },
          scroll: this.searchScroll<Dataset>(searchUrl),
        }
      },

      get: async (id: string): Promise<Dataset | Error> => {
        const url = new URL(`datasets/${id}`, this.baseUrl)

        const res = await this._fetch(url)

        if (!res.ok) {
          return new Error(`Failed to fetch dataset ${id}: ${res.status} ${res.statusText}`, {
            cause: await res.text(),
          })
        }

        const json = await res.json()
        return json.result
      },
    }
  }

  private searchScroll<T>(searchUrl: URL) {
    const scrollUrl = new URL('scroll', this.baseUrl)
    const scrollInitUrl = new URL(searchUrl)
    scrollInitUrl.searchParams.set('scroll', 'true')

    return async function* (this: HubSearch) {
      const searchRes: SearchResult<Dataset> = await (await this._fetch(scrollInitUrl)).json()
      if (!searchRes.result.scrollId) {
        return new Error('Scroll ID missing in response')
      }
      scrollUrl.searchParams.set('scrollId', searchRes.result.scrollId)

      yield searchRes.result.results

      let scrollRes
      do {
        scrollRes = await this._fetch(scrollUrl)
        if (scrollRes.ok) {
          const searchResult: SearchResult<T> = await scrollRes.json()

          if (searchResult.result.results.length === 0) {
            break
          }

          yield searchResult.result.results
        }
      } while (scrollRes.ok)
    }.bind(this)
  }
}

interface ResourceId {
  id: string
  resourceType: string
  catalogId: string
}

type Credentials = NitroRuntimeConfig['oauth']['keycloak']['clients']['hubRepo']

interface OAuthConfig {
  serverUrl: string
  realm: string
  credentials: Credentials
}

export class HubRepo {
  constructor(private baseUrl: string, private oauth: OAuthConfig, private _fetch = fetch) {}

  async getResource({ id, resourceType, catalogId }: ResourceId): Promise<AnyPointer> {
    const url = new URL(`/customresources/${resourceType}/${id}?catalogId=${catalogId}`, this.baseUrl)

    const res = await $rdf.fetch(url, {
      fetch: this._fetch,
    })
    const dataset = await res.dataset()

    return res.ok ? $rdf.clownface({ dataset }) : Promise.reject(res)
  }

  async putResource(id: ResourceId, body: AnyPointer): Promise<void> {
    const authToken = await requestServiceAccountToken(this.oauth.serverUrl, this.oauth.realm, this.oauth.credentials)

    const url = new URL(`/customresources/${id.resourceType}?id=${id.id}&catalogId=${id.catalogId}`, this.baseUrl)

    const res = await $rdf.fetch(url, {
      fetch: this._fetch,
      method: 'PUT',
      body: body.dataset,
      headers: {
        Authorization: `Bearer ${authToken}`,
      },
    })

    if (!res.ok) {
      return Promise.reject(new Error(`Failed to put resource: ${res.status} ${res.statusText}`, {
        cause: await res.text(),
      }))
    }
  }
}
