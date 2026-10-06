import CMS from 'decap-cms-app'

CMS.registerEditorComponent({
  id: 'uploaded-video',
  label: 'Upload Video',
  icon: 'video',
  fields: [
    {
      label: 'Select or Upload Video File',
      name: 'video_file',
      widget: 'file',
    },
  ],
  pattern: /^<video[^>]*src="([^"]+)"[^>]*>([\s\S]*?)<\/video>/,
  fromBlock: function (match) {
    return {
      video_file: match[1],
    }
  },
  toBlock: function (data) {
    if (!data.video_file) return ''
    return `<video controls width="100%" src="${data.video_file}"></video>`
  },
  toPreview: function (data) {
    if (!data.video_file) return ''
    return `<video controls width="100%" src="${data.video_file}"></video>`
  },
  isValid: function (data) {
    return data.video_file && data.video_file.match(/\.(mov|mp4|webm)$/i)
  },
})
