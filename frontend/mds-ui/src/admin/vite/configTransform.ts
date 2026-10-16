import type { TransformFunc } from 'vite-plugin-static-copy'
import * as yaml from 'js-yaml'

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
type Field = OrganizationsSelectField

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

  const config = yaml.load(content) as DecapConfig

  if (process.env.NODE_ENV === 'development') {
    // In development, we want to use the local backend

    config.local_backend = {
      url: 'http://localhost:8088/api/v1',
    }

    // add path leading to the local content clone
    config.media_folder = `opendata.swiss/ui/content/${config.media_folder}`

    for (const collection of config.collections) {
      collection.folder = `opendata.swiss/ui/content/${collection.folder}`
    }
  }
  else {
    config.backend.repo = `${process.env.GITHUB_OWNER}/${process.env.GITHUB_CMS_REPO}`
  }

  return yaml.dump(config)
}
