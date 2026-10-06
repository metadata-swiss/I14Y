import { defineTransformer } from '@nuxt/content'

export default defineTransformer({
  name: 'publication-date',
  extensions: ['.md'],
  transform(content) {
    if (content.publicationDate === '' || content.publicationDate === null) {
      delete content.publicationDate
    }
    else if (content.publicationDate) {
      const d = new Date(content.publicationDate as string | number | Date)
      if (!Number.isNaN(d.getTime())) {
        content.publicationDate = d.toISOString()
      }
      else {
        delete content.publicationDate
      }
    }
    return content
  },
})
