import { registerEditorComponent } from './index.js'
import { layout, slideshowOptions } from './common.js'

registerEditorComponent({
  id: 'OdsSection',
  label: 'Section',
  fields: [{
    name: 'title',
    label: 'Title',
    widget: 'string',
  }, {
    name: 'layout',
    label: 'Layout',
    widget: 'select',
    options: layout,
  }, {
    name: 'slideshowOptions',
    label: 'Slideshow Options',
    hint: 'Only applicable when layout is set to "slideshow"',
    widget: 'object',
    json: true,
    collapsed: true,
    fields: slideshowOptions,
  }, {
    name: 'accentColor',
    label: 'Accent Color',
    widget: 'select',
    options: ['50', '100', '200', '300', '400', '500', '600', '700', '800', '900'],
  }, {
    name: 'textColor',
    label: 'Text Color',
    widget: 'select',
    options: ['50', '100', '200', '300', '400', '500', '600', '700', '800', '900'],
  }],
  collapsed: true,
  content: true,
})
