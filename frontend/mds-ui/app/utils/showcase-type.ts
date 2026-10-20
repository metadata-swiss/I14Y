// The showcase types of opendata.swiss, the last part of the type URI, for example
// https://opendata.swiss/vocabulary/showcase-type/application. They were a vocabulary of the old portal;
// iop-core has none, so the labels are translations (message.showcase.type.*).
const SHOWCASE_TYPES = ['application', 'data_visualization']

/**
 * Label of a showcase type in the current language. An unknown type shows the last part of its URI.
 */
export function showcaseTypeLabel(uri: string | undefined, t: (key: string) => string): string {
  const type = uri?.split('/').pop()
  if (!type) {
    return ''
  }
  return SHOWCASE_TYPES.includes(type) ? t(`message.showcase.type.${type}`) : type.replaceAll('_', ' ')
}
