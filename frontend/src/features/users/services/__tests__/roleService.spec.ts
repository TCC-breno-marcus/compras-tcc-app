import { beforeEach, describe, expect, it, vi } from 'vitest'

const { apiClientMock } = vi.hoisted(() => ({
  apiClientMock: {
    post: vi.fn(),
  },
}))

vi.mock('@/services/apiClient', () => ({
  apiClient: apiClientMock,
}))

import { roleService } from '../roleService'

describe('Service: roleService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('deve atualizar perfil do usuário', async () => {
    apiClientMock.post.mockResolvedValue({ data: { message: 'Role atualizada.' } })

    const result = await roleService.updateUserRole({ email: 'a@b.com', role: 'Gestor' })

    expect(apiClientMock.post).toHaveBeenCalledWith('/roles/atribuir-role', {
      email: 'a@b.com',
      role: 'Gestor',
    })
    expect(result).toEqual({ message: 'Role atualizada.' })
  })

  it('deve lançar erro padronizado quando falhar', async () => {
    apiClientMock.post.mockRejectedValue(new Error('erro'))

    await expect(roleService.updateUserRole({ email: 'a@b.com', role: 'Gestor' })).rejects.toThrow(
      'Não foi possível alterar o perfil do usuário.',
    )
  })
})

