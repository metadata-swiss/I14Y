import { registerEditorComponent } from './index.js'
import { layout } from './common.js'

registerEditorComponent({
  id: 'OdsSectionNews',
  label: 'Latest News',
  fields: [{
    name: 'title',
    label: 'Title',
    widget: 'string',
  }, {
    name: 'max',
    label: 'Max',
    widget: 'number',
    min: 1,
    default: 5,
    step: 1,
    value_type: 'int',
    json: true,
  }, {
    name: 'layout',
    label: 'Layout',
    widget: 'select',
    options: layout,
  }],
})
