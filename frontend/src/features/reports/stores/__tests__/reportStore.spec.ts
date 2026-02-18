import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { reportServiceMock } = vi.hoisted(() => ({
  reportServiceMock: {
    getItemsPerDepartment: vi.fn(),
    getCenterExpenses: vi.fn(),
    getCategoryConsumption: vi.fn(),
  },
}))

vi.mock('../../services/reportService', () => ({
  reportService: reportServiceMock,
}))

import { useReportStore } from '../reportStore'

describe('Store: reportStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('deve buscar itens por departamento e preencher paginação', async () => {
    const store = useReportStore()
    reportServiceMock.getItemsPerDepartment.mockResolvedValue({
      data: [{ id: 1, nome: 'Item' }],
      totalCount: 25,
      pageNumber: 1,
      pageSize: 10,
    })

    await store.fetchItemsPerDepartment({} as any)

    expect(store.itemsDepartment).toHaveLength(1)
    expect(store.totalCount).toBe(25)
    expect(store.totalPages).toBe(3)
    expect(store.hasNextPage).toBe(true)
  })

  it('deve gerar relatório de gastos por centro', async () => {
    const store = useReportStore()
    reportServiceMock.getCenterExpenses.mockResolvedValue([{ centroId: 1 }])

    await store.generateReport('GASTOS_CENTRO', { DataInicio: '2026-01-01', DataFim: '2026-01-31' })

    expect(store.activeReportType).toBe('GASTOS_CENTRO')
    expect(store.centerData).toEqual([{ centroId: 1 }])
    expect(store.categoryData).toEqual([])
  })

  it('deve gerar relatório de consumo por categoria', async () => {
    const store = useReportStore()
    reportServiceMock.getCategoryConsumption.mockResolvedValue([{ categoriaNome: 'cat' }])

    await store.generateReport('CONSUMO_CATEGORIA', {
      DataInicio: '2026-01-01',
      DataFim: '2026-01-31',
    })

    expect(store.activeReportType).toBe('CONSUMO_CATEGORIA')
    expect(store.categoryData).toEqual([{ categoriaNome: 'cat' }])
    expect(store.centerData).toEqual([])
  })

  it('deve limpar activeReportType quando geração falhar', async () => {
    const store = useReportStore()
    reportServiceMock.getCenterExpenses.mockRejectedValue(new Error('erro geração'))

    await store.generateReport('GASTOS_CENTRO', { DataInicio: '2026-01-01', DataFim: '2026-01-31' })

    expect(store.error).toBe('erro geração')
    expect(store.activeReportType).toBeNull()
  })

  it('deve resetar estado', async () => {
    const store = useReportStore()
    store.itemsDepartment = [{ id: 1 } as any]
    store.error = 'erro'
    store.activeReportType = 'GASTOS_CENTRO'

    store.$reset()

    expect(store.itemsDepartment).toEqual([])
    expect(store.error).toBeNull()
    expect(store.activeReportType).toBeNull()
  })
})

