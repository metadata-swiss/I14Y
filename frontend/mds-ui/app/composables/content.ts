import type { Collections, CollectionQueryBuilder } from '@nuxt/content'
import type { H3Event } from 'h3'
import { queryCollection as queryCollectionServer } from '@nuxt/content/server'

export function queryPublishedContent<T extends keyof Collections>(collection: T): CollectionQueryBuilder<Collections[T]>
export function queryPublishedContent<T extends keyof Collections>(event: H3Event, collection: T): CollectionQueryBuilder<Collections[T]>
export function queryPublishedContent<T extends keyof Collections>(
  eventOrCollection: H3Event | T,
  maybeCollection?: T,
): CollectionQueryBuilder<Collections[T]> {
  if (typeof eventOrCollection === 'string') {
    return (queryCollection as unknown as (col: T) => CollectionQueryBuilder<Collections[T]>)(eventOrCollection).orWhere(q =>
      q.where('publicationDate', 'IS NULL').where('publicationDate', '<=', new Date().toISOString()),
    )
  }

  return queryCollectionServer(eventOrCollection, maybeCollection!).orWhere(q =>
    q.where('publicationDate', 'IS NULL').where('publicationDate', '<=', new Date().toISOString()),
  )
}
