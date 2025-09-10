import { apiClient } from '@/services/apiClient'
import type { PaginatedResponse } from '@/types'
import type { GetUsersFilters, User } from '../types'

interface IUserService {
  getAllUsers(filters?: GetUsersFilters): Promise<PaginatedResponse<User>>
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
}
