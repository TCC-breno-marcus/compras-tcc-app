import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { publicDataServiceMock } = vi.hoisted(() => ({
  publicDataServiceMock: {
    getSolicitations: vi.fn(),
    exportSolicitations: vi.fn(),
  },
}))

vi.mock('../../services/publicDataService', () => ({
  publicDataService: publicDataServiceMock,
}))

import { usePublicDataStore } from '../publicDataStore'

describe('Store: publicDataStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('deve buscar solicitações públicas e preencher métricas', async () => {
    const store = usePublicDataStore()

    publicDataServiceMock.getSolicitations.mockResolvedValue({
      data: [{ id: 1 }],
      totalCount: 11,
      pageNumber: 2,
      pageSize: 10,
      totalPages: 2,
      totalItensSolicitados: 30,
      valorTotalSolicitado: 580,
    })

    await store.fetchPublicSolicitations({ pageNumber: '2', pageSize: '10' })

    expect(store.solicitations).toHaveLength(1)
    expect(store.totalCount).toBe(11)
    expect(store.pageNumber).toBe(2)
    expect(store.pageSize).toBe(10)
    expect(store.totalPages).toBe(2)
    expect(store.totalItemsRequested).toBe(30)
    expect(store.totalAmountRequested).toBe(580)
    expect(store.hasNextPage).toBe(false)
    expect(store.error).toBeNull()
    expect(store.isLoading).toBe(false)
  })

  it('deve tratar erro na busca pública', async () => {
    const store = usePublicDataStore()
    publicDataServiceMock.getSolicitations.mockRejectedValue(new Error('erro consulta'))

    await store.fetchPublicSolicitations({})

    expect(store.solicitations).toEqual([])
    expect(store.totalCount).toBe(0)
    expect(store.totalPages).toBe(1)
    expect(store.error).toBe('erro consulta')
    expect(store.isLoading).toBe(false)
  })

  it('deve exportar arquivo e limpar flag de loading', async () => {
    const store = usePublicDataStore()
    const blob = new Blob(['ok'])
    publicDataServiceMock.exportSolicitations.mockResolvedValue(blob)

    const result = await store.exportPublicSolicitations({ pageSize: '25' }, 'csv')

    expect(publicDataServiceMock.exportSolicitations).toHaveBeenCalledWith({ pageSize: '25' }, 'csv')
    expect(result).toBe(blob)
    expect(store.isExporting).toBe(false)
    expect(store.error).toBeNull()
  })

  it('deve propagar erro ao falhar exportação', async () => {
    const store = usePublicDataStore()
    publicDataServiceMock.exportSolicitations.mockRejectedValue(new Error('erro export'))

    await expect(store.exportPublicSolicitations({}, 'json')).rejects.toThrow('erro export')
    expect(store.error).toBe('erro export')
    expect(store.isExporting).toBe(false)
  })
})
