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
   * Add a value to the table entry.
   * @param label The text shown.
   * @param nodeType The type of the value.
   * @param id The link target of href values.
   * @returns this table entry, so calls can be chained.
   */
  addValue(label: string, nodeType: OdsTableEntryType, id = ''): OdsTableEntry {
    this.#addValue(new OdsTableEntry(label, id, nodeType))
    return this
  }

  /**
   * Add a sub field to the table entry, if it has values or sub fields itself.
   * @param subField The sub field.
   * @returns this table entry, so calls can be chained.
   */
  addSubField(subField: OdsTableEntry): OdsTableEntry {
    if (subField.value.length > 0 || subField.subFields.length > 0) {
      this.#addSubField(subField)
    }
    return this
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
