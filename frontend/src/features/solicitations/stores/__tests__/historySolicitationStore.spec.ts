import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useSolicitationHistoryStore } from '../historySolicitationStore'
import { solicitationService } from '../../services/solicitationService'

// --- MOCKS ---

// 1. Mock do Service
vi.mock('../../services/solicitationService', () => ({
  solicitationService: {
    getSolicitationHistory: vi.fn(),
  },
}))

// 2. Mock do PrimeVue
const toastAddMock = vi.fn()
vi.mock('primevue', () => ({
  useToast: () => ({
    add: toastAddMock,
  }),
}))

describe('Store: historySolicitationStore', () => {
  
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  // Factory para criar dados de histórico falsos
  const createMockHistory = () => [
    {
      id: '100',
      dataOcorrencia: '2023-10-10',
      acao: 'Criação',
      detalhes: null,
      observacoes: null,
      nomePessoa: 'João',
    },
  ]

  it('deve inicializar com estado padrão vazio', () => {
    const store = useSolicitationHistoryStore()

    expect(store.solicitationHistory).toEqual([])
    expect(store.isLoading).toBe(false)
    expect(store.error).toBeNull()
  })

  it('deve buscar histórico com sucesso e atualizar o estado', async () => {
    const store = useSolicitationHistoryStore()
    const mockData = createMockHistory()

    vi.mocked(solicitationService.getSolicitationHistory).mockResolvedValue(mockData)

    const promise = store.fetchSolicitationHistory(123)
    
    expect(store.isLoading).toBe(true)
    
    await promise

    expect(store.isLoading).toBe(false)
    expect(store.solicitationHistory).toEqual(mockData)
    expect(store.error).toBeNull()
    expect(solicitationService.getSolicitationHistory).toHaveBeenCalledWith(123)
  })

  it('deve lidar com erros ao buscar histórico (Toast + Error State)', async () => {
    const store = useSolicitationHistoryStore()
    
    const errorMsg = 'Erro interno do servidor'
    vi.mocked(solicitationService.getSolicitationHistory).mockRejectedValue(new Error(errorMsg))

    await store.fetchSolicitationHistory(123)

    expect(store.isLoading).toBe(false)
    expect(store.solicitationHistory).toEqual([])
    expect(store.error).toBe(errorMsg)

    // Verifica se o Toast foi chamado
    expect(toastAddMock).toHaveBeenCalledWith(expect.objectContaining({
      severity: 'error',
      summary: 'Erro',
    }))
  })

  it('NÃO deve buscar novamente se já houver dados no histórico (Cache)', async () => {
    const store = useSolicitationHistoryStore()
    
    store.solicitationHistory = createMockHistory()

    await store.fetchSolicitationHistory(999)

    expect(solicitationService.getSolicitationHistory).not.toHaveBeenCalled()
  })

  it('deve limpar o histórico e resetar estados', () => {
    const store = useSolicitationHistoryStore()
    
    store.solicitationHistory = createMockHistory()
    store.error = 'Algum erro antigo'
    store.isLoading = true

    store.clearHistory()

    expect(store.solicitationHistory).toEqual([])
    expect(store.error).toBeNull()
    expect(store.isLoading).toBe(false)
  })
})