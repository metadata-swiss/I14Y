// Writes the OpenAPI document of /input-backend/ (served at /input-backend/openapi.json, shown at /input-backend/docs):
// iop-core's document, reduced to the GET operations of api-client/input-backend.paths.json and the schemas they use.
// Run by `npm run api:generate`, after orval.
import { readFileSync, writeFileSync } from 'node:fs'

const SOURCE = '../../backend/src/Core/Bfs.Iop.Core.ApiClient/Generated/IopCoreApiClient.swagger.json'
const PATHS = 'api-client/input-backend.paths.json'
const TARGET = 'api-client/generated/input-backend.openapi.json'

const core = JSON.parse(readFileSync(SOURCE, 'utf8'))
const allowed = JSON.parse(readFileSync(PATHS, 'utf8'))

/** Keeps only the application/json content of a request body or response. */
function jsonOnly(part) {
  return part?.content?.['application/json'] ? { ...part, content: { 'application/json': part.content['application/json'] } } : part
}

const paths = {}
for (const path of allowed) {
  const operation = core.paths[path]?.get
  if (!operation) throw new Error(`${PATHS}: iop-core has no GET ${path}`)

  // The operations are anonymous: no Bearer token, unlike the rest of iop-core.
  const anonymous = { ...operation }
  delete anonymous.security
  paths[path] = {
    get: {
      ...anonymous,
      responses: Object.fromEntries(Object.entries(operation.responses).map(([status, response]) => [status, jsonOnly(response)])),
    },
  }
}

// The schemas the operations refer to, directly or through other schemas.
const schemas = {}
const pending = [paths]
while (pending.length > 0) {
  const value = pending.pop()
  if (!value || typeof value !== 'object') continue

  const ref = value.$ref
  if (typeof ref === 'string' && ref.startsWith('#/components/schemas/')) {
    const name = ref.slice('#/components/schemas/'.length)
    if (!schemas[name]) {
      schemas[name] = core.components.schemas[name]
      pending.push(schemas[name])
    }
  }
  pending.push(...Object.values(value))
}

const tags = new Set(Object.values(paths).flatMap(item => item.get.tags ?? []))

const document = {
  openapi: core.openapi,
  info: {
    title: 'opendata.swiss: iop-core (read-only)',
    description: 'The iop-core endpoints that opendata.swiss uses, forwarded by its server. Anonymous, GET only.',
    version: core.info.version,
  },
  servers: [{ url: '/input-backend' }],
  paths,
  components: { schemas: Object.fromEntries(Object.keys(schemas).sort().map(name => [name, schemas[name]])) },
  tags: (core.tags ?? []).filter(tag => tags.has(tag.name)),
}

writeFileSync(TARGET, `${JSON.stringify(document, null, 2)}\n`)
console.log(`${TARGET}: ${Object.keys(paths).length} operations, ${Object.keys(schemas).length} schemas`)
