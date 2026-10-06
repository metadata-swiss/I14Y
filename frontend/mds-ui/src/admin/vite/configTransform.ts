import type { TransformFunc } from 'vite-plugin-static-copy'
import * as yaml from 'js-yaml'

const piveauHubSearch = process.env.NUXT_PUBLIC_PIVEAU_HUB_SEARCH_URL

interface FieldBase {
  label: string
  name: string
  widget: string
  types: ObjectField[]
}
interface ObjectField extends FieldBase {
  fields: Field[]
}
interface SelectField extends FieldBase {
  options: Array<{ label: string, value: string }>
}
interface OrganizationsSelectField extends SelectField {
  name: 'organization'
  widget: 'select'
}
interface PiveauVocabularyField extends FieldBase {
  widget: 'piveau-vocabulary'
  piveau: {
    search: string
  }
}

type Field = OrganizationsSelectField | PiveauVocabularyField

interface DecapConfig {
  backend: {
    repo: string
  }
  local_backend?: {
    url: string
  }
  media_folder: string
  collections: Array<{
    folder: string
    fields: Array<Field>
  }>
}

export const transform: TransformFunc<string> = async (content, filename) => {
  if (!filename.endsWith('config.yml')) {
    return content
  }

  if (!piveauHubSearch) {
    console.log('No piveauHubSearch configured')
  }

  const config = yaml.load(content) as DecapConfig

  if (process.env.NODE_ENV === 'development') {
    // In development, we want to use the local backend
    // and local piveau, if configured in .env

    config.local_backend = {
      url: 'http://localhost:8088/api/v1',
    }

    // add path leading to the local content clone
    config.media_folder = `opendata.swiss/ui/content/${config.media_folder}`

    for (const collection of config.collections) {
      collection.folder = `opendata.swiss/ui/content/${collection.folder}`

      for (const field of collection.fields) {
        await processField(field)
      }
    }
  }
  else {
    config.backend.repo = `${process.env.GITHUB_OWNER}/${process.env.GITHUB_CMS_REPO}`

    for (const collection of config.collections) {
      for (const field of collection.fields) {
        await processField(field)
      }
    }
  }

  return yaml.dump(config)
}

async function processField(field: Field) {
  if ('piveau' in field) {
    if (piveauHubSearch) {
      field.piveau.search = piveauHubSearch
    }
  }

  if ('types' in field) {
    for (const type of field.types ?? []) {
      for (const typeField of type.fields) {
        await processField(typeField)
      }
    }
  }
}
