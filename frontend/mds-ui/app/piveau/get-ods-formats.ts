import type { Dataset, Format } from '@piveau/sdk-core'

/**
 * Extract unique formats from dataset distributions.
 * This was needed because the original format extraction contained duplicates and was an array of strings only.
 *
 * @param dataset dataset
 * @returns Array of unique formats with id, label and resource
 */
export function getOdsFormats(dataset: Dataset) {
  const distributions = (dataset?.distributions ?? [])

  return distributions.reduce((acc, distribution) => {
    const distFormat = distribution.format
    if (distFormat && distFormat.id && distFormat.label && distFormat.resource) {
      if (acc.some(item => item.id === distFormat.id)) {
        return acc
      }
      acc.push({
        label: distFormat.label,
        resource: distFormat.resource,
        id: distFormat.id,
      })
    }
    return acc
  }, [] as Format[])
}
