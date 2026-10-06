import { defineNitroPlugin } from '#imports'
import { config } from 'zod'
import { en, de, fr, it } from 'zod/locales'
import { getLanguage } from '#server/lib/locale'

const languages = new Map(Object.entries({ en, de, fr, it }))

export default defineNitroPlugin((nitro) => {
  nitro.hooks.hook('request', (event) => {
    const language = getLanguage(event)
    const locale = languages.get(language || 'de') || de
    config(locale())
  })
})
