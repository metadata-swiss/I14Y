// IRIs of I14Y resources, built like public-ui does (shared/helper/iri-helpers.ts) and like iop-core's RDF export:
// {IRI base URL}/{type}/{identifier}. iop-core's JSON API doesn't return them. The base URL is the setting iriUrl
// (NUXT_PUBLIC_IRI_URL).

export function buildAgentIri(iriBaseUrl: string, identifier: string): string {
  return `${iriBaseUrl.replace(/\/+$/, '')}/agent/${identifier}`
}
