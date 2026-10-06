import type { PropertyTableEntry, PropertyTableEntryNode } from '@piveau/sdk-vue'

/**
 * This class represents an entry in the property table of a dataset. It can be a value or a node. A node can have subfields and values.
 * The value of an entry can be a string, a date, an email, a telephone number or a href.
 */
export class OdsTableEntry {
  label: string
  id: string
  subFields: OdsTableEntry[] = []
  nodeType: OdsTableEntryType
  value: OdsTableEntry[] = []

  /**
   * Create a new table entry.
   * @param label The label of the entry.
   * @param id The id of the entry. This is used for nodes to identify them, but can be empty for value entries.
   * @param nodeType The type of the entry. This is used to determine how to display the entry in the UI.
   */
  constructor(label: string, id: string, nodeType: OdsTableEntryType) {
    this.label = label
    this.id = id
    this.nodeType = nodeType
  }

  /**
   * Add entries from piveau to the table entry. This method is recursive for nodes.
   * It also cleans up duplicates for email and date values.
   * @param entries The entries from piveau to add to the table entry.
   */
  addPiveauPropertyTableEntry(entries: (PropertyTableEntry | PropertyTableEntryNode)[]): void {
    for (const entry of entries) {
      if (entry.type === 'node') {
        const node = entry as PropertyTableEntryNode
        const newTableEntry = new OdsTableEntry(node.label, node.id, OdsTableEntryType.Node)
        newTableEntry.addPiveauPropertyTableEntry(node.data || [])
        this.#addSubField(newTableEntry)
        continue
      }
      // this is a value entry
      const valueEntry = entry as PropertyTableEntry
      if (valueEntry.type === 'value') {
        const isDate = !isNaN(Date.parse(valueEntry.data as string))
        if (isDate) {
          this.#addValue(new OdsTableEntry(valueEntry.data as string, '', OdsTableEntryType.Date))
        }
        const isEmail = typeof valueEntry.data === 'string' && /^\S+@\S+\.\S+$/.test(valueEntry.data)
        if (isEmail) {
          this.#addValue(new OdsTableEntry(valueEntry.data as string, `mailto:${valueEntry.data}`, OdsTableEntryType.Email))
        }
        const isPhone = typeof valueEntry.data === 'string' && /^\+?[0-9\s\-()]+$/.test(valueEntry.data)
        if (isPhone) {
          this.#addValue(new OdsTableEntry(valueEntry.data as string, `tel:${valueEntry.data}`, OdsTableEntryType.Telephone))
        }
        else {
          this.#addValue(new OdsTableEntry(valueEntry.data as string, '', OdsTableEntryType.String))
        }
      }
      else if (valueEntry.type === 'href') {
        const hrefData = valueEntry.data as { label: string, href: string }
        this.#addValue(new OdsTableEntry(hrefData.label, hrefData.href, OdsTableEntryType.Href))
      }
      else {
        console.warn('--------------------------------------------------------------')
        console.warn('Unknown entry type:', valueEntry.type, 'for entry:', valueEntry)
        console.warn('--------------------------------------------------------------')
      }
    }
    this.#cleanEmailDuplicates()
    this.#cleanDateDuplicates()
  }

  #addValue(value: OdsTableEntry) {
    this.value.push(value)
  }

  #addSubField(subField: OdsTableEntry) {
    this.subFields.push(subField)
  }

  blockType(): 'block' | 'row' | 'tree' {
    if (this.subFields.filter(f => f.value.length > 0).length > 0) {
      return 'row'
    }
    if (this.subFields.filter(f => f.subFields.length > 0).length > 0) {
      return 'tree'
    }
    return 'block'
  }

  /**
   * This method removes string values that have the same label as email values, as they are duplicates. It is called after adding all entries to the table entry.
   */
  #cleanEmailDuplicates() {
    const emailValues = this.value.filter(v => v.nodeType === OdsTableEntryType.Email)
    for (const emailValue of emailValues) {
      // remove string values with the same label as the email value
      this.value = this.value.filter(v => !(v.nodeType === OdsTableEntryType.String && v.label === emailValue.label))
    }
    this.subFields.forEach(subField => subField.#cleanEmailDuplicates())
  }

  /**
   * This method removes string values that have the same label as date values, as they are duplicates. It is called after adding all entries to the table entry.
   */
  #cleanDateDuplicates() {
    const dateValues = this.value.filter(v => v.nodeType === OdsTableEntryType.Date)
    for (const dateValue of dateValues) {
      // remove string values with the same label as the date value
      this.value = this.value.filter(v => !(v.nodeType === OdsTableEntryType.String && v.label === dateValue.label))
    }
    this.subFields.forEach(subField => subField.#cleanDateDuplicates())
  }
}
/**
 * This enum represents the type of an entry in the property table of a dataset. It can be a string, a date, an email, a telephone number, a href or a node.
 */
export enum OdsTableEntryType {
  Date = 'date',
  String = 'string',
  Href = 'href',
  Email = 'email',
  Telephone = 'telephone',
  Node = 'node',
  Metadata = 'metadata',
}
