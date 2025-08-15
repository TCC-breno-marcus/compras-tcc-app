import { apiClient } from '@/services/apiClient'
import type { CreateSolicitationPayload, SolicitationResult } from '..'

export const solicitationService = {
  /**
   * Cria uma nova solicitação no backend.
   * @param payload Os dados da nova solicitação.
   * @returns A solicitação criada.
   */
  async create(payload: CreateSolicitationPayload): Promise<SolicitationResult> {
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
}
