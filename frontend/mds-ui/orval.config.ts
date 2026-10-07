import { defineConfig } from 'orval'

const isParameter = (segment: string) => segment.startsWith('{') || segment.startsWith('$')
const pascalCase = (segment: string) => segment.split(/[-_]/).map(word => word.charAt(0).toUpperCase() + word.slice(1)).join('')

// iop-core operations have no operationId, so the names are built from the path:
// GET /api/Agents -> getAgents, GET /api/Agents/{id} -> getAgentsById, GET /api/Agents/{id}/sub-agent-of -> getAgentsSubAgentOf,
// GET /api/Datasets/by-identifier/{identifier} -> getDatasetsByIdentifier.
function operationName(_operation: unknown, route: string, verb: string) {
  const segments = route.replace(/^\/api\//, '').split('/')
  const words = segments.filter(segment => !isParameter(segment)).map(pascalCase)
  const last = segments.at(-1) ?? ''
  const by = isParameter(last) ? `By${pascalCase(last.replace(/[${}]/g, ''))}` : ''
  return verb + words.join('') + (words.at(-1) === by ? '' : by)
}

export default defineConfig({
  iopCore: {
    // Written by backend/src/Core/Bfs.Iop.Core.Api.ClientGenerator.
    input: '../../backend/src/Core/Bfs.Iop.Core.ApiClient/Generated/IopCoreApiClient.swagger.json',
    output: {
      target: 'api-client/generated/iop-core.ts',
      mode: 'single',
      client: 'vue-query',
      httpClient: 'fetch',
      override: {
        operationName,
        // Swashbuckle declares text/plain, application/json and text/json for every response; generate one type, not three.
        contentType: { include: ['application/json', 'multipart/form-data'] },
        mutator: { path: 'api-client/iop-core-fetch.ts', name: 'iopCoreFetch' },
        fetch: { includeHttpResponseReturnType: false },
        query: { version: 5 },
      },
    },
  },
})
