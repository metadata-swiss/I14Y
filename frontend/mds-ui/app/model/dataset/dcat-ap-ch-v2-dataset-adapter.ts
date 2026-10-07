import type { DcatDatasetModel, SearchResultModel, VocabularyEntryModel } from '~~/api-client/generated/iop-core'
import { DcatApChV2DistributionAdapter } from './dcat-ap-ch-v2-distribution-adapter'
import { PropertyTableBuilder, type Translate } from './property-table-builder'
import type { TagItem } from '~/components/OdsTagItem.vue'
import type { AppLanguage } from '~/constants/langages'
import { localize } from '~/utils/getCurrentTranslation'

/**
 * Formats of the linked data (metadata) downloads of a dataset or distribution.
 */
export type LinkedDataFormats = 'rdf' | 'ttl' | 'n3' | 'nt' | 'jsonld'

/**
 * Terms of use of a distribution. The id is the license URI, e.g. http://dcat-ap.ch/vocabulary/licenses/terms_by.
 */
export interface OdsLicense {
  id: string
  label: string
  resource: string
}

export class DcatApChV2DatasetAdapter {
  #dataset: DcatDatasetModel
  #lang: AppLanguage
  #t: Translate
  #formats: VocabularyEntryModel[]

  /**
   * @param d The dataset from iop-core.
   * @param lang The language of the texts.
   * @param t Translates the labels of the property table.
   * @param formats The formats of the dataset. Taken from the distributions if not given.
   */
  constructor(d: DcatDatasetModel, lang: AppLanguage, t: Translate, formats?: VocabularyEntryModel[]) {
    this.#dataset = d
    this.#lang = lang
    this.#t = t
    this.#formats = formats ?? (d.distributions ?? []).flatMap(distribution => distribution.format ?? [])
  }

  /**
   * Create an adapter for a search result. A search result has fewer properties than a dataset:
   * no distributions, keywords, licenses, release or modification date.
   *
   * iop-core gap [G9]: GET /api/Search returns no keywords, licenses, dct:issued or dct:modified.
   */
  static fromSearchResult(result: SearchResultModel, lang: AppLanguage, t: Translate) {
    const dataset: DcatDatasetModel = {
      id: result.id,
      identifiers: result.identifier ? [result.identifier] : [],
      title: result.title,
      description: result.description,
      publisher: result.publisher,
      themes: result.themes,
      accessRights: result.accessRights,
      system: result.system ?? {},
    }
    return new DcatApChV2DatasetAdapter(dataset, lang, t, result.formats ?? [])
  }

  /**
   * Get the id of the dataset in the portal: its DCAT identifier, which is also what opendata.swiss used in its URLs.
   * Falls back to the iop-core id.
   */
  get id() {
    return this.#dataset.identifiers?.[0] ?? this.#dataset.id ?? ''
  }

