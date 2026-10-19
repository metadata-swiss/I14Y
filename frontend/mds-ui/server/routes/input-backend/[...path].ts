import { joinURL } from 'ufo'
import allowedPaths from '../../../api-client/input-backend.paths.json'

// The browser calls iop-core through this route, in `nuxt dev` and in production: iop-core has no CORS rule for the
// mds-ui hosts. The target is read at runtime (NUXT_PUBLIC_IOP_CORE_URL), so the same image works in every environment.
// Only the GET endpoints of api-client/input-backend.paths.json are forwarded (documented at /input-backend/docs),
// without the caller's credentials or cookies.

const PREFIX = '/input-backend'

// The path templates as patterns, matched against the decoded path segments: /api/Vocabularies/{identifier} -> one segment.
const ALLOWED_PATHS = allowedPaths.map(template =>
  new RegExp(`^${template.split('/').map(segment => /^\{\w+\}$/.test(segment) ? '[^/]+' : segment.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')).join('/')}$`),
)

const FORWARDED_HEADERS = ['accept', 'accept-language', 'if-none-match', 'if-modified-since']

/** The decoded segments of the path, or undefined when one is malformed or could leave the endpoint (`..`, `\`, `/`). */
function decodeSegments(pathname: string): string[] | undefined {
  try {
    const segments = pathname.split('/').slice(1).map(segment => decodeURIComponent(segment))
    return segments.every(segment => segment && segment !== '.' && segment !== '..' && !/[/\\]/.test(segment))
      ? segments
      : undefined
  }
  catch {
    return undefined
  }
}

export default defineEventHandler((event) => {
  if (event.method !== 'GET') {
    setResponseHeader(event, 'allow', 'GET')
    throw createError({ statusCode: 405 })
  }

  const [pathname = '', search] = event.path.slice(PREFIX.length).split(/\?(.*)/s)
  const segments = decodeSegments(pathname)
  if (!segments || !ALLOWED_PATHS.some(allowed => allowed.test(`/${segments.join('/')}`))) {
    throw createError({ statusCode: 404 })
  }
  // iop-core gets the path that was checked, encoded again, not the caller's spelling of it.
  const path = `/${segments.map(segment => encodeURIComponent(segment)).join('/')}${search ? `?${search}` : ''}`

  const headers: Record<string, string> = {}
  for (const name of FORWARDED_HEADERS) {
    const value = getRequestHeader(event, name)
    if (value) headers[name] = value
  }

  const { iopCoreUrl } = useRuntimeConfig(event).public

  return sendProxy(event, joinURL(iopCoreUrl, path), { headers, fetchOptions: { method: event.method } })
})
