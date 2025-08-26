import type { LocationQueryValue } from 'vue-router'

export const getFirstQueryValue = (value: LocationQueryValue | LocationQueryValue[]): string => {
  if (Array.isArray(value)) {
    return value[0] || ''
  }
  return value || ''
}


export const getSortOrderFromQuery = (
  value: LocationQueryValue | LocationQueryValue[],
): 'asc' | 'desc' | null => {
  const querySort = getFirstQueryValue(value)
  if (querySort === 'asc' || querySort === 'desc') {
    return querySort
  }
  return null
}

export const getQueryAsArrayOfNumbers = (value: unknown): number[] => {
  if (!value) return []
  const arr = Array.isArray(value) ? value : [value]
  return arr.map(Number).filter((n) => !isNaN(n) && Number.isInteger(n))
}

