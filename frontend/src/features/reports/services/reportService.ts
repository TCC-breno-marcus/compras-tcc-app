import { apiClient } from '@/services/apiClient'
import type { PaginatedResponse } from '@/types'
import type { CategoryConsumptionResponse, CenterExpenseResponse, ItemDepartmentResponse, ItemsDepartmentFilters, ReportDateFilters } from '../types'

interface IReportService {
  getItemsPerDepartment(
    filters?: ItemsDepartmentFilters,
  ): Promise<PaginatedResponse<ItemDepartmentResponse>>
  exportItemsPerDepartment(
    itemsType: 'geral' | 'patrimonial',
    fileFormat: 'csv' | 'excel',
  ): Promise<Blob>
  getCenterExpenses(filters: ReportDateFilters): Promise<CenterExpenseResponse[]>
  getCategoryConsumption(filters: ReportDateFilters): Promise<CategoryConsumptionResponse[]>
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

  /**
   * Gerar um relatório com todos os itens solicitados agrupados por departamento na API.
   * @param itemsType O tipo de relatório (geral ou patrimonial)
   * @param fileFormat O formato do arquivo ('csv' ou 'excel')
   * @returns Um Blob contendo o arquivo no fileFormat especificado.
   */
  async exportItemsPerDepartment(itemsType, fileFormat = 'excel') {
    try {
      const response = await apiClient.get<Blob>(
        `/relatorio/itens-departamento/${itemsType}/exportar`,
        {
          params: { formatoArquivo: fileFormat },
          responseType: 'blob',
        },
      )
      return response.data
    } catch (error) {
      console.error(`Erro ao gerar relatório de itens (${fileFormat.toUpperCase()}):`, error)
      throw new Error(
        `Não foi possível gerar relatório de itens no formato ${fileFormat.toUpperCase()}.`,
      )
    }
  },

  /**
   * Busca relatório de gastos por centro.
   */
  async getCenterExpenses(filters) {
    const params = new URLSearchParams()
    params.append('DataInicio', filters.DataInicio)
    params.append('DataFim', filters.DataFim)

    try {
      const response = await apiClient.get<CenterExpenseResponse[]>(
        '/centro/relatorios/gastos-por-centro',
        { params }
      )
      return response.data
    } catch (error) {
      console.error('Erro ao buscar gastos por centro:', error)
      throw new Error('Não foi possível carregar os gastos por centro.')
    }
  },

  /**
   * Busca relatório de consumo por categoria.
   */
  async getCategoryConsumption(filters) {
    const params = new URLSearchParams()
    params.append('DataInicio', filters.DataInicio)
    params.append('DataFim', filters.DataFim)

    try {
      const response = await apiClient.get<CategoryConsumptionResponse[]>(
        '/centro/relatorios/consumo-por-categoria',
        { params }
      )
      return response.data
    } catch (error) {
      console.error('Erro ao buscar consumo por categoria:', error)
      throw new Error('Não foi possível carregar o consumo por categoria.')
    }
  }
}
