import type { LocationQuery, LocationQueryValue } from 'vue-router'
import type { CatalogoFilters, Categoria } from '../types'
import { categorysIdFilterPerName } from './categoriaTransformer'

export const mapQueryToFilters = (query: LocationQuery): CatalogoFilters => {
  return {
    searchTerm: getFirstQueryValue(query.searchTerm),
    nome: getFirstQueryValue(query.nome),
    descricao: getFirstQueryValue(query.descricao),
    catMat: getFirstQueryValue(query.catmat),
    especificacao: getFirstQueryValue(query.especificacao),
    categoriaId: getQueryAsArrayOfNumbers(query.categoriaId),
    status: getStatusFromQuery(query.isActive),
    sortOrder: getSortOrderFromQuery(query.sortOrder),
  }
}

export const getFirstQueryValue = (value: LocationQueryValue | LocationQueryValue[]): string => {
  if (Array.isArray(value)) {
    return value[0] || ''
  }
  return value || ''
}

export const getStatusFromQuery = (value: LocationQueryValue | LocationQueryValue[]): string => {
  if (value === 'true') {
    return 'ativo'
  }
  if (value === 'false') {
    return 'inativo'
  }
  return ''
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

export const mountQueryWithPreFilterCategory = (
  queryFilters: CatalogoFilters,
  allCategories: Categoria[],
  categoryNamesFromProps: string[] | undefined,
): CatalogoFilters => {
  if (!categoryNamesFromProps || categoryNamesFromProps.length === 0) {
    return queryFilters
  }

  const propCategoriesIds = categorysIdFilterPerName(allCategories, categoryNamesFromProps)
  const urlCategories = queryFilters.categoriaId

  if (!urlCategories || urlCategories.length === 0) {
    return { ...queryFilters, categoriaId: propCategoriesIds }
  }

  const propCategoriesSet = new Set(propCategoriesIds)
  const intersection = urlCategories.filter((id) => propCategoriesSet.has(id))

  return { ...queryFilters, categoriaId: intersection }
}
