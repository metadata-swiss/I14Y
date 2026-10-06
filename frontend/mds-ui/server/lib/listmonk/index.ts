import type { NitroRuntimeConfig } from 'nitropack/types'
import type { AppLanguage } from '~/constants/langages'

interface Envelope<T> {
  data: T
}

export type Frequency = 'daily' | 'weekly'

interface Attribs extends Record<string, unknown> {
  frequency?: Frequency
  datasets?: string[]
  categories?: string[]
  organisations?: string[]
  language?: AppLanguage
}

export interface Subscriber {
  id: number
  name: string
  email: string
  status: 'enabled' | 'blocklisted'
  lists?: number[]
  attribs?: Attribs
}

interface Subscribers {
  results: Array<Subscriber>
}

interface TransactionBase {
  template_id: number
  data: unknown
  subscriber_mode?: 'external' | 'fallback' | 'default'
  from_email?: string
  subject?: string
}

interface SubscriberById {
  subscriber_id: Subscriber['id']
}

interface SubscriberByEmail {
  subscriber_email: Subscriber['email']
}

type Transaction = TransactionBase & (SubscriberById | SubscriberByEmail)

export type ListmonkConfig = NitroRuntimeConfig['listmonk']

export default class {
  private readonly authorization: HeadersInit
  private readonly baseUrl: URL

  constructor(private config: ListmonkConfig) {
    this.authorization = {
      Authorization: `token ${config.api.user}:${config.api.token}`,
    }

    this.baseUrl = new URL('api/', config.api.url)
  }

  get subscribers() {
    const url = new URL('subscribers', this.baseUrl)

    return {
      get: async (id: string): Promise<Subscriber> => {
        const res = await fetch(new URL(`${id}`, url + '/'), {
          headers: this.authorization,
        })

        if (!res.ok) {
          console.error(await res.text())
          throw new Error(`Failed to fetch subscriber ${id}: ${res.status} ${res.statusText}`)
        }

        const payload: Envelope<Subscriber> = await res.json()
        return payload.data
      },

      list: async ({ email, attribs }: { email?: string, attribs?: Partial<Attribs> } = {}) => {
        const searchUrl = new URL(url)
        if (email) {
          searchUrl.searchParams.set('query', `subscribers.email = '${email}'`)
        }

        if (attribs) {
          const query = []
          for (const [key, value] of Object.entries(attribs)) {
            query.push(`subscribers.attribs->>'${key}' = '${value}'`)
          }
          searchUrl.searchParams.set('query', query.join(' AND '))
        }

        const getSubscribers = await fetch(searchUrl, {
          headers: this.authorization,
        })

        if (!getSubscribers.ok) {
          console.error(await getSubscribers.text())
          throw new Error(`Failed to fetch subscribers: ${getSubscribers.status} ${getSubscribers.statusText}`)
        }

        const subscribers: Envelope<Subscribers> = await getSubscribers.json()

        return subscribers.data.results
      },

      create: (subscriber: Omit<Subscriber, 'id' | 'email'>) => {
        return fetch(url, {
          method: 'POST',
          body: JSON.stringify(subscriber),
          headers: {
            ...this.authorization,
            'content-type': 'application/json',
          },
        })
      },

      update: (subscriber: Subscriber['id'], patch: Partial<Subscriber>) => {
        const subscriberUrl = new URL(`${subscriber}`, url + '/')

        return fetch(subscriberUrl, {
          method: 'PATCH',
          body: JSON.stringify(patch),
          headers: {
            ...this.authorization,
            'content-type': 'application/json',
          },
        })
      },
    }
  }

  get transactional() {
    return {
      sendDigest: ({ subscriber, language, data }: { subscriber: Subscriber['id'], language: AppLanguage, data: unknown }) => {
        const template_id = this.config.template.ids[language]

        const transaction = {
          template_id,
          subscriber_id: subscriber,
          data,
        }

        return this.transactional.send(transaction)
      },

      send: (transaction: Transaction) => {
        const txUrl = new URL('tx', this.baseUrl)
        return fetch(txUrl, {
          method: 'POST',
          body: JSON.stringify(transaction),
          headers: {
            ...this.authorization,
            'content-type': 'application/json',
          },
        })
      },
    }
  }
}
