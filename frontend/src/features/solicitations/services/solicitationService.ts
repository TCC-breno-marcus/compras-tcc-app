import { apiClient } from '@/services/apiClient'
import type { CreateSolicitationPayload, SolicitationResult } from '..'

interface ISolicitationService {
  create(payload: CreateSolicitationPayload): Promise<SolicitationResult>
  getById(id: number): Promise<SolicitationResult>
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
      throw new Error('Não foi possível enviar a solicitação.')
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
      console.error(`Erro ao buscar solicitação:`, error)
      throw new Error('Não foi possível enviar a solicitação.')
    }
  },
}
