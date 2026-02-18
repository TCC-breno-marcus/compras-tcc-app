import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { getItensMock } = vi.hoisted(() => ({
  getItensMock: vi.fn(),
}))

vi.mock('../../services/catalogoService', () => ({
  catalogoService: {
    getItens: getItensMock,
  },
}))

import { useCatalogoStore } from '../catalogoStore'

describe('Store: catalogoStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('deve preencher estado e paginação ao buscar itens', async () => {
    const store = useCatalogoStore()
    getItensMock.mockResolvedValue({
      data: [{ id: 1, nome: 'Item 1' }],
      totalCount: 12,
      pageNumber: 1,
      pageSize: 10,
    })

    await store.fetchItems()

    expect(store.items).toHaveLength(1)
    expect(store.totalCount).toBe(12)
    expect(store.totalPages).toBe(2)
    expect(store.loading).toBe(false)
  })

  it('deve limpar itens e definir erro quando falhar', async () => {
    const store = useCatalogoStore()
    store.items = [{ id: 1 } as any]
    getItensMock.mockRejectedValue(new Error('erro'))

    await store.fetchItems()

    expect(store.items).toEqual([])
    expect(store.error).toBe('Ocorreu um erro ao buscar os itens.')
    expect(store.loading).toBe(false)
  })

  it('deve calcular hasNextPage corretamente', async () => {
    const store = useCatalogoStore()
    getItensMock.mockResolvedValue({
      data: [],
      totalCount: 30,
      pageNumber: 1,
      pageSize: 10,
    })

    await store.fetchItems()

    expect(store.hasNextPage).toBe(true)
  })
})

