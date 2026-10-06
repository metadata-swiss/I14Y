import type { Dataset } from '@piveau/sdk-core'

interface Resource {
  resource: string
  label: string
}

/**
 * Get accrual periodicity from an enriched dataset.
 *
 * Property	frequency
 * Requirement level	Optional
 * Cardinality	0..1
 * URI	dct:accrualPeriodicity
 * Range	dct:Frequency
 * Usage Note
 * This property refers to the frequency at which the Dataset is updated.
 * CV to be used: [VOCAB-EU-FREQUENCY].
 *
 * accrual_periodicity: {
 *     resource: "http://publications.europa.eu/resource/authority/frequency/DAILY",
 *     label: "daily"
 * }
 * @param dataset dataset
 * @returns Catalog information (id, modified, issued, title, description, record: {modified, issued})
 */
export function getOdsAccrualPeriodicity(dataset: Dataset): Resource | undefined {
  const accrualPeriodicity = dataset?.accrual_periodicity

  if (!accrualPeriodicity) {
    return undefined
  }

  const resource = accrualPeriodicity.resource
  const label = accrualPeriodicity.label

  if (!resource || !label) {
    return undefined
  }

  return {
    resource,
    label,
  }
}
