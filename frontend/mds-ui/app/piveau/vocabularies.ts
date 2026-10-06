import z from 'zod'
import { defineHubSearch } from '@piveau/sdk-vue'

export function useVocabularySearch() {
  const baseUrl = useRuntimeConfig().public.piveauHubSearchUrl as string

  return defineHubSearch({
    baseUrl,
    index: 'vocabulary',
    indexDetails: 'vocabularies',
    schema: z.object({
      pref_label: z.record(z.string(), z.string()),
      id: z.string(),
      resource: z.string(),
      in_scheme: z.string(),
      index: z.string(),
    }),
  }, resource => resource)
}
