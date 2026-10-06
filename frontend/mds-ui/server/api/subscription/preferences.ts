import Listmonk from '../../lib/listmonk'
import { getIdFromQuery } from '#server/lib/subscription/preferences'

export default defineEventHandler(async (event) => {
  const { listmonk: config } = useRuntimeConfig()

  const id = await getIdFromQuery(event)

  const listmonk = new Listmonk(config)

  const subscriber = await listmonk.subscribers.get(id)

  return {
    preferences: subscriber.attribs || {},
  }
})
