// The opendata.swiss terms of use (by, open, ask, by_ask) of a license. Both the DCAT-AP CH vocabulary URIs and the
// anchors of the opendata.swiss terms-of-use page are recognised. Anything else, such as a name that is already short,
// is returned unchanged.
const TERMS_OF_USE = /^(?:http:\/\/dcat-ap\.ch\/vocabulary\/licenses\/|https:\/\/opendata\.swiss\/en\/terms-of-use\/#)terms_(by_ask|by|open|ask)$/

export function termsOfUseName(license: string): string {
  return license.match(TERMS_OF_USE)?.[1] ?? license
}
