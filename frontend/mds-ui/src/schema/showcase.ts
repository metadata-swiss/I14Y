import { z } from 'zod/v4'
import { APP_LANGUAGES } from '../../app/constants/langages.js'

export const relationshipRole = z.enum([
  'author',
  'collaborator',
  'pointOfContact',
])

export const shape = {
  active: z.boolean(),
  pinned: z.boolean(),
  title: z.string().min(5),
  images: z.array(z.object({
    image: z.string().nonempty(),
  })).min(1),
  url: z.string(),
  themes: z.array(z.string()).optional(),
  type: z.string(),
  createdBy: z.string(),
  datasets: z.array(z.object({
    id: z.string(),
    label: z.string(),
  })).min(1),
  keywords: z.array(z.string()).optional(),
  more: z.object({
    whoAreYou: z.string().optional(),
    goal: z.string().optional(),
    why: z.string().optional(),
    users: z.string().optional(),
    challengesBefore: z.string().optional(),
    challengesCreating: z.string().optional(),
    getMost: z.string().optional(),
    anythingElse: z.string().optional(),
  }).optional(),
  relationships: z.array(
    z.discriminatedUnion('type', [
      z.object({
        type: z.literal('organization'),
        organization: z.array(z.object({
          id: z.string().nonempty(),
          label: z.string().nonempty(),
        })).nonempty(),
        role: relationshipRole,
      }),
      z.object({
        type: z.literal('organization-external'),
        name: z.string().nonempty(),
        url: z.array(z.string()).optional(),
        role: relationshipRole,
      }),
      z.object({
        type: z.literal('person'),
        name: z.string().nonempty(),
        github: z.string().optional(),
        role: relationshipRole,
      }),
    ]),
  ).optional(),
  rawbody: z.string().optional(),
  publicationDate: z.string().optional(),
}

const submissionSchemaShape = APP_LANGUAGES.reduce((acc, lang) => {
  acc[lang] = z.object(shape)
  return acc
}, { slug: z.string().min(5) } as Record<string, z.ZodObject<typeof shape>> & { slug: z.ZodString })

export const submissionSchema = z.object(submissionSchemaShape)

export default z.object(shape)
