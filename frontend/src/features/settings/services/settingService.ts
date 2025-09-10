import { apiClient } from '@/services/apiClient'
import type { Setting } from '../types'

interface ISettingService {
  getSettings(): Promise<Setting>
  updateSettings(settings: Partial<Setting>): Promise<Setting>
}

export const settingService: ISettingService = {
  /**
   * Busca todas as configurações no backend.
   * @returns .
   */
  async getSettings() {
    try {
      const response = await apiClient.get<Setting>('/configuracao')
      return response.data
    } catch (error) {
      console.error(`Erro ao buscar o prazo de submissão:`, error)
      throw new Error('Não foi possível buscar o prazo de submissão.')
    }
  },

  /**
   * Altera somente as configurações passadas no param
   * @param settings Configurações a serem modificadas
   */
  async updateSettings(settings) {
    try {
      
      const response = await apiClient.patch<Setting>('/configuracao', settings)
      return response.data
    } catch (error) {
      console.error(`Erro ao alterar as configurações:`, error)
      throw new Error('Não foi possível alterar as configurações.')
    }
  },
}
