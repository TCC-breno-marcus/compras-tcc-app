import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { userService } from '../services/userService'
import type { GetUsersFilters, User } from '../types'

/**
 * Store para gerenciar estados com dados de usuários
 */
export const useUserStore = defineStore('user', () => {
  const users = ref<User[]>([])
  const totalCount = ref<number>(0)
  const pageNumber = ref<number>(1)
  const pageSize = ref<number>(50)
  const totalPages = ref<number>(1)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  /**
   * Busca os usuários e atualiza o estado.
   * @param filters Os parâmetros de filtro e paginação.
   */
  const fetchUsers = async (filters?: GetUsersFilters) => {
    isLoading.value = true
    error.value = null

    try {
      const response = await userService.getAllUsers(filters)

      users.value = response.data
      totalCount.value = response.totalCount
      pageNumber.value = response.pageNumber
      pageSize.value = response.pageSize
      totalPages.value = Math.ceil(response.totalCount / response.pageSize) || 1
    } catch (err) {
      error.value = 'Ocorreu um erro ao buscar os usuários.'
      users.value = []
      console.error(err)
    } finally {
      isLoading.value = false
    }
  }

  const solicitantes = computed(() => users.value.filter((user) => user.role === 'Solicitante'))
  const gestores = computed(() => users.value.filter((user) => user.role === 'Gestor'))
  const admins = computed(() => users.value.filter((user) => user.role === 'Admin'))

  return {
    users,
    isLoading,
    error,
    fetchUsers,
    solicitantes,
    gestores,
    admins,
  }
})
