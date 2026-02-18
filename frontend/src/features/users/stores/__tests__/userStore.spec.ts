import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { getAllUsersMock } = vi.hoisted(() => ({
  getAllUsersMock: vi.fn(),
}))

vi.mock('../../services/userService', () => ({
  userService: {
    getAllUsers: getAllUsersMock,
  },
}))

import { useUserStore } from '../userStore'

describe('Store: userStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('deve carregar usuários e paginação', async () => {
    const store = useUserStore()
    getAllUsersMock.mockResolvedValue({
      data: [
        { id: 1, nome: 'A', role: 'Admin', isActive: true },
        { id: 2, nome: 'B', role: 'Solicitante', isActive: false },
      ],
      totalCount: 2,
      pageNumber: 1,
      pageSize: 50,
    })

    await store.fetchUsers()

    expect(store.users).toHaveLength(2)
    expect(store.activeUsers).toHaveLength(1)
    expect(store.inactiveUsers).toHaveLength(1)
    expect(store.admins).toHaveLength(1)
    expect(store.solicitantes).toHaveLength(1)
  })

  it('deve limpar estado quando fetch falhar', async () => {
    const store = useUserStore()
    store.users = [{ id: 1 } as any]
    getAllUsersMock.mockRejectedValue(new Error('erro'))

    await store.fetchUsers()

    expect(store.users).toEqual([])
    expect(store.error).toBe('Ocorreu um erro ao buscar os usuários.')
    expect(store.isLoading).toBe(false)
  })
})

