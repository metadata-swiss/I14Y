import type { H3Event } from 'h3'
import Listmonk from '#server/lib/listmonk'
import { validatePreferencesToken } from '#server/lib/listmonk/token'

export async function getIdFromQuery(event: H3Event) {
  const { user } = event.context

  const { id, token } = getQuery(event)
  if (!id) {
    throw createError({
      statusCode: 400,
      message: 'Subscriber ID is required',
    })
  }

  const listmonk = useRuntimeConfig().listmonk
  if (token) {
    return validatePreferencesToken(event, listmonk.preferences.hmac_key)
  }

  const subscriber = await new Listmonk(listmonk).subscribers.get(id.toString())

  if (subscriber.email === user.email) {
    return id.toString()
  }

  throw createError({
    statusCode: 403,
    message: 'Forbidden',
  })
}
