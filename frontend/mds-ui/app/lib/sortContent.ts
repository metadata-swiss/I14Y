interface Sortable {
  after?: string | null
}

export function sortContent<T extends Sortable>(items: T[] | undefined, getSlug: (item: T) => string | undefined) {
  const sortedItems = items?.slice() || []
  const maxIterations = sortedItems.length
  for (let iter = 0; iter < maxIterations; iter++) {
    let changed = false
    for (let i = 0; i < sortedItems.length; i++) {
      const { after } = sortedItems[i] || {}
      if (after) {
        const targetIndex = sortedItems.findIndex(p => getSlug(p) === after)
        if (targetIndex !== -1 && targetIndex > i) {
          const aToMove = sortedItems.splice(i, 1)[0]!
          // Find target again because index shifted if i < targetIndex
          const newTargetIndex = sortedItems.findIndex(p => getSlug(p) === after)
          sortedItems.splice(newTargetIndex + 1, 0, aToMove)
          changed = true
          break
        }
      }
    }
    if (!changed) break
  }

  return sortedItems
}
