import type { LocationQueryValue } from 'vue-router'

/**
 * Normaliza valores de query string para uma string única.
 * @param value Valor bruto da query (string única ou array).
 * @returns Primeiro valor válido ou string vazia.
 */
export const getFirstQueryValue = (value: LocationQueryValue | LocationQueryValue[]): string => {
  if (Array.isArray(value)) {
    return value[0] || ''
  }
  return value || ''
}

/**
 * Valida e converte a ordenação vinda da query para o tipo aceito pela aplicação.
 * @param value Valor de ordenação vindo da URL.
 * @returns `'asc'`, `'desc'` ou `null` para entradas inválidas.
 */
export const getSortOrderFromQuery = (
  value: LocationQueryValue | LocationQueryValue[],
): 'asc' | 'desc' | null => {
  const querySort = getFirstQueryValue(value)
  if (querySort === 'asc' || querySort === 'desc') {
    return querySort
  }
  return null
}

/**
 * Converte valor de query (único ou array) em lista de inteiros válidos.
 * @param value Valor bruto vindo da query.
 * @returns Lista de números inteiros válidos.
 */
export const getQueryAsArrayOfNumbers = (value: unknown): number[] => {
  if (!value) return []
  const arr = Array.isArray(value) ? value : [value]
  return arr.map(Number).filter((n) => !isNaN(n) && Number.isInteger(n))
}
