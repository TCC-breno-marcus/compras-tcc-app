import { apiClient } from '@/services/apiClient'
import type { UpdateUserRoleRequest, UpdateUserRoleResponse, User } from '../types'

interface IRoleService {
  updateUserRole(body: UpdateUserRoleRequest): Promise<UpdateUserRoleResponse>
}

export const roleService: IRoleService = {
  /**
   * Atualiza o perfil do usuário.
   * @returns mensagem
   */
  async updateUserRole(body) {
    try {
      const response = await apiClient.post<UpdateUserRoleResponse>(`/roles/atribuir-role`, body)
      return response.data
    } catch (error) {
      console.error(`Erro ao alterar perfil do usuário:`, error)
      throw new Error('Não foi possível alterar o perfil do usuário.')
    }
  },
}
