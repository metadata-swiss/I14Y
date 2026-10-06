import { layout, slideshowOptions } from './common.js'
import { registerEditorComponent } from './index.js'

registerEditorComponent({
  id: 'OdsSectionPromotedShowcases',
  label: 'Latest Showcases',
  fields: [{
    name: 'title',
    label: 'Title',
    widget: 'string',
  }, {
    name: 'max',
    label: 'Max',
    widget: 'number',
    min: 1,
    value_type: 'int',
    step: 1,
    json: true,
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
  }],
})
