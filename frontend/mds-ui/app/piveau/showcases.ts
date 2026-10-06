import z from 'zod'
import { defineHubSearch } from '@piveau/sdk-vue'

export const facets = ['categories', 'keywords', 'type']

const schema = z.object({
  image: z.array(z.string()),
  references: z.array(z.any()).optional(),
  keywords: z.array(
    z.object({
      id: z.string(),
      label: z.string(),
      language: z.string(),
    }),
  ),
  catalog: z.object({
    id: z.string(),
    description: z.record(z.string(), z.string()),
    issued: z.string(),
    title: z.record(z.string(), z.string()),
    publisher: z.object({
      type: z.string(),
      name: z.string(),
      email: z.string(),
      homepage: z.string(),
    }),
    modified: z.string(),
    homepage: z.string(),
  }),
  index: z.string(),
  id: z.string(),
  abstract: z.record(z.string(), z.string()),
  text: z.record(z.string(), z.string()),
  categories: z.array(
    z.object({
      id: z.string(),
      label: z.record(z.string(), z.string()),
      resource: z.string(),
    }),
  ),
  title: z.record(z.string(), z.string()),
  catalog_record: z.object({
    modified: z.string(),
    issued: z.string(),
  }),
  type: z.object({
    id: z.string(),
    label: z.record(z.string(), z.string()),
    resource: z.string(),
  }),
})

export type PiveauShowcase = z.infer<typeof schema>

export function useShowcaseSearch() {
  const baseUrl = useRuntimeConfig().public.piveauHubSearchUrl as string

  return defineHubSearch({
    baseUrl,
    index: 'resource',
    facets,
    schema,
  }, (showcase) => {
    return {
      ...showcase,
    }
  })
}
