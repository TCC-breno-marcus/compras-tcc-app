import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { Categoria } from '../../types'

const { apiClientMock } = vi.hoisted(() => ({
  apiClientMock: {
    get: vi.fn(),
  },
}))

vi.mock('@/services/apiClient', () => ({
  apiClient: apiClientMock,
}))

import { categoriaService } from '../categoriaService'

describe('Service: categoriaService', () => {
  const category: Categoria = {
    id: 1,
    nome: 'CATEGORIA TESTE',
    descricao: 'desc',
    isActive: true,
  }

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('deve serializar filtros com múltiplos nomes em getAll', async () => {
    apiClientMock.get.mockResolvedValue({ data: [category] })

    const result = await categoriaService.getAll({ nome: ['A', 'B'] })

    const [, config] = apiClientMock.get.mock.calls[0]
    const params = config.params as URLSearchParams
    expect(params.getAll('nome')).toEqual(['A', 'B'])
    expect(result[0].nome).toBe('Categoria Teste')
  })

  it('deve buscar categoria por id com nome transformado', async () => {
    apiClientMock.get.mockResolvedValue({ data: category })

    const result = await categoriaService.getById(99)

    expect(apiClientMock.get).toHaveBeenCalledWith('/categoria/99')
    expect(result.nome).toBe('Categoria Teste')
  })
})

