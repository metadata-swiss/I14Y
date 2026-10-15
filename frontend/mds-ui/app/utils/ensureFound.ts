import { createError } from '#imports'
import type { ErrorType } from '~~/api-client/iop-core-fetch'

/**
 * Ends rendering with the error page (app/error.vue) when a page has nothing to show: 404 when the resource doesn't
 * exist, 503 when iop-core failed or couldn't be reached. Without it, a missing resource renders an empty page with
 * status 200, which search engines index as a real page.
 */
export function ensureFound<T>(value: T | null | undefined, error?: ErrorType<unknown> | null): asserts value is T {
  if (error && error.statusCode !== 400 && error.statusCode !== 404) {
    throw createError({ statusCode: 503, statusMessage: 'Service Unavailable', fatal: true })
  }
  if (value === null || value === undefined) {
    throw createError({ statusCode: 404, statusMessage: 'Page Not Found', fatal: true })
  }
}
