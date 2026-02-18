import type { LocationQuery } from 'vue-router'
import type { MySolicitationFilters, SolicitationFilters } from '../types'
import { getFirstQueryValue, getSortOrderFromQuery } from '@/utils/queryHelper'

/**
 * Converte query da URL para filtros de listagem de solicitações.
 * @param query Query atual da rota.
 * @param filterParamsType Define se inclui filtros exclusivos de visão gerencial.
 * @returns Objeto de filtros pronto para o serviço de solicitações.
 */
export const mapQueryToFilters = (
  query: LocationQuery,
  filterParamsType: 'MySolicitationFilters' | 'SolicitationFilters',
): MySolicitationFilters | SolicitationFilters => {
  const tipoFromQuery = getFirstQueryValue(query.tipo)

  const dataInicialString = getFirstQueryValue(query.dataInicial)
  const dataFinalString = getFirstQueryValue(query.dataFinal)

  let dateRangeValue: (Date | null)[] | null = null

  if (dataInicialString) {
    const dataInicial = new Date(`${dataInicialString}T00:00:00`)
    const dataFinal = dataFinalString ? new Date(`${dataFinalString}T00:00:00`) : null
    dateRangeValue = [dataInicial, dataFinal]
  }

  const pessoaIdString = getFirstQueryValue(query.pessoaId)
  const pessoaId = pessoaIdString ? Number(pessoaIdString) : null

  const statusIds = getQueryAsArrayOfNumbers(query.statusIds)

  return {
    externalId: getFirstQueryValue(query.externalId),
    tipo: tipoFromQuery === 'Geral' || tipoFromQuery === 'Patrimonial' ? tipoFromQuery : '',
    dateRange: dateRangeValue,
    sortOrder: getSortOrderFromQuery(query.sortOrder),
    pageSize: getFirstQueryValue(query.pageSize),
    pageNumber: getFirstQueryValue(query.pageNumber),
    statusIds,
    ...(filterParamsType === 'SolicitationFilters' && {
      pessoaId,
      siglaDepartamento: getFirstQueryValue(query.siglaDepartamento),
    }),
  }
}

/**
 * Normaliza valores de query para lista numérica.
 * @param value Valor único ou array da query string.
 * @returns Array de números válidos.
 */
const getQueryAsArrayOfNumbers = (value: unknown): number[] => {
  if (!value) return []
  const arr = Array.isArray(value) ? value : [value]
  return arr.map(Number).filter((n) => !isNaN(n))
}
