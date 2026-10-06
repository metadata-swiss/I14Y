import type Listmonk from '../../lib/listmonk'
import type { NitroRuntimeConfig } from 'nitropack/types'
import type { HubRepo, HubSearch } from '../../lib/piveau'
import { rdf, schema } from '@tpluscode/rdf-ns-builders'
import { ns as piveau } from '../../lib/piveau'

export interface User {
  email: string
}

export interface Page {
  url: string
  title: string
  identifier: `dataset-${string}` | `showcase/${string}`
  ratings: {
    average: number
  }
}

export interface Comment {
  user: User
  page: Page
  parent: Comment | null
  body_html: string
  status: 'pending' | 'published'
  history: Array<{
    type: 'moderation'
    new_status: 'published'
  }>
}

export interface Rating {
  id: number
  created_at: number | null
  page: Page | null
  user: User | null
  rating: number
}

export default class {
  constructor(
    private config: {
      hyvor: Pick<NitroRuntimeConfig['hyvor'], 'publisherNotificationTemplateId'>
      showcases: Pick<NitroRuntimeConfig['showcases'], 'catalogId' | 'resourceType'>
    },
    private listmonk: Listmonk,
    private hubSearch: HubSearch,
    private hubRepo: HubRepo,
  ) {
  }

  private shouldNotifyAboutComment(comment: Comment): boolean {
    if (!this.config.hyvor.publisherNotificationTemplateId) {
      console.log('Publisher notification template ID not configured')
      return false
    }

    if (comment.parent) {
      return false
    }

    if (comment.status === 'published') {
      if (comment.history.length === 0) {
        // Comment was published immediately, so we should notify
        return true
      }

      if (comment.history[0]!.type === 'moderation' && comment.history[0]!.new_status === 'published') {
        // Comment was published after moderation, so we should notify
        return true
      }
    }

    return false
  }

  async handleComment(payload: Comment) {
    if (!this.shouldNotifyAboutComment(payload)) {
      return
    }

    const datasetId = payload.page.identifier.substring('dataset-'.length)
    const dataset = await this.hubSearch.datasets.get(datasetId)

    if (dataset instanceof Error) {
      return dataset
    }

    const publisher = dataset.contact_point?.shift()

    if (!publisher?.email) {
      return new Error(`No contact point email found for dataset ${datasetId}`)
    }

    const res = await this.listmonk.transactional.send({
      template_id: this.config.hyvor.publisherNotificationTemplateId,
      subscriber_email: publisher.email,
      data: {
        page: {
          url: payload.page.url,
          title: payload.page.title,
        },
        publisher: {
          name: publisher.name,
        },
        author: {
          email: payload.user.email,
        },
        comment: {
          body: payload.body_html,
        },
      },
      subscriber_mode: 'external',
    })

    if (!res.ok) {
      return new Error(`Failed to send notification email: ${res.status}`, {
        cause: await res.text(),
      })
    }
  }

  async handleRating({ page }: Rating) {
    if (!page) {
      return new Error('Rating does not have an associated page')
    }

    const hyvorPageId = page.identifier
    const showcaseId = /showcase\/(.+)/.exec(hyvorPageId)?.[1]

    if (!showcaseId) {
      return new Error(`Failed to extract showcase ID from Hyvor page identifier '${hyvorPageId}'`)
    }

    const id = {
      resourceType: this.config.showcases.resourceType,
      catalogId: this.config.showcases.catalogId,
      id: showcaseId,
    }

    const graph = await this.hubRepo.getResource(id)
    const showcase = graph
      .has(rdf.type, piveau.CustomResource)

    showcase
      .deleteOut(schema.ratingValue)
      .addOut(schema.ratingValue, page.ratings.average)
    await this.hubRepo.putResource(id, showcase)
  }
}
