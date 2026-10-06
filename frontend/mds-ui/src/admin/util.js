export function toArray(values) {
  if (Array.isArray(values)) {
    return values
  }
  else if (values) {
    return [...values].map(item => ({
      id: item.get('id'),
      label: item.get('label'),
    }))
  }

  return []
}
