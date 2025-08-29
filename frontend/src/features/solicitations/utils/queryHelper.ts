import type { LocationQuery } from 'vue-router'
import type { MySolicitationFilters } from '../types'
import { getFirstQueryValue, getSortOrderFromQuery } from '@/utils/queryHelper'

export const mapQueryToFilters = (query: LocationQuery): MySolicitationFilters => {
  const tipoFromQuery = getFirstQueryValue(query.tipo)

  const dataInicialString = getFirstQueryValue(query.dataInicial)
  const dataFinalString = getFirstQueryValue(query.dataFinal)

  let dateRangeValue: (Date | null)[] | null = null

  if (dataInicialString) {
    const dataInicial = new Date(`${dataInicialString}T00:00:00`)
    const dataFinal = dataFinalString ? new Date(`${dataFinalString}T00:00:00`) : null
    dateRangeValue = [dataInicial, dataFinal]
  }

  return {
    externalId: getFirstQueryValue(query.externalId),
    tipo: tipoFromQuery === 'Geral' || tipoFromQuery === 'Patrimonial' ? tipoFromQuery : '',
    dateRange: dateRangeValue,
    sortOrder: getSortOrderFromQuery(query.sortOrder),
    pageSize: getFirstQueryValue(query.pageSize),
    pageNumber: getFirstQueryValue(query.pageNumber),
  }
}
