import type { H3Event } from 'h3'

import acceptLanguage from 'accept-language'
import type { AppLanguage } from '~/constants/langages'

acceptLanguage.languages(['de', 'fr', 'it', 'en'])

export function getLanguage({ headers }: H3Event): AppLanguage {
  return <AppLanguage>acceptLanguage.get(headers.get('accept-language')) || 'de'
}
