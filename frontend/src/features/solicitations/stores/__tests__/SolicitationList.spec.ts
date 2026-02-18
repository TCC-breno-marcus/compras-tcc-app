import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useSolicitationListStore } from '../SolicitationList' // Ajuste o nome do arquivo se necessário
import { solicitationService } from '../../services/solicitationService'
import { transformSolicitation } from '../../utils'
import type { Solicitation, SolicitationFilters } from '../../types'
import type { PaginatedResponse } from '@/types'

// --- MOCKS ---

// 1. Mock do Service
vi.mock('../../services/solicitationService', () => ({
  solicitationService: {
    getAllSolicitations: vi.fn(),
  },
}))

// 2. Mock do Utils
vi.mock('../../utils', () => ({
  transformSolicitation: vi.fn(),
}))

describe('Store: SolicitationList (Admin/Gestor)', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  // --- Factories ---
  const createMockSolicitation = (id: number): Solicitation => ({
    id,
    dataCriacao: '2023-01-01',
    justificativaGeral: 'Teste',
    externalId: `SOL-${id}`,
    status: { id: 1, nome: 'Pendente', descricao: '' },
    solicitante: { id: '1', nome: 'João da Silva', email: 'j@j.com', unidade: {} as any },
    itens: [],
    kpis: {} as any,
    valorPorCategoria: {} as any,
    topItensPorValor: [],
  })

  const createPaginatedResponse = (totalCount: number): PaginatedResponse<Solicitation> => ({
    data: [createMockSolicitation(1)],
    pageNumber: 1,
    pageSize: 10,
    totalCount: totalCount,
    totalPages: Math.ceil(totalCount / 10),
  })

  it('deve inicializar com estado padrão', () => {
    const store = useSolicitationListStore()

    expect(store.solicitations).toEqual([])
    expect(store.isLoading).toBe(false)
    expect(store.error).toBeNull()
    expect(store.pageNumber).toBe(1)
  })

  describe('fetchAll', () => {
    it('deve buscar TODAS as solicitações e usar o transformer correto', async () => {
      const store = useSolicitationListStore()

      // Mock do Service
      const mockResponse = createPaginatedResponse(30)
      vi.mocked(solicitationService.getAllSolicitations).mockResolvedValue(mockResponse)

      // Mock do Transformer retornando item com requester
      vi.mocked(transformSolicitation).mockReturnValue({
        id: 1,
        requester: 'João da Silva',
      } as any)

      // Filtros de exemplo (Admin filtra por departamento, por exemplo)
      const filters: SolicitationFilters = { siglaDepartamento: 'TI' }

      await store.fetchAll(filters)

      // 1. Verifica sucesso
      expect(store.isLoading).toBe(false)
      expect(store.solicitations).toHaveLength(1)
      expect(store.solicitations[0].requester).toBe('João da Silva')
      expect(store.totalCount).toBe(30)

      // 2. VERIFICAÇÃO CHAVE: O Service correto foi chamado?
      expect(solicitationService.getAllSolicitations).toHaveBeenCalledWith(filters)

      // 3. VERIFICAÇÃO CHAVE: O Transformer foi chamado com 'allSolicitations'?
      expect(transformSolicitation).toHaveBeenCalledWith(
        expect.anything(),
        'allSolicitations', // <--- Garante que vai mostrar o nome do solicitante
      )
    })

    it('deve lidar com erro na API', async () => {
      const store = useSolicitationListStore()
      const errorMsg = 'Erro 500'
      vi.mocked(solicitationService.getAllSolicitations).mockRejectedValue(new Error(errorMsg))

      await store.fetchAll()

      expect(store.isLoading).toBe(false)
      expect(store.solicitations).toEqual([])
      expect(store.error).toBe(errorMsg)
    })
  })

  describe('$reset', () => {
    it('deve resetar o estado', () => {
      const store = useSolicitationListStore()
      store.solicitations = [{ id: 1 } as any]
      store.totalCount = 100

      store.$reset()

      expect(store.solicitations).toEqual([])
      expect(store.totalCount).toBe(0)
    })
  })
})
