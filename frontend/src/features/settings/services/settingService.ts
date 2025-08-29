import { apiClient } from '@/services/apiClient'
import type { PrazoSubmissao } from '../types'

interface ISettingService {
  getPrazoSubmissao(): Promise<PrazoSubmissao>
  editarPrazoSubmissao(newDate: string): void
}

export const settingService: ISettingService = {
  /**
   * Busca o prazo de submissão (criação e edição) de solicitações.
   * @returns O prazo.
   */
  async getPrazoSubmissao() {
    try {
      const response = await apiClient.get<PrazoSubmissao>('/configuracao/prazo-submissao')
      return response.data
    } catch (error) {
      console.error(`Erro ao buscar o prazo de submissão:`, error)
      throw new Error('Não foi possível buscar o prazo de submissão.')
    }
  },

  /**
   * Altera o prazo de submissão das solicitações.
   * @param newDate Nova data no formato "2025-08-15T23:57:32.074Z".
   */
  async editarPrazoSubmissao(newDate) {
    try {
      await apiClient.put('/configuracao/prazo-submissao', {
        prazoSubmissao: newDate,
      })
    } catch (error) {
      console.error(`Erro ao alterar o prazo de submissão:`, error)
      throw new Error('Não foi possível alterar o prazo de submissão.')
    }
  },
}
