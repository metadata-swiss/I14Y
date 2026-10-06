import crypto from 'node:crypto'
import type { Comment, Rating } from '#server/lib/webhooks/hyvor'
import Hyvor from '#server/lib/webhooks/hyvor'
import Listmonk from '#server/lib/listmonk'
import { HubRepo, HubSearch } from '#server/lib/piveau'

interface RatingWebhookPayload {
  event: 'rating.created' | 'rating.updated' | 'rating.deleted'
  data: {
    rating: Rating
  }
}

interface CommentWebhookPayload {
  event: 'comment.create' | 'comment.update'
  data: Comment
}

type WebhookPayload = RatingWebhookPayload | CommentWebhookPayload

export default defineEventHandler(async (event) => {
  const { oauth, listmonk, showcases, hyvor, public: { piveauHubRepoUrl, piveauHubSearchUrl } } = useRuntimeConfig(event)

  const body = await readRawBody(event)
  const headers = getRequestHeaders(event)

  const expectedSignature = headers['x-signature']
  const signature = crypto.createHmac('sha256', hyvor.webhookSecret)
    .update(body!)
    .digest('hex')

  if (!expectedSignature || expectedSignature !== signature) {
    return createError({
      status: 400,
      message: 'Signature mismatch',
    })
  }

  if (!body) {
    return createError({
      status: 400,
      message: 'Missing body',
    })
  }

  if (hyvor.webhooksEnabled === false) {
    return 204
  }

  const hubRepoAuth = {
    serverUrl: oauth.keycloak.serverUrl,
    realm: oauth.keycloak.realm,
    credentials: oauth.keycloak.clients.hubRepo,
  }
  const payload: WebhookPayload = JSON.parse(body.toString())
  const api = new Hyvor(
    {
      hyvor,
      showcases,
    },
    new Listmonk(listmonk),
    new HubSearch(piveauHubSearchUrl),
    new HubRepo(piveauHubRepoUrl, hubRepoAuth, fetch),
  )

  switch (payload.event) {
    case 'comment.create':
    case 'comment.update':
      return api.handleComment(payload.data)
    case 'rating.created':
    case 'rating.updated':
      return api.handleRating(payload.data.rating)
    default:
      return createError({
        status: 400,
        message: `Unsupported event type "${payload.event}"`,
      })
  }
})
