import { registerEditorComponent } from './index.js'

registerEditorComponent({
  id: 'OdsCard',
  label: 'Card',
  collapsed: true,
  fields: [{
    name: 'type',
    label: 'Variant',
    widget: 'select',
    options: ['default', 'highlight', 'twitter', 'flat', 'universal', 'list'],
  }, {
    name: 'slideshowCard',
    label: 'Slideshow Card',
    hint: 'Tick when using slideshow layout',
    widget: 'boolean',
  }, {
    name: 'title',
    label: 'Title',
    widget: 'string',
  }, {
    name: 'image',
    label: 'Image',
    widget: 'image',
  }, {
    name: 'href',
    label: 'Link',
    widget: 'string',
  }, {
    name: 'meta',
    label: 'Meta Info',
    widget: 'list',
  }],
  content: true,
})
