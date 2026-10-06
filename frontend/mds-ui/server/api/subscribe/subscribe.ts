import type { H3Event } from 'h3'
import Listmonk from '../../lib/listmonk'
import { getLanguage } from '#server/lib/locale'

type PayloadItem = {
  name: 'category' | 'organisation' | 'dataset'
  data: Buffer
}

export function subscribe(key: 'categories' | 'datasets' | 'organisations', fieldName: PayloadItem['name']) {
  return async (event: H3Event) => {
    const { user: { name, email } } = event.context
    const t = await useTranslation(event)
    const referer = getHeader(event, 'referer')
    const payload = await readFormData(event)
    const values = payload.getAll(fieldName).map(field => field.toString()).filter(Boolean)
    const language = getLanguage(event)

    if (!values.length) {
      return createError({
        statusCode: 400,
        statusMessage: 'No values provided for subscription',
      })
    }

    const { listmonk: config } = useRuntimeConfig()

    const listmonk = new Listmonk(config)
    const subscribers = await listmonk.subscribers.list({ email })

    const subscriber = subscribers.pop() || {
      name,
      email,
      status: 'enabled',
      lists: [],
      attribs: {
        language,
        frequency: 'daily',
        [key]: values,
      },
    }

    let res: Response
    if ('id' in subscriber) {
      res = await listmonk.subscribers.update(subscriber.id, {
        attribs: {
          [key]: [...new Set([
            ...(subscriber.attribs?.[key] || []),
            ...values,
          ])],
        },
      })
    }
    else {
      res = await listmonk.subscribers.create(subscriber)
    }

    if (!res.ok) {
      const cause = await res.text()
      return createError({
        statusCode: res.status,
        statusMessage: `Failed to subscribe: ${res.statusText}`,
        cause,
      })
    }

    const messageId = 'subscribe.success'
    if (referer) {
      setCookie(event, 'message', messageId, { path: '/' })
      return sendRedirect(event, referer)
    }

    return {
      message: t(`message.${messageId}`),
    }
  }
}
