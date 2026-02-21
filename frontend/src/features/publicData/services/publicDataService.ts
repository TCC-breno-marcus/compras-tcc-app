import { apiClient } from '@/services/apiClient'
import type { PublicSolicitationFilters, PublicSolicitationQueryResult } from '@/features/publicData/types'

type PublicExportFormat = 'csv' | 'json' | 'pdf'

interface IPublicDataService {
  getSolicitations(filters?: PublicSolicitationFilters): Promise<PublicSolicitationQueryResult>
  exportSolicitations(filters: PublicSolicitationFilters, format: PublicExportFormat): Promise<Blob>
}

const buildParams = (filters?: PublicSolicitationFilters, format?: PublicExportFormat) => {
  const params = new URLSearchParams()

  if (!filters) {
    if (format) {
      params.set('formatoArquivo', format)
    }
    return params
  }

  Object.entries(filters).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      params.set(key, String(value))
    }
  })

  if (format) {
    params.set('formatoArquivo', format)
  }

  return params
}

export const publicDataService: IPublicDataService = {
  /**
   * Consulta solicitações públicas com filtros opcionais e retorno paginado.
   * @param filters Conjunto de filtros aceitos pela API pública.
   * @returns Resultado paginado da consulta pública.
   */
  async getSolicitations(filters) {
    try {
      const response = await apiClient.get<PublicSolicitationQueryResult>(
        '/dados-publicos/solicitacoes',
        {
          params: buildParams(filters),
        },
      )

      return response.data
    } catch (error) {
      console.error('Erro ao consultar solicitações públicas:', error)
      throw new Error('Não foi possível carregar os dados públicos no momento.')
    }
  },

  /**
   * Exporta dados públicos conforme o formato solicitado.
   * Para CSV/PDF utiliza o arquivo retornado pela API.
   * Para JSON serializa o payload de consulta em um Blob baixável.
   * @param filters Filtros aplicados na exportação.
   * @param format Formato de saída (`csv`, `pdf` ou `json`).
   * @returns Blob pronto para download.
   */
  async exportSolicitations(filters, format) {
    try {
      if (format === 'csv' || format === 'pdf') {
        const response = await apiClient.get<Blob>('/dados-publicos/solicitacoes', {
          params: buildParams(filters, format),
          responseType: 'blob',
        })

        return response.data
      }

      const response = await apiClient.get<PublicSolicitationQueryResult>(
        '/dados-publicos/solicitacoes',
        {
          params: buildParams(filters, 'json'),
        },
      )

      return new Blob([JSON.stringify(response.data, null, 2)], {
        type: 'application/json;charset=utf-8',
      })
    } catch (error) {
      console.error(`Erro ao exportar solicitações públicas em ${format.toUpperCase()}:`, error)
      throw new Error(`Não foi possível exportar os dados públicos em ${format.toUpperCase()}.`)
    }
  },
}
