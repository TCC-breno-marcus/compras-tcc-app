import { apiClient } from '@/services/apiClient'
import type { PublicSolicitationFilters, PublicSolicitationQueryResult } from '@/features/publicData/types'

interface IPublicDataService {
  getSolicitations(filters?: PublicSolicitationFilters): Promise<PublicSolicitationQueryResult>
  exportSolicitationsCsv(filters?: PublicSolicitationFilters): Promise<Blob>
}

const buildParams = (filters?: PublicSolicitationFilters, forceCsv = false) => {
  const params = new URLSearchParams()

  if (!filters) {
    if (forceCsv) {
      params.set('formatoArquivo', 'csv')
    }
    return params
  }

  Object.entries(filters).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      params.set(key, String(value))
    }
  })

  if (forceCsv) {
    params.set('formatoArquivo', 'csv')
  }

  return params
}

export const publicDataService: IPublicDataService = {
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

  async exportSolicitationsCsv(filters) {
    try {
      const response = await apiClient.get<Blob>('/dados-publicos/solicitacoes', {
        params: buildParams(filters, true),
        responseType: 'blob',
      })

      return response.data
    } catch (error) {
      console.error('Erro ao exportar solicitações públicas em CSV:', error)
      throw new Error('Não foi possível exportar os dados públicos em CSV.')
    }
  },
}
