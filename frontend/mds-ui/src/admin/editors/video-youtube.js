import CMS from 'decap-cms-app'

CMS.registerEditorComponent({
  id: 'youtube-video',
  label: 'YouTube Video',
  icon: 'video',
  fields: [
    {
      label: 'YouTube Video ID',
      name: 'id',
      widget: 'string',
    },
  ],
  pattern: /<iframe[^>]*src="https?:\/\/(?:www\.)?youtube\.com\/embed\/([A-Za-z0-9_-]{6,})[^"]*"[^>]*>[\s\S]*?<\/iframe>/,
  fromBlock: function (match) {
    return {
      id: match[1],
    }
  },
  toBlock: function (data) {
    if (!data.id) return ''
    const id = String(data.id).trim()
    if (!/^[A-Za-z0-9_-]{6,}$/.test(id)) return ''
    const embed = `https://www.youtube.com/embed/${id}`
    return `<iframe width="100%" height="420" src="${embed}" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>`
  },
  toPreview: function (data) {
    return this.toBlock(data)
  },
  isValid: function (data) {
    return !!(data.id && /^[A-Za-z0-9_-]{6,}$/.test(String(data.id)))
  },
})
