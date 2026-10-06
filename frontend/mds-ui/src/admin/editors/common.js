export
const layout = [{
  label: 'One Card',
  value: 'grid--items-1',
}, {
  label: 'Two Cards',
  value: 'grid--items-2',
}, {
  label: 'Three Cards',
  value: 'grid--items-3',
}, {
  label: 'Four Cards',
  value: 'grid--items-4',
}, {
  label: 'Five Cards',
  value: 'grid--items-5',
}, {
  label: 'Two columns',
  value: 'grid--responsive-cols-2',
}, {
  label: 'Three columns',
  value: 'grid--responsive-cols-3',
}, {
  label: 'Four columns',
  value: 'grid--responsive-cols-4',
}, {
  label: 'Slideshow',
  value: 'slideshow',
}]

export const slideshowOptions = [{
  name: 'speed',
  label: 'Speed [ms]',
  widget: 'number',
  default: 500,
  value_type: 'int',
  step: 50,
}, {
  name: 'autoplay',
  widget: 'object',
  collapsed: true,
  summary: 'Autoplay enabled={{enabled}}, delay={{delay}}',
  fields: [{
    name: 'enabled',
    widget: 'boolean',
  }, {
    name: 'delay',
    label: 'Delay [s]',
    widget: 'number',
    default: 2.5,
    value_type: 'float',
    step: 0.5,
  }],
}]
