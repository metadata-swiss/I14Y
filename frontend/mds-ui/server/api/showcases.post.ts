import type { ShowcasesCollectionItem } from '@nuxt/content'
import type { H3Event, MultiPartData } from 'h3'
import slugify from 'slugify'
import * as yaml from 'yaml'
import { submissionSchema } from '~~/src/schema/showcase'
import { match, P } from 'ts-pattern'
import git from '~~/server/lib/git'
import fs from '~~/server/lib/fs'
import * as image from '~~/server/lib/images'
import type { AppLanguage, AppLanguage as Language } from '~/constants/langages'
import { APP_LANGUAGES, APP_LANGUAGES as languages } from '~/constants/langages'
import type { ShowcaseStorage } from '~~/server/lib/showcaseStorage'
import Listmonk from '#server/lib/listmonk'

type FormDataFieldNames = keyof ShowcasesCollectionItem | 'contactDetails'
type ShowcaseTranslation = Omit<Partial<ShowcasesCollectionItem>, 'body'> & {
  body: ''
}
type Showcase = Record<Language, ShowcaseTranslation> & {
  slug: string
}
type PayloadData = Array<Omit<MultiPartData, 'name'> & { name: FormDataFieldNames }>

const empty = (): ShowcaseTranslation => ({
  active: true,
  pinned: false,
  themes: [],
  datasets: [],
  keywords: [],
  body: '',
  relationships: [],
})

type MoreDetailsField = keyof Required<Required<ShowcasesCollectionItem>['more']>