  /**
   * Get the title of the dataset. The title is mandatory but if not available it returns the id.
   *
   * from dcat-ap-ch:
   * Property	Title
   * Requirement level	Mandatory
   * Cardinality	1..n
   * URI	dct:title
   * Range	rdfs:Literal
   * Usage Note
   * This property contains a name given to the Dataset.
   * This property can be repeated for parallel language versions of the title (see 2.3 Multilingualism).
   */
  get title(): string {
    return localize(this.#dataset.title, this.#lang) || this.id
  }

  /**
   * Get the description of the dataset. A description is mandatory but if not available it returns an empty string.
   *
   * from dcat-ap-ch:
   * Property	description
   * Requirement level	Mandatory
   * Cardinality	1..n
   * URI	dct:description
   * Range	rdfs:Literal
   * Usage Note
   * This property contains a free-text account of the Dataset.
   * This property can be repeated for parallel language versions of the description (see 2.3 Multilingualism). On the user interface of data portals, the content of the element whose language corresponds to the display language selected by the user is displayed.
   */
  get description(): string | undefined {
    return localize(this.#dataset.description, this.#lang).replaceAll(/\r\n/g, '\n').trim()
  }

  /**
   * Get the categories/ themes of the dataset. We convert the categories into TagItem objects for easier handling in the UI.
   * The label is taken in the requested language. If not available, the other APP_LANGUAGES are tried as fallback.
   *
   * iop-core gap [G2]: these are the I14Y themes of the dataset. The opendata.swiss categories (EU data themes)
   * are on the DCAT catalog records of the dataset.
   *
   * Property	theme/category
   * Requirement level	Recommended
   * Cardinality	0..n
   * URI	dcat:theme
   * Range	skos:Concept
   * Usage Note
   * This property refers to a category of the Dataset. A Dataset may be associated with multiple themes.
   * CV to be used: [VOCAB-EU-THEME]
   *
   * @param lang
   * @returns {TagItem[]} The categories as TagItem array
   */
  getCategoriesForLanguage(lang: AppLanguage | string): TagItem[] {
    return (this.#dataset.themes ?? []).flatMap((theme) => {
      const label = localize(theme.name, lang)
      if (!theme.code || !label) {
        return []
      }
      return [{ id: theme.code, label } as TagItem]
    })
  }

  /**
   * Get the publisher of the dataset.
   *
   * from dcat-ap-ch:
   * Property	publisher
   * Requirement level	Mandatory
   * Cardinality	1..1
   * URI	dct:publisher
   * Range	foaf:Agent
   * Usage Note
   * - This property refers to an entity (organisation) responsible for making the Dataset available.
   *
   * @returns The identifier of the organization, its multilingual name and its homepage.
   */
  get publisher() {
    const publisher = this.#dataset.publisher
    return {
      id: publisher?.identifier ?? undefined,
      name: publisher?.name ?? {},
      homepage: publisher?.homePage ?? undefined,
    }
  }

  /**
   * Get the licenses of the dataset.
   *
   * In DCAP-AP-CH a dataset has no license, but its distributions have. This returns the licenses of the distributions, without duplicates.
   */
  get licenses(): OdsLicense[] {
    const licenses = new Map(this.distributions.flatMap(distribution => distribution.license ?? []).map(license => [license.id, license]))
    return [...licenses.values()]
  }

  /**
   * Get the release date of the dataset if available
   *
   * from dcat-ap-ch:
   * Property	release date
   * Requirement level	Recommended
   * Cardinality	0..1
   * URI	dct:issued
   * Range	rdfs:Literal (typed as as xsd:date, xsd:dateTime, xsd:gYear or xsd:gYearMonth)
   * Usage Note
   * - This property contains the date of formal issuance (e.g., first publication of the Dataset).
   * - If this date is not known, the date of the first referencing of the data collection in the Catalogue can be entered.
   */
  get releaseDate() {
    if (!this.#dataset.issued) {
      return undefined
    }
    return new Date(this.#dataset.issued)
  }

  /**
   * Get the modification date of the dataset if available
   *
   * from dcat-ap-ch:
   * Property	update/ modification date
   * Requirement level	Recommended
   * Cardinality	0..1
   * URI	dct:modified
   * Range	rdfs:Literal (typed as as xsd:date, xsd:dateTime, xsd:gYear or xsd:gYearMonth)
   * Usage Note
   * - This property contains the most recent date on which the Dataset was changed or modified.
   * - No value may indicate that the Dataset has never changed after its initial publication,
   *   or that the date of the last modification is not known, or that the Dataset is continuously updated
   * - This property MUST only be set if the distributions (the actual data) that the Dataset describes
   *   have been updated after it has been issued. In this case the property MUST contain the date of the last update.
   *   That way a person or institution using the data for an analysis or application will know when to update the
   *   report or application on their side.
   */
  get modificationDate() {
    if (!this.#dataset.modified) {
      return undefined
    }
    return new Date(this.#dataset.modified)
  }

  /**
   * Get the paths of the linked data (metadata) downloads.
   *
   * iop-core gap [G6]: iop-core has no DCAT export of a single dataset, so there are no metadata downloads.
   */
  get getLinkedData(): Record<LinkedDataFormats, string> {
    return {} as Record<LinkedDataFormats, string>
  }

  /**
   * Returns the distributions wrapped in DistributionAdapter instances
   *
   * @returns {DistributionAdapter[]} An array of DistributionAdapter instances
   */
  get distributions() {
    return (this.#dataset.distributions ?? []).map(d => new DcatApChV2DistributionAdapter(d, this, this.#lang, this.#t))
  }

  /**
   * Get the available formats of the dataset, without duplicates.
   */
  get getOdsFormats(): { id?: string | null | undefined, label?: string | null | undefined, resource?: string | null | undefined }[] {
    const formats = new Map(this.#formats.filter(format => format.code).map(format => [format.code, {
      id: format.code,
      label: localize(format.name, this.#lang) || format.code,
      resource: format.uri,
    }]))
    return [...formats.values()]
  }

  get formats(): TagItem[] {
    return this.getOdsFormats.map((format) => {
      const tagItem = {
        id: format.id,
        label: format.label,
        size: 'ods',
        variant: 'default',
      } as unknown as TagItem
      return tagItem
    })
  }

  /**
   * Get the keywords of the dataset in the language of the adapter.
   * We convert the keywords into TagItem objects for easier handling in the UI.
   *
   * from dcat-ap-ch:
   * Property	keyword/ tag
   * Requirement level	Recommended
   * Cardinality	0..n
   * URI	dcat:keyword
   * Range	rdfs:Literal
   * Usage Note
   * This property contains a keyword or tag describing the Dataset.
   * If a suitable keyword is available in [TERMDAT] then this SHOULD be used.
   * Good practice: mark the language of the keywords with the [ISO 639-1] language code such as "geodata"@en.
   */
  get keywords(): TagItem[] {
    return (this.#dataset.keywords ?? [])
      .flatMap((keyword) => {
        // Like piveau, only the keywords in the current language are shown.
        const label = keyword.label?.[this.#lang]
        if (!label) {
          return []
        }
        const tagItem = {
          id: keyword.uri ?? label,
          label,
          size: 'sm',
        } as TagItem
        return [tagItem]
      })
      .sort((a, b) => (a.label ?? '').localeCompare(b.label ?? ''))
  }

  /**
   * Get the catalog information of the dataset. Only the publisher is used by the components.
   */
  get catalog() {
    return {
      publisher: {
        id: this.publisher.id ?? '',
        name: localize(this.#dataset.publisher?.name, this.#lang),
      },
    }
  }

  /**
   * Get the accrual periodicity of the dataset if available
   *
   * from dcat-ap-ch:
   * Property	frequency
   * Requirement level	Optional
   * Cardinality	0..1
   * URI	dct:accrualPeriodicity
   * Range	dct:Frequency
   * Usage Note
   * - This property refers to the frequency at which the Dataset is updated.
   * - CV to be used: [VOCAB-EU-FREQUENCY].
   */
  get frequency() {
    const frequency = this.#dataset.frequency
    if (!frequency) {
      return undefined
    }
    return {
      resource: frequency.uri ?? '',
      label: localize(frequency.name, this.#lang) || frequency.code || '',
    }
  }

  frequencyForLanguage(lang: string) {
    const frequency = this.#dataset.frequency
    if (!frequency) {
      return undefined
    }
    return localize(frequency.name, lang) || frequency.code || undefined
  }

  get propertyTable() {
    const d = this.#dataset
    return new PropertyTableBuilder(this.#lang, this.#t)
      .strings('identifiers', d.identifiers)
      .links('landing_pages', d.landingPages)
      .contactPoints('contact_points', d.contactPoints)
      .vocabulary('languages', d.languages)
      .strings('spatial', d.spatial)
      .periods('temporal_coverage', d.temporalCoverage)
      .vocabulary('access_rights', [d.accessRights])
      .links('conforms_to', d.conformsTo)
      .links('documentation', d.documentation)
      .links('relations', d.relations)
      .links('is_referenced_by', d.isReferencedBy)
      .strings('version', [d.version])
      .strings('version_notes', [localize(d.versionNotes, this.#lang)])
      .build()
  }
}
