import { beforeEach, describe, expect, it, vi } from 'vitest'

const { apiClientMock, isAxiosErrorMock } = vi.hoisted(() => ({
  apiClientMock: {
    get: vi.fn(),
  },
  isAxiosErrorMock: vi.fn(),
}))

vi.mock('@/services/apiClient', () => ({
  apiClient: apiClientMock,
}))

vi.mock('axios', () => ({
  default: {
    isAxiosError: isAxiosErrorMock,
  },
}))

import { unitOrganizationalService } from '../unitOrganizationalService'

describe('Service: unitOrganizationalService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('deve buscar centros', async () => {
    apiClientMock.get.mockResolvedValue({ data: [{ id: 1, nome: 'Centro' }] })

    const result = await unitOrganizationalService.getCenters()

    expect(apiClientMock.get).toHaveBeenCalledWith('/centro')
    expect(result).toEqual([{ id: 1, nome: 'Centro' }])
  })

  it('deve buscar departamentos', async () => {
    apiClientMock.get.mockResolvedValue({ data: [{ id: 2, nome: 'Dep' }] })

    const result = await unitOrganizationalService.getDepartments()

    expect(apiClientMock.get).toHaveBeenCalledWith('/departamento')
    expect(result).toEqual([{ id: 2, nome: 'Dep' }])
  })

  it('deve usar mensagem da API em erro axios', async () => {
    isAxiosErrorMock.mockReturnValue(true)
    apiClientMock.get.mockRejectedValue({ response: { data: { message: 'Falhou API' } } })

    await expect(unitOrganizationalService.getCenters()).rejects.toThrow('Falhou API')
  })

  it('deve lançar erro de conexão padrão', async () => {
    isAxiosErrorMock.mockReturnValue(false)
    apiClientMock.get.mockRejectedValue(new Error('boom'))

    await expect(unitOrganizationalService.getDepartments()).rejects.toThrow(
      'Erro de conexão com a API ao tentar buscar departamentos.',
    )
  })
})

