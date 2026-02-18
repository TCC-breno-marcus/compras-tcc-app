import type { LocationQuery } from 'vue-router'
import { getFirstQueryValue, getSortOrderFromQuery } from '@/utils/queryHelper'
import type { ItemsDepartmentFilters } from '../types'

/**
 * Mapeia parâmetros de query da URL para o contrato de filtros de relatório.
 * @param query Query atual da rota.
 * @returns Filtros normalizados para consumo no serviço.
 */
export const mapQueryToFilters = (query: LocationQuery): ItemsDepartmentFilters => {
  return {
    searchTerm: getFirstQueryValue(query.searchTerm),
    categoriaNome: getFirstQueryValue(query.categoriaNome),
    itemsType: getFirstQueryValue(query.itemsType) as 'geral' | 'patrimonial' | null,
    siglaDepartamento: getFirstQueryValue(query.siglaDepartamento),
    sortOrder: getSortOrderFromQuery(query.sortOrder),
    pageSize: getFirstQueryValue(query.pageSize),
    pageNumber: getFirstQueryValue(query.pageNumber),
  }
}