export default defineEventHandler(async (event) => {
  const logger = console

  const t = await useTranslation(event)
  const runtimeConfig = useRuntimeConfig()

  const uploads: Array<(storage: ShowcaseStorage) => Promise<void>> = []
  const reqBody = await readMultipartFormData(event) as PayloadData
  let language = getRequestHeader(event, 'accept-language') as AppLanguage | undefined
  if (!language) {
    return createError({
      status: 400,
      message: 'Accept-Language header is required',
    })
  }

  if (!APP_LANGUAGES.includes(language)) {
    language = 'de'
  }

  const showcase: Showcase = {
    slug: '',
    it: empty(),
    de: empty(),
    fr: empty(),
    en: empty(),
  }

  const images: Array<{ image: string }> = []
  let email: string | undefined
  let pointOfContact: { type: 'person', role: 'pointOfContact', name: string, github?: string } = {
    type: 'person',
    role: 'pointOfContact',
    name: '',
  }

  for (const { name, data, filename } of reqBody) {
    match(name)
      .with('title', () => {
        const showcaseTitle = data.toString()
        showcase[language].title = showcaseTitle
        showcase.slug = slugify(showcaseTitle, { lower: true, locale: language })
      })
      .with(P.union('url', 'type'), (urlOrType) => {
        const value = data.toString()
        if (value) {
          toAll(showcase, urlOrType, value)
        }
      })
      .with('keywords', () => {
        const keywords = data.toString().split(',').map(tag => tag.trim()).filter(tag => tag.length > 0)
        if (keywords.length > 0) {
          toAll(showcase, 'keywords', keywords)
        }
      })
      .with('themes', () => {
        toAll(showcase, 'themes', (translation) => {
          const theme = data.toString().trim()
          if (theme) {
            translation.themes!.push(theme)
          }
        })
      })
      .with('images', () => {
        const imagePath = `assets/showcase-${showcase.slug}-${filename}`
        uploads.push(storage => storage.writeImage(imagePath, data))
        images.push({ image: `/cms/${imagePath}` })
      })
      .with('createdBy', () => {
        toAll(showcase, 'createdBy', data.toString())
      })
      .with(P.string.startsWith('datasets'), () => {
        const { id } = /^datasets\[(?<id>.+)]$/.exec(name)?.groups || {}
        if (id) {
          const label = data.toString()
          toAll(showcase, 'datasets', (translation) => {
            translation.datasets!.push({ id, label })
          })
        }
      })
      .with(P.string.startsWith('contactDetails.'), (field) => {
        const key = field.slice('contactDetails.'.length)
        if (key === 'email') {
          email = data.toString()
          return
        }

        pointOfContact = Object.assign(pointOfContact, { [key]: data.toString() })
      })
      .with(P.string.startsWith('more.'), (field) => {
        const more: Required<ShowcasesCollectionItem>['more'] = showcase.de.more || {}

        const k = field.slice('more.'.length) as MoreDetailsField
        more[k] = data.toString()

        toAll(showcase, 'more', more)
      })
      .otherwise(() => {
        console.warn(`Unknown field: ${name}`)
      })
  }

  if (!email) {
    setCookie(event, 'message', 'server.api.showcases.post.failure', { path: '/' })
    return createError({ statusCode: 400, statusMessage: 'Email is required' })
  }

  toAll(showcase, 'images', images)
  toAll(showcase, 'relationships', (translation) => {
    translation.relationships!.push(pointOfContact)
  })

  const errors = validate(event, showcase)
    ?.filter(error => error.path[0] === language)
    ?.map((error) => {
      const path = error.path
      path.shift()
      return {
        ...error,
        path,
      }
    })

  let storage: ShowcaseStorage

  if (process.env.GITHUB_TOKEN || process.env.GITHUB_APP_ID) {
    storage = image.storage(git(showcase.slug), runtimeConfig.showcases)
    const branchCreated = await storage.prepare?.()
    if (!branchCreated) {
      event.node.res.statusCode = 409
      return {
        error: t('message.server.api.showcases.post.error.submission_exists'),
      }
    }
    logger.info('Initialized git storage backend')
  }
  else {
    const { public: { rootDir } } = useRuntimeConfig()
    storage = image.storage(fs(rootDir), runtimeConfig.showcases)
    logger.info('Initialized filesystem storage backend')
  }

  if (errors?.length) {
    logger.info('Validation failed. Reverting showcase submission.')
    await storage.rollback?.()
    return errors
  }

  if (!await save(showcase, uploads, storage)) {
    await storage.rollback?.()
    event.node.res.statusCode = 500
    return {
      error: t('message.server.api.showcases.post.error.unspecified_error'),
    }
  }

  logger.info(`Showcase submission from ${email} saved`)

  const listmonk = new Listmonk(runtimeConfig.listmonk)
  const notificationSent = await listmonk.transactional.send({
    subscriber_email: runtimeConfig.showcases.submissionNotification.sendTo,
    template_id: runtimeConfig.showcases.submissionNotification.templateId,
    subject: `New Showcase submission: ${showcase[language].title}`,
    from_email: email,
    subscriber_mode: 'external',
    data: {
      decapUrl: `${runtimeConfig.appUrl}admin/#/collections/Showcases/entries/${showcase.slug}`,
      submission: yaml.stringify(showcase[language]),
    },
  })

  if (!notificationSent.ok) {
    logger.warn('Failed to send notification', notificationSent.statusText, await notificationSent.text())
  }
  else {
    logger.info('Notification sent')
  }

  event.node.res.statusCode = 200
  setCookie(event, 'message', 'server.api.showcases.post.success', { path: '/' })
  return showcase
})

async function save(showcase: Showcase, uploads: Array<(storage: ShowcaseStorage) => Promise<void>>, storage: ShowcaseStorage) {
  const { slug } = showcase

  const writeContent = languages.map((language) => {
    const path = `showcases/${slug}.${language}.md`

    const { body, ...meta } = showcase[language]
    const frontMatter = yaml.stringify(meta)

    return storage.writeFile(path, `---\n${frontMatter}---\n${body}`)
  })

  await Promise.all([...writeContent, ...uploads.map(upload => upload(storage))])
  return storage.finalize?.()
}

interface Setter {
  (showcase: ShowcaseTranslation): void
}

function toAll<K extends keyof ShowcaseTranslation>(showcase: Showcase, key: K, value: ShowcaseTranslation[K] | Setter) {
  for (const language of languages) {
    if (typeof value === 'function') {
      value(showcase[language])
    }
    else {
      showcase[language][key] = value
    }
  }
}

function validate(event: H3Event, showcase: Showcase) {
  const { error } = submissionSchema.safeParse(showcase)
  if (error) {
    event.node.res.statusCode = 400
    return error.issues
  }
}
