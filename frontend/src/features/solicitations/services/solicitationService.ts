import { apiClient } from '@/services/apiClient'
import type {
  CreateSolicitationPayload,
  MySolicitationFilters,
  Solicitation,
  SolicitationFilters,
  SolicitationHistoryEvent,
  SolicitationStatusPayload,
} from '../types'
import type { PaginatedResponse } from '@/types'
import axios from 'axios'

interface ISolicitationService {
  create(payload: CreateSolicitationPayload): Promise<Solicitation>
  update(id: number, payload: Partial<Solicitation>): Promise<Solicitation>
  getById(id: number): Promise<Solicitation>
  getMySolicitations(filters?: MySolicitationFilters): Promise<PaginatedResponse<Solicitation>>
  getAllSolicitations(filters?: SolicitationFilters): Promise<PaginatedResponse<Solicitation>>
  getSolicitationHistory(solicitationId: number): Promise<SolicitationHistoryEvent[]>
  updateStatus(solicitationId: number, payload: SolicitationStatusPayload): Promise<Solicitation>
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
      const response = await apiClient.post<Solicitation>(endpoint, apiPayload)
      return response.data
    } catch (error) {
      console.error(`Erro ao criar solicitação '${type}':`, error)
      throw new Error('Não foi possível criar a solicitação.')
    }
  },

  /**
   * Atualiza uma solicitação no backend.
   * @param id ID da solicitação.
   * @param payload Os novos dados da solicitação.
   * @returns A solicitação com os novos dados.
   */
  async update(id, payload) {
    try {
      const response = await apiClient.patch<Solicitation>(`/solicitacao/${id}`, payload)
      return response.data
    } catch (error) {
      console.error(`Erro ao atualizar solicitação:`, error)
      throw new Error('Não foi possível atualizar a solicitação.')
    }
  },

  /**
   * Busca uma solicitação através do id.
   * @param id O ID da solicitação.
   * @returns A solicitação criada.
   */
  async getById(id) {
    try {
      const response = await apiClient.get<Solicitation>(`/solicitacao/${id}`)
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
    console.log({filters})
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (key === 'dateRange' && Array.isArray(value) && value.length > 0) {
          if (value[0]) {
            params.append('dataInicial', value[0].toISOString().split('T')[0])
          }

          if (value[1]) {
            params.append('dataFinal', value[1].toISOString().split('T')[0])
          }
        } else if (key === 'statusIds' && Array.isArray(value) && value.length > 0) {
          value.forEach((id) => params.append('statusIds', String(id)))
        } else {
          if (value != null && value !== '' && value.length !== 0) {
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

  /**
   * Busca todas as solicitações feitas.
   * @returns As solicitações.
   */
  async getAllSolicitations(filters?) {
    const params = new URLSearchParams()

    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (key === 'dateRange' && Array.isArray(value) && value.length > 0) {
          if (value[0]) {
            params.append('dataInicial', value[0].toISOString().split('T')[0])
          }

          if (value[1]) {
            params.append('dataFinal', value[1].toISOString().split('T')[0])
          }
        } else if (key === 'statusIds' && Array.isArray(value) && value.length > 0) {
          value.forEach((id) => params.append('statusIds', String(id)))
        } else {
          if (value != null && value !== '') {
            params.set(key, String(value))
          }
        }
      })
    }

    try {
      const response = await apiClient.get<PaginatedResponse<Solicitation>>(`/solicitacao`, {
        params,
      })
      return response.data
    } catch (error) {
      console.error(`Erro ao buscar solicitações:`, error)
      throw new Error('Não foi possível buscar solicitações.')
    }
  },

  /**
   * Busca o histórico da solicitação.
   * @param solicitationId O ID da solicitação.
   * @returns Um array de histórico.
   */
  async getSolicitationHistory(solicitationId) {
    try {
      const response = await apiClient.get<SolicitationHistoryEvent[]>(
        `/solicitacao/${solicitationId}/historico`,
      )
      return response.data
    } catch (error) {
      console.error(`Erro ao buscar histórico da solicitação:`, error)
      throw new Error('Não foi possível buscar histórico da solicitação.')
    }
  },

  /**
   * Atualiza o status de uma solicitação no backend.
   * @param solicitationId ID do novo status.
   * @param body O ID do status e a observação/justificativa.
   * @returns A solicitação atualizada.
   */
  async updateStatus(solicitationId, payload) {
    try {
      const response = await apiClient.patch<Solicitation>(
        `/solicitacao/${solicitationId}/status`,
        payload,
      )
      return response.data
    } catch (error) {
      console.error(`Erro ao atualizar status da solicitação:`, error)
      if (axios.isAxiosError(error) && error.response) {
        throw new Error(error.response.data.message || 'Não foi possível atualizar o status.')
      }
      throw new Error('Ocorreu um erro de comunicação ao tentar atualizar o status.')
    }
  },
}
