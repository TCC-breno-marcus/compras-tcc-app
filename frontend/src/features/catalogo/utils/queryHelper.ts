import type { LocationQuery, LocationQueryValue } from 'vue-router'
import type { CatalogoFilters, Categoria } from '../types'
import { categorysIdFilterPerName } from './categoriaTransformer'
import {
  getFirstQueryValue,
  getQueryAsArrayOfNumbers,
  getSortOrderFromQuery,
} from '@/utils/queryHelper'

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
    pageNumber: getFirstQueryValue(query.pageNumber),
    pageSize: getFirstQueryValue(query.pageSize),
  }
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

/**
 * Aplica filtros pré-definidos (vindas de props) sobre os filtros da URL.
 * @param filtersFromUrl Filtros extraídos da query da URL.
 * @param allCategories Lista completa de categorias para fazer a tradução de nome para ID.
 * @param preFilters Um objeto contendo os valores dos pré-filtros vindos das props.
 * @returns Um objeto CatalogoFilters final e pronto para ser enviado à API.
 */
export const applyPreFilters = (
  filtersFromUrl: CatalogoFilters,
  allCategories: Categoria[],
  preFilters: {
    categoryNames?: string[]
    status?: string
  },
): CatalogoFilters => {
  let finalFilters = { ...filtersFromUrl }

  if (preFilters.categoryNames && preFilters.categoryNames.length > 0) {
    const propCategoriesIds = categorysIdFilterPerName(allCategories, preFilters.categoryNames)
    const urlCategoriesIds = filtersFromUrl.categoriaId

    if (!urlCategoriesIds || urlCategoriesIds.length === 0) {
      finalFilters.categoriaId = propCategoriesIds
    } else {
      const propCategoriesSet = new Set(propCategoriesIds)
      const intersection = urlCategoriesIds.filter((id) => propCategoriesSet.has(id))
      finalFilters.categoriaId = intersection
    }
  }

  if (preFilters.status) {
    finalFilters.status = preFilters.status
  }

  return finalFilters
}
