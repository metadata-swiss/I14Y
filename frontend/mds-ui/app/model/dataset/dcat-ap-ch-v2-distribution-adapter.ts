import type { DcatDistributionModel } from '~~/api-client/generated/iop-core'
import type { DcatApChV2DatasetAdapter, LinkedDataFormats, OdsLicense } from './dcat-ap-ch-v2-dataset-adapter'
import { PropertyTableBuilder, type Translate } from './property-table-builder'
import type { AppLanguage } from '~/constants/langages'
import { localize } from '~/utils/getCurrentTranslation'

export class DcatApChV2DistributionAdapter {
  #distribution: DcatDistributionModel | undefined
  #dataset: DcatApChV2DatasetAdapter
  #lang: AppLanguage
  #t: Translate

  constructor(d: DcatDistributionModel, dataset: DcatApChV2DatasetAdapter, lang: AppLanguage, t: Translate) {
    this.#distribution = d
    this.#dataset = dataset
    this.#lang = lang
    this.#t = t
  }

  /**
   * Get the dataset this distribution belongs to
   */
  get dataset() {
    return this.#dataset
  }

  /**
   * Get the title of the distribution.
   *
   * It return the title of the distribution if available, otherwise it returns the title of the dataset. The title on a distribution is not mandatory if
   * the distribution contains the whole dataset.
   *
   * From dcat-ap-ch:
   * Property	Title
   * Requirement level	Recommended
   * Cardinality	0..n
   * URI	dct:title
   * Range	rdfs:Literal
   * Usage Note
   * - This property contains a name given to the Distribution. This property can be repeated for parallel language versions of the description (see 2.3 Multilingualism).
   * - The title MUST be given if the distribution contains only part of the data offered by the Dataset
   * - The title can be given in several languages. In multilingual data portals, the title in the language selected by a user will usually be shown as title for the distribution.
   */
  get title() {
    return localize(this.#distribution?.title, this.#lang) || this.#dataset.title
  }

  /**
   * Get the description of the distribution. If not available, return the description of the dataset.
   *
   * It returns the description of the distribution if available, otherwise it returns an empty string.
   *
   * Property	description
   * Requirement level	Recommended
   * Cardinality	0..n
   * URI	dct:description
   * Range	rdfs:Literal
   * Usage Note
   * - This property contains a free-text account of the Distribution.
   * - The description MUST be provided if the distribution contains only part of the data offered by the Dataset.
   * - This property can be repeated for parallel language versions of the description (see 2.3 Multilingualism).
   *
   */
  get description() {
    return localize(this.#distribution?.description, this.#lang).replaceAll(/\r\n/g, '\n').trim() || this.#dataset.description || ''
  }

  /**
   * Get the formattedByteSize size of the distribution if available
   *
   * from dcat-ap-ch:
   * Property	byte size
   * Requirement level	Optional
   * Cardinality	0..1
   * URI	dcat:byteSize
   * Range	rdfs:Literal (typed as as xsd:decimal).
   * Usage Note
   * This property contains the size of a Distribution in bytes. If the precise size is not known, an approximate size can be indicated.
   */
  get formattedByteSize() {
    const byteSizeNumber = this.#distribution?.byteSize

    if (byteSizeNumber === undefined || byteSizeNumber === null || isNaN(byteSizeNumber)) {
      return ''
    }

    if (byteSizeNumber === 0) {
      return '0 B'
    }

    const units = ['B', 'KB', 'MB', 'GB', 'TB']
    let size = byteSizeNumber
    let unitIndex = 0

    while (size >= 1024 && unitIndex < units.length - 1) {
      size /= 1024
      unitIndex++
    }

    return `${size % 1 === 0 ? size : size.toFixed(2)} ${units[unitIndex]}`
  }

  /**
   * Get the download URLs of the distribution.
   *
   * From dcat-ap-ch:
   * Property	download URL
   * Requirement level	Optional
   * Cardinality	0..n
   * URI	dcat:downloadURL
   * Range	rdfs:Resource
   * Usage Note
   * - In case of a downloadable file, it is good practice to repeat the mandatory accessURL in this more specific property,
   *   to indicate to the data user that the distribution has this extra characteristic of being downloadable. The downloadURLs
   *   MAY thus be the same as the accessURLs but they MAY also differ.
   */
  get downloadUrls() {
    const uri = this.#distribution?.downloadUrl?.uri
    return uri ? [uri] : []
  }

  /**
   * Get the access URLs of the distribution.
   *
   * From dcat-ap-ch:
   * Property	access URL
   * Requirement level	Mandatory
   * Cardinality	1..n
   * URI	dcat:accessURL
   * Range	rdfs:Resource
   * Usage Note
   * - This property contains a URL that gives access to a Distribution of the Dataset. The resource at the access URL may contain
   *   information about how to get the Dataset.
   */
  get accessUrls() {
    const uri = this.#distribution?.accessUrl?.uri
    return uri ? [uri] : []
  }

  /**
   * Get the format of the distribution.
   *
   * From dcat-ap-ch:
   * Property	format
   * Requirement level	Recommended
   * Cardinality	0..1
   * URI	dct:format
   * Range	dct:MediaTypeOrExtent
   * Usage Note
   * - This property refers to the file format of the Distribution.CV to be used: [VOCAB-EU-FILE-TYPE]
   * - CV to be used: [VOCAB-EU-FILE-TYPE]
   * - If a format is not available:
   *   a) media type ([IANA-MEDIA-TYPES]) should be used
   *   b) If necessary, a discussion to evaluate the adoption within the EU should be launched (Contact point: [VOCAB-EU-OP-CONTACT]).
   */
  get format(): string {
    const format = this.#distribution?.format
    if (format) {
      return localize(format.name, this.#lang) || format.code || ''
    }
    // Fallback: the media type
    return this.#distribution?.mediaType?.code ?? ''
  }

  /**
   * Get the license of the distribution if available
   *
   * From dcat-ap-ch:
   * Property	license
   * Requirement level	Mandatory
   * Cardinality	1..1
   * URI	dct:license
   * Range	dct:LicenseDocument
   * Usage Note
   * This property refers to the licence under which the Distribution is made available.
   * CV to used: [VOCAB-CH-LICENSE]
   */
  get license(): OdsLicense | undefined {
    const lic = this.#distribution?.license
    const id = lic?.uri ?? lic?.code
    if (!lic || !id) {
      return undefined
    }
    return {
      id,
      label: localize(lic.name, this.#lang) || id,
      resource: lic.uri ?? id,
    }
  }

  /**
   * Get the release date of the distribution if available
   * Property	release date
   * Requirement level	Optional
   * Cardinality	0..1
   * URI	dct:issued
   * Range	rdfs:Literal (typed as as xsd:date, xsd:dateTime, xsd:gYear or xsd:gYearMonth)
   * Usage Note
   * - This property contains the date of formal issuance (e.g., publication) of the Distribution.
   * - Date of formal issuance (publication) of the distribution
   * - UsageThe first time issuance of the distribution.
   *
   * @returns {Date | undefined} The release date as a Date object, or undefined if not available or invalid.
   */
  get releaseDate() {
    const releaseDateString = this.#distribution?.issued || ''
    if (!releaseDateString) {
      return undefined
    }
    const releaseDate = new Date(releaseDateString)
    return isNaN(releaseDate.getTime()) ? undefined : releaseDate
  }

  /**
   * Get the modified date of the distribution if available
   *
   * from dcat-ap-ch:
   *  Property: update/modification date
   * This property contains the most recent date on which the Distribution was changed or modified.
   * Property	update/ modification date
   * Requirement level	Recommended
   * Cardinality	0..1
   * URI	dct:modified
   * Range	rdfs:Literal (typed as as xsd:date, xsd:dateTime, xsd:gYear or xsd:gYearMonth)
   * Usage Note
   *
   * @returns {Date | undefined} The modified date as a Date object, or undefined if not available or invalid.
   */
  get modificationDate() {
    const modifiedDateString = this.#distribution?.modified || ''
    if (!modifiedDateString) {
      return undefined
    }
    const modifiedDate = new Date(modifiedDateString)
    return isNaN(modifiedDate.getTime()) ? undefined : modifiedDate
  }

  /**
   * Get the id of the distribution
   *
   * Note: This is the iop-core id, used in the URL of the distribution page.
   */
  get id() {
    return this.#distribution?.id || ''
  }

  /**
   * Get the names of the languages of the distribution.
   *
   * Property	Language
   * Requirement level	Optional
   * Cardinality	0..n
   * URI	dct:language
   * Range	rdfs:Literal
   * Usage Note
   * - This property refers to a language used in the Distribution.
   * - This property can be repeated if the metadata is provided in multiple languages.
   * - The property MUST be set if the distribution is language-dependent or if it is given in some of the languages
   *   German, French, Italian and English but not in all four languages.
   * - CV to be used: [VOCAB-EU-LANGUAGE]
   */
  get languages() {
    return (this.#distribution?.languages ?? []).map(language => localize(language.name, this.#lang) || language.code || '').filter(Boolean)
  }

  /**
   * Get the paths of the linked data (metadata) downloads.
   *
   * iop-core gap [G6]: iop-core has no DCAT export of a single distribution, so there are no metadata downloads.
   */
  get getLinkedData(): Record<LinkedDataFormats, string> {
    return {
      rdf: '',
      ttl: '',
      n3: '',
      nt: '',
      jsonld: '',
    }
  }

  get propertyTable() {
    const d = this.#distribution
    const checksum = d?.checksum?.checksumValue
      ? `${localize(d.checksum.algorithm?.name, this.#lang) || d.checksum.algorithm?.code || ''} ${d.checksum.checksumValue}`.trim()
      : undefined
    return new PropertyTableBuilder(this.#lang, this.#t)
      .strings('identifiers', [d?.identifier])
      .links('access_url', [d?.accessUrl])
      .links('download_url', [d?.downloadUrl])
      .vocabulary('format', [d?.format])
      .vocabulary('media_type', [d?.mediaType])
      .vocabulary('packaging_format', [d?.packagingFormat])
      .strings('byte_size', [this.formattedByteSize])
      .strings('checksum', [checksum])
      .vocabulary('languages', d?.languages)
      .vocabulary('license', [d?.license])
      .strings('rights', [d?.rights])
      .vocabulary('availability', [d?.availability])
      .links('conforms_to', d?.conformsTo)
      .links('documentation', d?.documentation)
      .periods('temporal_coverage', d?.coverage)
      .strings('temporal_resolution', [d?.temporalResolution])
      .strings('spatial_resolution', [d?.spatialResolution === undefined || d?.spatialResolution === null ? undefined : `${d.spatialResolution} m`])
      .build()
  }
}
