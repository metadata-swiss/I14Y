import type { LinkedDataFormats } from '@piveau/sdk-vue'
import type { Dataset } from '../../../model/dataset'
import type { DcatApChV2DatasetAdapter } from './dcat-ap-ch-v2-dataset-adapter'
import { OdsTableEntry, OdsTableEntryType } from './table-entry'
import type { OdsLicense } from '~/piveau/get-ods-licenses'

type EnhancedDistribution = Dataset['getDistributions'][number]

export class DcatApChV2DistributionAdapter {
  #distribution: EnhancedDistribution | undefined
  #dataset: DcatApChV2DatasetAdapter

  constructor(d: EnhancedDistribution, dataset: DcatApChV2DatasetAdapter) {
    this.#distribution = d
    this.#dataset = dataset
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
   * Language handling is done by peiveau.
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
    return this.#distribution?.title || this.#dataset.title
  }

  /**
   * Get the description of the distribution. If not available, return the description of the dataset.
   *
   * It returns the description of the distribution if available, otherwise it returns an empty string.
   * Language handling is done by peiveau.
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
    return (this.#distribution?.description ?? '').replaceAll(/\r\n/g, '\n').trim() || this.#dataset.description || ''
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
    const byteSizeNode = this.#distribution?.getPropertyTable.find(node => node.id === 'byteSize')

    if (!byteSizeNode || !byteSizeNode.data?.length) {
      return ''
    }

    const byteSizeValue = byteSizeNode.data[0]?.data
    if (!byteSizeValue || typeof byteSizeValue !== 'string') {
      return ''
    }

    // Try to parse the byte size as a number.
    // Piveau returns it as a string with dots as thousands separators which is not valid for Number()
    const byteSizeNumber = Number(byteSizeValue.replace(/\./g, '').replace(/,/g, '').replace(/\s+/g, '')) // Remove spaces for thousands separators

    if (isNaN(byteSizeNumber)) {
      return byteSizeValue // Return the raw value if parsing fails
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
    return this.#distribution?.downloadUrls || []
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
    return this.#distribution?.accessUrls || []
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
      return format
    }
    // Fallback: get mediaType from property table
    const mediaTypeNode = this.#distribution?.getPropertyTable.find(node => node.id === 'mediaType')
    if (!mediaTypeNode || !mediaTypeNode.data?.length) {
      return ''
    }
    const mediaType = mediaTypeNode?.data[0]?.data ?? ''
    if (typeof mediaType !== 'string') {
      return ''
    }
    return mediaType
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
  get license() {
    const lic = this.#distribution?.license
    if (lic) {
      return lic as OdsLicense
    }
    return undefined
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
   * Note: This is not part of dcat-ap-ch, but added by piveau
   */
  get id() {
    return this.#distribution?.id || ''
  }

  /**
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
    return this.#distribution?.languages || []
  }

  get getLinkedData(): Record<LinkedDataFormats, string> {
    return this.#distribution?.getLinkedData ?? {
      rdf: '',
      ttl: '',
      n3: '',
      nt: '',
      jsonld: '',
    }
  }

  get propertyTable() {
    const rootNode = this.#distribution?.getPropertyTable
    if (!rootNode) {
      return []
    }

    // const ignoredNode = ['catalogRecord']
    const nodesToConsider = rootNode.filter(n => n.data) // .filter(n => !ignoredNode.includes(n.id))

    const newTableEntries: OdsTableEntry[] = []

    for (const node of nodesToConsider) {
      if (!(node.type === 'node') || !node.data) {
        continue
      }
      const newTableEntry = new OdsTableEntry(node.label, node.id, OdsTableEntryType.Node)
      newTableEntries.push(newTableEntry)
      if (node.id === 'byteSize') {
        newTableEntry.addPiveauPropertyTableEntry([{ type: 'value', id: 'byteSize_value', data: this.formattedByteSize }])
      }
      else {
        newTableEntry.addPiveauPropertyTableEntry(node.data || [])
      }
    }
    return newTableEntries.sort((a, b) => a.label.localeCompare(b.label))
  }
}
