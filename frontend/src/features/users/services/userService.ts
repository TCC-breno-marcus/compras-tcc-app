import { apiClient } from '@/services/apiClient'
import type { PaginatedResponse } from '@/types'
import type { GetUsersFilters, UpdateUserStatusResponse, User } from '../types'

interface IUserService {
  getAllUsers(filters?: GetUsersFilters): Promise<PaginatedResponse<User>>
  activeUser(id: number): Promise<UpdateUserStatusResponse | undefined>
  inactiveUser(id: number): Promise<UpdateUserStatusResponse | undefined>
}

export const userService: IUserService = {
  /**
   * Busca todos os usuários do sistema.
   * @returns Array de usuários.
   */
  async getAllUsers(filters?) {
    const params = new URLSearchParams()

    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (key === 'role') {
          //  lógica pra transformar role
        } else {
          if (value != null && value !== '') {
            params.set(key, String(value))
          }
        }
      })
    }

    try {
      const response = await apiClient.get<PaginatedResponse<User>>(`/usuario`, { params })
      return response.data
    } catch (error) {
      console.error(`Erro ao buscar usuários:`, error)
      throw new Error('Não foi possível buscar usuários.')
    }
  },

  /**
   * Ativar um usuário.
   * @param id - ID do usuário a ser ativado.
   * @returns Promise que resolve com a resposta da ativação ou undefined se sucesso sem conteúdo.
   */
  async activeUser(id) {
    try {
      const response = await apiClient.patch<UpdateUserStatusResponse | undefined>(
        `/usuario/${id}/ativar`,
      )
      return response.data
    } catch (error) {
      console.error(`Erro ao ativar usuário:`, error)
      throw new Error('Não foi possível ativar o usuário.')
    }
  },

  /**
   * Inativar um usuário.
   * @param id - ID do usuário a ser desativado.
   * @returns Promise que resolve com a resposta da desativação ou undefined se sucesso sem conteúdo.
   */
  async inactiveUser(id) {
    try {
      const response = await apiClient.patch<UpdateUserStatusResponse | undefined>(
        `/usuario/${id}/inativar`,
      )
      return response.data
    } catch (error) {
      console.error(`Erro ao inativar usuário:`, error)
      throw new Error('Não foi possível inativar o usuário.')
    }
  },
}
