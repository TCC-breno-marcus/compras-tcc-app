import type { LocationQuery } from 'vue-router'
import type { MySolicitationFilters } from '..'
import { getFirstQueryValue, getSortOrderFromQuery } from '@/utils/queryHelper'

export const mapQueryToFilters = (query: LocationQuery): MySolicitationFilters => {
  const tipoFromQuery = getFirstQueryValue(query.tipo)

  const dataInicialString = getFirstQueryValue(query.dataInicial)
  const dataFinalString = getFirstQueryValue(query.dataFinal)

  return {
    externalId: getFirstQueryValue(query.externalId),
    tipo: tipoFromQuery === 'Geral' || tipoFromQuery === 'Patrimonial' ? tipoFromQuery : '',
    dataInicial: dataInicialString ? new Date(`${dataInicialString}T00:00:00`) : null,
    dataFinal: dataFinalString ? new Date(`${dataFinalString}T00:00:00`) : null,
    sortOrder: getSortOrderFromQuery(query.sortOrder),
  }
}
