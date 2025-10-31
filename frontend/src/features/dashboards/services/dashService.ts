import { apiClient } from '@/services/apiClient'
import type { PaginatedResponse } from '@/types'
import type { DashboardResponse } from '../types'

interface IDashService {
  getDashboards(): Promise<DashboardResponse>
}

export const dashService: IDashService = {
  /**
   * Busca os KPIs e dashboards do gestor.
   * @returns Dados de KPIs e dashboards.
   */
  async getDashboards() {
    try {
      const response = await apiClient.get<DashboardResponse>(`/dashboard`)
      return response.data
    } catch (error) {
      console.error(`Erro ao buscar dashboards:`, error)
      throw new Error('Não foi possível buscar dashboards.')
    }
  },
}
