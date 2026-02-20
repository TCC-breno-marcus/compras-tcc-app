import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { getDashboardsMock } = vi.hoisted(() => ({
  getDashboardsMock: vi.fn(),
}))

vi.mock('../../services/dashService', () => ({
  dashService: {
    getDashboards: getDashboardsMock,
  },
}))

import { useDashboardStore } from '../dashStore'

describe('Store: dashStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  const dashboardResponse = {
    kpis: {
      valorTotalEstimado: 100,
      custoMedioSolicitacao: 50,
      totalItensUnicos: 3,
      totalUnidadesSolicitadas: 10,
      totalDepartamentosSolicitantes: 2,
      totalSolicitacoes: 4,
    },
    valorPorDepartamento: { labels: ['A'], data: [1] },
    valorPorCategoria: { labels: ['B'], data: [2] },
    visaoGeralStatus: { labels: ['P'], data: [3] },
    topItensPorQuantidade: [],
    topItensPorValorTotal: [],
  }

  it('deve buscar dashboard e preencher computeds', async () => {
    const store = useDashboardStore()
    getDashboardsMock.mockResolvedValue(dashboardResponse)

    await store.fetchGestorDashboard()

    expect(store.kpis).toEqual(dashboardResponse.kpis)
    expect(store.valorPorDepartamento).toEqual(dashboardResponse.valorPorDepartamento)
    expect(store.isLoading).toBe(false)
  })

  it('deve evitar nova chamada se dados já estiverem carregados', async () => {
    const store = useDashboardStore()
    getDashboardsMock.mockResolvedValue(dashboardResponse)

    await store.fetchGestorDashboard()
    await store.fetchGestorDashboard()

    expect(getDashboardsMock).toHaveBeenCalledTimes(1)
  })

  it('deve preencher erro ao falhar', async () => {
    const store = useDashboardStore()
    getDashboardsMock.mockRejectedValue(new Error('falhou'))

    await store.fetchGestorDashboard()

    expect(store.error).toBe('falhou')
    expect(store.isLoading).toBe(false)
  })
})

