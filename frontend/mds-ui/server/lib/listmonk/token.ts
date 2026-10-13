import type { H3Event } from 'h3'
import crypto from 'node:crypto'

export function generateToken(subscriberId: string, key: string) {
  return crypto.createHmac('sha256', key)
    .update(subscriberId.toString())
    .digest('hex')
}

export function validatePreferencesToken(event: H3Event, key: string) {
  const { token, id: subscriber } = getQuery(event)

  if (!token) {
    throw createError({
      statusCode: 400,
      message: 'Token is required',
    })
  }

  if (!subscriber) {
    throw createError({
      statusCode: 400,
      message: 'Subscriber ID is required',
    })
  }

  const expected = generateToken(subscriber.toString(), key)

  if (token !== expected) {
    throw createError({
      statusCode: 403,
      message: 'Invalid token',
    })
  }

  return subscriber.toString()
}
