import { apiClient } from '@/services/apiClient'
import type { PaginatedResponse } from '@/types'
import type { ItemDepartmentResponse, ItemsDepartmentFilters } from '../types'

interface IReportService {
  getItemsPerDepartment(
    filters?: ItemsDepartmentFilters,
  ): Promise<PaginatedResponse<ItemDepartmentResponse>>
}

export const reportService: IReportService = {
  /**
   * Busca todos os itens solicitados agrupados por departamento na API.
   * @returns Uma lista de itens do tipo ItemDepartmentResponse.
   */
  async getItemsPerDepartment(filters?) {
    const params = new URLSearchParams()

    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value != null && value !== '') {
          params.set(key, String(value))
        }
      })
    }

    try {
      const response = await apiClient.get<PaginatedResponse<ItemDepartmentResponse>>(
        `/relatorio/itens-departamento`,
        {
          params,
        },
      )
      return response.data
    } catch (error) {
      console.error(`Erro ao buscar itens solicitados e agrupados por departamento:`, error)
      throw new Error('Não foi possível buscar itens solicitados e agrupados por departamento.')
    }
  },
}
