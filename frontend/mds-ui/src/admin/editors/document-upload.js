import CMS from 'decap-cms-app'

CMS.registerEditorComponent({
  id: 'document-upload',
  label: 'Uploaded document (link)',
  icon: 'file',
  summary: '{{linkText}}',
  collapsed: true,
  fields: [
    {
      label: 'Upload',
      name: 'file',
      widget: 'file',
    },
    {
      label: 'Link text',
      name: 'linkText',
      widget: 'string',
    },
    {
      label: 'Open in new tab',
      name: 'openInNewTab',
      widget: 'boolean',
      required: false,
      default: true,
    },
  ],
  pattern: /^<a\s+(?=[^>]*\bdata-decap-upload\b)[^>]*href="([^"]+)"[^>]*>([\s\S]*?)<\/a>/,
  fromBlock: function (match) {
    return {
      file: match[1],
      linkText: match[2],
      openInNewTab: /target="_blank"/i.test(match[0]),
    }
  },
  toBlock: function (data) {
    if (!data.file) return ''
    const linkText = data.linkText || data.file
    if (data.openInNewTab) {
      return `<a data-decap-upload href="${data.file}" target="_blank" rel="noopener noreferrer">${linkText}</a>`
    }
    return `<a data-decap-upload href="${data.file}">${linkText}</a>`
  },
  toPreview: function (data) {
    return this.toBlock(data)
  },
})
