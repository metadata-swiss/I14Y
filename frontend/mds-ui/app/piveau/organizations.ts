import { defineHubSearch } from '@piveau/sdk-vue'
import { z } from 'zod/v4'

export const facets = ['classification']

const schema = z.object({
  id: z.string(),
  resource: z.url(),
  sub_organization_of: z.array(z.string()).optional(),
  name: z.record(z.string(), z.string()).optional(),
  pref_label: z.record(z.string(), z.string()).optional(),
  ancestors: z.array(z.object({
    id: z.string(),
    resource: z.url(),
    name: z.record(z.string(), z.string()).optional(),
    pref_label: z.record(z.string(), z.string()).optional(),
  })).optional(),
})

export type Organization = z.infer<typeof schema>

export function useOrganizationSearch() {
  const baseUrl = useRuntimeConfig().public.piveauHubSearchUrl as string

  return defineHubSearch({
    baseUrl,
    index: 'organization',
    facets,
    schema,
  }, org => org)
}
