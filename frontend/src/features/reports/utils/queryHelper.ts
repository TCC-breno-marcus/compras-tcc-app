import type { LocationQuery } from 'vue-router'
import { getFirstQueryValue, getSortOrderFromQuery } from '@/utils/queryHelper'
import type { ItemsDepartmentFilters } from '../types'

export const mapQueryToFilters = (query: LocationQuery): ItemsDepartmentFilters => {
  return {
    searchTerm: getFirstQueryValue(query.searchTerm),
    categoriaNome: getFirstQueryValue(query.categoriaNome),
    departamento: getFirstQueryValue(query.departamento),
    sortOrder: getSortOrderFromQuery(query.sortOrder),
    pageSize: getFirstQueryValue(query.pageSize),
    pageNumber: getFirstQueryValue(query.pageNumber),
  }
}
