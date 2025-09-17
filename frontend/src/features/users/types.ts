import type { UnitOrganizational } from '../unitOrganizational/types'

/**
 * Representa os dados de um usuário
 */
export interface User {
  id: number
  nome: string
  email: string
  telefone: string
  cpf: string
  role: 'Admin' | 'Gestor' | 'Solicitante'
  isActive: boolean
  unidade: UnitOrganizational | null
}

/**
 * Representa os filtros de parâmetro do serviço get all users
 */
export interface GetUsersFilters {
  role?: 'Admin' | 'Gestor' | 'Solicitante'
  sortOrder?: string | null
  isActive?: boolean
  pageSize?: string
  pageNumber?: string
}

/**
 * Representa o body de requisição para modificar o perfil de um usuário
 */
export interface UpdateUserRoleRequest {
  email: string
  role: 'Admin' | 'Gestor' | 'Solicitante'
}

export interface UpdateUserRoleResponse {
  message: string
}

export interface UpdateUserStatusResponse {
  message: string
}
