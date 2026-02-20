import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { getAllMock } = vi.hoisted(() => ({
  getAllMock: vi.fn(),
}))

vi.mock('../../services/categoriaService', () => ({
  categoriaService: {
    getAll: getAllMock,
  },
}))

import { useCategoriaStore } from '../categoriaStore'

describe('Store: categoriaStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('deve buscar e ordenar categorias por nome', async () => {
    const store = useCategoriaStore()
    getAllMock.mockResolvedValue([
      { id: 2, nome: 'Zeta', descricao: '', isActive: true },
      { id: 1, nome: 'Alpha', descricao: '', isActive: true },
    ])

    await store.fetch()

    expect(store.categorias[0].nome).toBe('Alpha')
    expect(store.categorias[1].nome).toBe('Zeta')
  })

  it('deve evitar chamada quando já possui categorias e sem filtros', async () => {
    const store = useCategoriaStore()
    getAllMock.mockResolvedValue([{ id: 1, nome: 'A', descricao: '', isActive: true }])
    await store.fetch()
    expect(getAllMock).toHaveBeenCalledTimes(1)

    await store.fetch()
    expect(getAllMock).toHaveBeenCalledTimes(1)
  })

  it('deve limpar categorias e definir erro ao falhar', async () => {
    const store = useCategoriaStore()
    getAllMock.mockRejectedValue(new Error('erro'))

    await store.fetch()

    expect(store.categorias).toEqual([])
    expect(store.error).toBe('Ocorreu um erro ao buscar as categorias.')
  })
})

