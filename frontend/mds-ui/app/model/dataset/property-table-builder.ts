import type { PeriodOfTimeModel, ResourceModel, VCardModel, VocabularyEntryModel } from '~~/api-client/generated/iop-core'
import { localize } from '~/utils/getCurrentTranslation'
import { OdsTableEntry, OdsTableEntryType } from './table-entry'

/**
 * Translates an i18n key, e.g. the `t` of useI18n().
 */
export type Translate = (key: string) => string

/**
 * Builds the property table of a dataset or distribution from iop-core values.
 * Each property gets a row labelled `message.dataset_detail.properties.<key>`; properties without a value are left out.
 */
export class PropertyTableBuilder {
  #entries: OdsTableEntry[] = []
  #lang: string
  #t: Translate

  constructor(lang: string, t: Translate) {
    this.#lang = lang
    this.#t = t
  }

  /**
   * Add a row with text values.
   */
  strings(key: string, values: (string | null | undefined)[] | null | undefined): PropertyTableBuilder {
    const entry = this.#entry(key)
    for (const value of values ?? []) {
      if (value) {
        entry.addValue(value, OdsTableEntryType.String)
      }
    }
    return this.#add(entry)
  }

  /**
   * Add a row with links. The label of a link is its localized label, or its URI.
   */
  links(key: string, resources: (ResourceModel | null | undefined)[] | null | undefined): PropertyTableBuilder {
    const entry = this.#entry(key)
    for (const resource of resources ?? []) {
      if (resource?.uri) {
        entry.addValue(localize(resource.label, this.#lang) || resource.uri, OdsTableEntryType.Href, resource.uri)
      }
    }
    return this.#add(entry)
  }

  /**
   * Add a row with the localized names of vocabulary entries, linked to their URI when they have one.
   */
  vocabulary(key: string, entries: (VocabularyEntryModel | null | undefined)[] | null | undefined): PropertyTableBuilder {
    const entry = this.#entry(key)
    for (const vocabularyEntry of entries ?? []) {
      const label = localize(vocabularyEntry?.name, this.#lang) || vocabularyEntry?.code
      if (!label) {
        continue
      }
      if (vocabularyEntry?.uri) {
        entry.addValue(label, OdsTableEntryType.Href, vocabularyEntry.uri)
      }
      else {
        entry.addValue(label, OdsTableEntryType.String)
      }
    }
    return this.#add(entry)
  }

  /**
   * Add a row with one sub field per contact point: name, email, telephone and address.
   * The name is a value, because the title of a sub field isn't shown.
   */
  contactPoints(key: string, contactPoints: VCardModel[] | null | undefined): PropertyTableBuilder {
    const entry = this.#entry(key)
    for (const contactPoint of contactPoints ?? []) {
      const name = localize(contactPoint.fn, this.#lang)
      const subField = new OdsTableEntry(name || contactPoint.hasEmail || '', '', OdsTableEntryType.Node)
      if (name) {
        subField.addValue(name, OdsTableEntryType.String)
      }
      if (contactPoint.hasEmail) {
        subField.addValue(contactPoint.hasEmail, OdsTableEntryType.Email, `mailto:${contactPoint.hasEmail}`)
      }
      if (contactPoint.hasTelephone) {
        subField.addValue(contactPoint.hasTelephone, OdsTableEntryType.Telephone, `tel:${contactPoint.hasTelephone}`)
      }
      const address = localize(contactPoint.hasAddress, this.#lang)
      if (address) {
        subField.addValue(address, OdsTableEntryType.String)
      }
      entry.addSubField(subField)
    }
    return this.#add(entry)
  }

  /**
   * Add a row with periods of time, shown as "start – end".
   */
  periods(key: string, periods: PeriodOfTimeModel[] | null | undefined): PropertyTableBuilder {
    return this.strings(key, (periods ?? []).map((period) => {
      const start = this.#formatDate(period.start)
      const end = this.#formatDate(period.end)
      return start || end ? `${start} – ${end}` : ''
    }))
  }

  /**
   * Returns the rows, sorted by label.
   */
  build(): OdsTableEntry[] {
    return this.#entries.sort((a, b) => a.label.localeCompare(b.label))
  }

  #entry(key: string) {
    return new OdsTableEntry(this.#t(`message.dataset_detail.properties.${key}`), key, OdsTableEntryType.Node)
  }

  #add(entry: OdsTableEntry) {
    if (entry.value.length > 0 || entry.subFields.length > 0) {
      this.#entries.push(entry)
    }
    return this
  }

  #formatDate(value: string | null | undefined) {
    if (!value) {
      return ''
    }
    // Fixed time zone, so the server and the browser render the same date.
    return new Date(value).toLocaleDateString(`${this.#lang}-CH`, { timeZone: 'Europe/Zurich' })
  }
}
