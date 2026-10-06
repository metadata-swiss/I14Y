import CMS from 'decap-cms-app'

CMS.registerEditorComponent({
  id: 'vimeo-video',
  label: 'Vimeo Video',
  icon: 'video',
  fields: [
    {
      label: 'Vimeo Video ID',
      name: 'id',
      widget: 'string',
    },
  ],
  // Match previously generated Vimeo iframe embeds
  pattern: /<iframe[^>]*src="https?:\/\/player\.vimeo\.com\/video\/(\d+)[^"]*"[^>]*>[\s\S]*?<\/iframe>/,
  fromBlock: function (match) {
    return {
      id: match[1],
    }
  },
  toBlock: function (data) {
    if (!data.id) return ''
    const id = String(data.id).trim()
    if (!/^\d+$/.test(id)) return ''
    const embed = `https://player.vimeo.com/video/${id}`
    return `<iframe width="100%" height="420" src="${embed}" frameborder="0" allow="autoplay; fullscreen; picture-in-picture" allowfullscreen></iframe>`
  },
  toPreview: function (data) {
    return this.toBlock(data)
  },
  isValid: function (data) {
    return !!(data.id && /^\d+$/.test(String(data.id)))
  },
})
