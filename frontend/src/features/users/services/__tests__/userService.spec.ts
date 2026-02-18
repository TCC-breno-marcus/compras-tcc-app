import { beforeEach, describe, expect, it, vi } from 'vitest'

const { apiClientMock } = vi.hoisted(() => ({
  apiClientMock: {
    get: vi.fn(),
    patch: vi.fn(),
  },
}))

vi.mock('@/services/apiClient', () => ({
  apiClient: apiClientMock,
}))

import { userService } from '../userService'

describe('Service: userService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('deve serializar filtros no getAllUsers', async () => {
    apiClientMock.get.mockResolvedValue({ data: { data: [], totalCount: 0, pageNumber: 1, pageSize: 10 } })

    await userService.getAllUsers({
      isActive: true,
      sortOrder: 'asc',
      pageNumber: '1',
      pageSize: '10',
    })

    const [, config] = apiClientMock.get.mock.calls[0]
    const params = config.params as URLSearchParams
    expect(params.get('isActive')).toBe('true')
    expect(params.get('sortOrder')).toBe('asc')
  })

  it('deve ativar usuário', async () => {
    apiClientMock.patch.mockResolvedValue({ data: { message: 'Ativado' } })
    const result = await userService.activeUser(10)

    expect(apiClientMock.patch).toHaveBeenCalledWith('/usuario/10/ativar')
    expect(result).toEqual({ message: 'Ativado' })
  })

  it('deve inativar usuário', async () => {
    apiClientMock.patch.mockResolvedValue({ data: { message: 'Inativado' } })
    const result = await userService.inactiveUser(20)

    expect(apiClientMock.patch).toHaveBeenCalledWith('/usuario/20/inativar')
    expect(result).toEqual({ message: 'Inativado' })
  })

  it('deve lançar erro padronizado no getAllUsers', async () => {
    apiClientMock.get.mockRejectedValue(new Error('erro'))

    await expect(userService.getAllUsers()).rejects.toThrow('Não foi possível buscar usuários.')
  })
})

