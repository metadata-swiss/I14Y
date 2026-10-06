import { defineCollection, defineContentConfig } from '@nuxt/content'
import { z } from 'zod/v4'
import * as showcaseSchema from './src/schema/showcase.js'

function sourcesFor(include: string) {
  const sources = [{
    include,
  }]

  if (process.env.NODE_ENV === 'development') {
    // During development, include test sources
    sources.push({
      include: `.test/${include}`,
    })
    sources.push({
      include: `.local/${include}`,
    })
  }

  return sources
}

export default defineContentConfig({
  collections: {
    pages: defineCollection({
      source: sourcesFor('pages/*.md'),
      type: 'page',
      schema: z.object({
        title: z.string(),
        heading: z.string().optional(),
        subHeading: z.string().optional(),
        permalink: z.string().optional(),
        parent: z.string().optional(),
        heroImage: z.string().optional(),
        fullWidth: z.boolean().optional(),
        mainMenu: z.boolean().optional(),
        after: z.string().optional(),
        rawbody: z.string(),
        publicationDate: z.string().optional(),
      }),
    }),
    handbook: defineCollection({
      source: sourcesFor('handbook/**/*.md'),
      type: 'page',
      schema: z.object({
        active: z.boolean(),
        title: z.string(),
        breadcrumb_title: z.string().optional(),
        slug: z.string(),
        parent: z.string().optional(),
        after: z.string().optional(),
        publicationDate: z.string().optional(),
      }),
    }),
    blog: defineCollection({
      source: sourcesFor('blog/*.md'),
      type: 'page',
      schema: z.object({
        title: z.string(),
        pinned: z.boolean(),
        slug: z.string().optional(),
        date: z.date().optional(),
        subHeading: z.string().optional(),
        image: z.string(),
        publicationDate: z.string().optional(),
      }),
    }),
    showcases: defineCollection({
      source: sourcesFor('showcases/*.md'),
      type: 'page',
      schema: z.object(showcaseSchema.shape),
    }),
  },
})
