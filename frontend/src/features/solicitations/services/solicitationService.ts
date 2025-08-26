import { apiClient } from '@/services/apiClient'
import type {
  CreateSolicitationPayload,
  MySolicitationFilters,
  Solicitation,
  SolicitationResult,
} from '..'
import type { PaginatedResponse } from '@/types'

interface ISolicitationService {
  create(payload: CreateSolicitationPayload): Promise<SolicitationResult>
  getById(id: number): Promise<Solicitation>
  getMySolicitations(filters?: MySolicitationFilters): Promise<PaginatedResponse<Solicitation>>
}

export const solicitationService: ISolicitationService = {
  /**
   * Cria uma nova solicitação no backend.
   * @param payload Os dados da nova solicitação.
   * @returns A solicitação criada.
   */
  async create(payload) {
    const { type, ...apiPayload } = payload
    const endpoint = type === 'geral' ? '/solicitacao/geral' : '/solicitacao/patrimonial'
    try {
      const response = await apiClient.post<SolicitationResult>(endpoint, apiPayload)
      return response.data
    } catch (error) {
      console.error(`Erro ao criar solicitação '${type}':`, error)
      throw new Error('Não foi possível criar a solicitação.')
    }
  },

  /**
   * Busca uma solicitação através do id.
   * @param id O ID da solicitação.
   * @returns A solicitação criada.
   */
  async getById(id) {
    try {
      const response = await apiClient.get<SolicitationResult>(`/solicitacao/${id}`)
      return response.data
    } catch (error) {
      console.error(`Erro ao buscar a solicitação:`, error)
      throw new Error('Não foi possível buscar a solicitação.')
    }
  },

  /**
   * Busca todas as solicitações do solicitante logado.
   * @returns As solicitações.
   */
  async getMySolicitations(filters?) {
    const params = new URLSearchParams()

    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if ((key === 'dataInicial' || key === 'dataFinal') && value instanceof Date) {
          params.append(key, value.toISOString().split('T')[0])
        } else {
          if (value != null && value !== '') {
            params.set(key, String(value))
          }
        }
      })
    }

    try {
      const response = await apiClient.get<PaginatedResponse<Solicitation>>(
        `/solicitacao/minhas-solicitacoes`,
        { params },
      )
      return response.data
    } catch (error) {
      console.error(`Erro ao buscar suas solicitações:`, error)
      throw new Error('Não foi possível buscar suas solicitações.')
    }
  },
}
