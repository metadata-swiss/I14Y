import type { LinkedDataFormats } from '@piveau/sdk-vue'
import type { Dataset } from '../../../model/dataset'
import { DcatApChV2DistributionAdapter } from './dcat-ap-ch-v2-distribution-adapter'
import { OdsTableEntry, OdsTableEntryType } from './table-entry'
import type { Catalog } from '~/piveau/get-ods-catalog-info'
import type { TagItem } from '../../OdsTagItem.vue'
import type { AppLanguage } from '~/constants/langages'
import { APP_LANGUAGES } from '~/constants/langages'

export class DcatApChV2DatasetAdapter {
  #dataset: Dataset

  constructor(d: Dataset) {
    this.#dataset = d
  }

  /**
   * Get the id of the dataset
   *
   */
  get id() {
    return this.#dataset.getId
  }

  /**
   * Get the title of the dataset. The title is mandatory but if not available it returns the id.
   * The language handling is done by piveau.
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
    return this.#dataset.getTitle ?? this.id
  }

  /**
   * Get the description of the dataset. A description is mandatory but if not available it returns an empty string.
   *
   * The language handling is done by piveau.
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
    return (this.#dataset.getDescription ?? '').replaceAll(/\r\n/g, '\n').trim()
  }

  /**
   * Get the categories/ themes of the dataset. We convert the categories into TagItem objects for easier handling in the UI.
   * The language handling is done here, because piveau returns all languages in the label object. We try to get the label in the requested language.
   * If not available, we try the other APP_LANGUAGES as fallback.
   * If still not available, we take any available label.
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
  getCategoriesForLanguage(lang: AppLanguage): TagItem[] {
    const categories = this.#dataset?.getCategories ?? []
    const tagItems = categories.flatMap((cat) => {
      const categoryLabes = cat.label
      if (!categoryLabes) {
        // some datasets are using unknown vacabularies
        // warn and drop them
        console.warn(`No labels for category\ndataset title: ${this.#dataset.getTitle}\ndataset: ${this.#dataset.getId}\nid: ${cat.id}\nresource: <${cat.resource}>\nlabels: ${cat.label}`)
        return []
      }
      const preferredLabel = this.getLabelByLangagePrecedence(lang, cat)
      if (!preferredLabel) {
        return []
      }

      const tagItem = {
        id: cat.id,
        label: preferredLabel,
      } as TagItem
      return [tagItem]
    })

    return tagItems
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
   */
  get publisher() {
    // the interface in piveau is wrong.
    // i get this ...
    // {
    //   name: "Bundesamt für Lebensmittelsicherheit und Veterinärwesen BLV",
    //   resource: "https://www.blv.admin.ch",
    //   type: "Agent",
    // }

    return this.#dataset.getPublisher as unknown as {
      id: string | undefined
      name: Record<string, string>
      homepage: string | undefined
    }
  }

  /**
   * Get the licenses of the dataset.
   *
   * In DCAP-AP-CH a dataset has no license, but its distributions have. However, in piveau the dataset
   * has a getLicenses property that aggregates the licenses of its distributions.
   *
   */
  get licenses() {
    return this.#dataset.getOdsLicenses ?? []
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
    if (!this.#dataset.getIssued) {
      return undefined
    }
    return new Date(this.#dataset.getIssued)
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
    if (!this.#dataset.getModified) {
      return undefined
    }
    return new Date(this.#dataset.getModified)
  }

  get getLinkedData(): Record<LinkedDataFormats, string> {
    return this.#dataset?.getLinkedData ?? {} as Record<LinkedDataFormats, string>
  }

  /**
   * Returns the distributions wrapped in DistributionAdapter instances
   *
   * @returns {DistributionAdapter[]} An array of DistributionAdapter instances
   */
  get distributions() {
    return this.#dataset.getDistributions.map(d => new DcatApChV2DistributionAdapter(d, this))
  }

  /**
   * Get the available formats of the dataset
   */
  get getOdsFormats(): { id?: string | null | undefined, label?: string | null | undefined, resource?: string | null | undefined }[] {
    return this.#dataset?.getOdsFormats ?? []
  }

  get formats(): TagItem[] {
    return (this.#dataset?.getOdsFormats ?? []).map((format) => {
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
   * Get the keywords of the dataset. The language handling is done by piveau.
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
    return (this.#dataset?.getKeywords ?? [])
      .map((keyword) => {
        const tagItem = {
          id: keyword.id,
          label: keyword.label,
          size: 'sm',
        } as TagItem
        return tagItem
      })
      .sort((a, b) => (a.label ?? '').localeCompare(b.label ?? ''))
  }

  /**
   * Get the catalog information of the dataset if available
   */
  get catalog(): Catalog {
    return this.#dataset.getOdsCatalogInfo
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
    return this.#dataset.getOdsAccrualPeriodicity
  }

  frequencyForLanguage(lang: string) {
    const frequencyResource = this.frequency
    if (!frequencyResource) {
      return undefined
    }
    if (!frequencyResource.label) {
      return undefined
    }
    return this.getLabelByLangagePrecedence(lang, frequencyResource as unknown as { label?: Record<string, string | undefined> | undefined })
  }

  get propertyTable() {
    const rootNode = this.#dataset.getPropertyTable

    if (!rootNode) {
      return []
    }

    const ignoredNode = ['catalogRecord', 'accrualPeriodicity']
    const nodesToConsider = rootNode.filter(n => n.data).filter(n => !ignoredNode.includes(n.id))

    const newTableEntries: OdsTableEntry[] = []

    for (const node of nodesToConsider) {
      if (!(node.type === 'node') || !node.data) {
        continue
      }

      const newTableEntry = new OdsTableEntry(node.label, node.id, OdsTableEntryType.Node)
      newTableEntries.push(newTableEntry)
      newTableEntry.addPiveauPropertyTableEntry(node.data || [])
    }
    return newTableEntries.sort((a, b) => a.label.localeCompare(b.label))
  }

  /**
   * Get the label with langage precedence.
   * @param lang the langage
   * @param resource  the available labels
   * @returns a string of the best label candidate available
   */
  private getLabelByLangagePrecedence(
    lang: string,
    resource: { label?: Record<string, string | undefined> | undefined },
  ): string {
    const labels = resource.label
    if (!labels) {
      return ''
    }

    let preferredLabel = labels[lang]
    if (!preferredLabel) {
      for (const fallbackLang of APP_LANGUAGES) {
        if (labels[fallbackLang]) {
          preferredLabel = labels[fallbackLang]
          break
        }
      }
    }

    if (!preferredLabel) {
      preferredLabel = Object.values(labels)[0] ?? ''
    }

    return preferredLabel
  }
}
