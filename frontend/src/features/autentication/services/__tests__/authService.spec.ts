import { beforeEach, describe, expect, it, vi } from 'vitest'

const { apiClientMock, isAxiosErrorMock } = vi.hoisted(() => ({
  apiClientMock: {
    post: vi.fn(),
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

import { authService } from '../authService'

describe('Service: authService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('deve realizar login com sucesso', async () => {
    apiClientMock.post.mockResolvedValue({ data: { token: 't', message: 'ok' } })

    const result = await authService.login({ email: 'a@b.com', password: '123456' })

    expect(apiClientMock.post).toHaveBeenCalledWith('/auth/login', {
      email: 'a@b.com',
      password: '123456',
    })
    expect(result.token).toBe('t')
  })

  it('deve retornar mensagem da API em erro de login', async () => {
    isAxiosErrorMock.mockReturnValue(true)
    apiClientMock.post.mockRejectedValue({ response: { data: { message: 'Credenciais inválidas.' } } })

    await expect(authService.login({ email: 'x', password: 'y' })).rejects.toThrow(
      'Credenciais inválidas.',
    )
  })

  it('deve lançar erro de conexão em login quando não for axios error', async () => {
    isAxiosErrorMock.mockReturnValue(false)
    apiClientMock.post.mockRejectedValue(new Error('boom'))

    await expect(authService.login({ email: 'x', password: 'y' })).rejects.toThrow(
      'Erro de conexão com a API ao tentar logar no sistema.',
    )
  })

  it('deve registrar usuário com sucesso', async () => {
    apiClientMock.post.mockResolvedValue({ data: {} })

    await authService.register({
      nome: 'Novo',
      email: 'novo@sistema.com',
      telefone: '9999999',
      cpf: '12345678901',
      password: '123456',
      role: 'Solicitante',
    })

    expect(apiClientMock.post).toHaveBeenCalledWith('/auth/register', expect.any(Object))
  })

  it('deve buscar dados do usuário autenticado', async () => {
    apiClientMock.get.mockResolvedValue({ data: { id: 1, nome: 'User' } })

    const result = await authService.getMyData()

    expect(apiClientMock.get).toHaveBeenCalledWith('/auth/me')
    expect(result).toEqual({ id: 1, nome: 'User' })
  })
})

