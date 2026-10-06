import type { Dataset } from '@piveau/sdk-core'

/**
 * Get Licenses for a dataset.
 *
 * @param dataset dataset
 * @returns Array of unique licenses with id, label and resource
 */
export function getOdsLicenses(dataset: Dataset) {
  const licenses = dataset.distributions?.flatMap(distribution => distribution.license ?? []) ?? []
  const uniqueLicenses: Record<string, OdsLicense> = {}
  licenses.forEach((license) => {
    if (typeof license === 'string') {
      uniqueLicenses[license] = { id: license, label: license, resource: license }
    }
    else if (license.id) {
      uniqueLicenses[license.id] = {
        id: license.id,
        label: license.label ?? license.id,
        resource: license.resource ?? license.id,
      }
    }
  })
  const licensesArray = Object.values(uniqueLicenses)
  return licensesArray
}

export interface OdsLicense {
  id: string
  label: string
  resource: string
}
