import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useMySolicitationListStore } from '../mySolicitationList'
import { solicitationService } from '../../services/solicitationService'
import { transformSolicitation } from '../../utils'
import type { Solicitation } from '../../types'
import type { PaginatedResponse } from '@/types'

// --- MOCKS ---

// 1. Mock do Service
vi.mock('../../services/solicitationService', () => ({
  solicitationService: {
    getMySolicitations: vi.fn(),
  },
}))

// 2. Mock do Utils (Transformer)
// Isso é importante para garantir que a store está chamando a transformação
// com o parâmetro correto 'mySolicitations'
vi.mock('../../utils', () => ({
  transformSolicitation: vi.fn(),
}))

describe('Store: mySolicitationList', () => {
  
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
    solicitante: { id: '1', nome: 'User', email: 'u@u.com', unidade: {} as any },
    itens: [],
    kpis: {} as any,
    valorPorCategoria: {} as any,
    topItensPorValor: []
  })

  const createPaginatedResponse = (
    totalCount: number, 
    pageSize: number
  ): PaginatedResponse<Solicitation> => ({
    data: [createMockSolicitation(1)],
    pageNumber: 1,
    pageSize: pageSize,
    totalCount: totalCount,
    totalPages: Math.ceil(totalCount / pageSize)
  })

  it('deve inicializar com estado padrão', () => {
    const store = useMySolicitationListStore()

    expect(store.solicitations).toEqual([])
    expect(store.isLoading).toBe(false)
    expect(store.error).toBeNull()
    expect(store.pageNumber).toBe(1)
    expect(store.totalPages).toBe(1)
  })

  describe('fetchAll', () => {
    it('deve buscar dados, transformar e atualizar paginação com sucesso', async () => {
      const store = useMySolicitationListStore()
      
      // Setup: 25 itens, páginas de 10 em 10 (Total 3 páginas)
      const mockResponse = createPaginatedResponse(25, 10)
      vi.mocked(solicitationService.getMySolicitations).mockResolvedValue(mockResponse)
      
      // Setup: Mock do transformer retornando algo identificável
      vi.mocked(transformSolicitation).mockReturnValue({ 
        id: 1, 
        typeDisplay: 'Geral' 
      } as any)

      await store.fetchAll()

      // 1. Verifica estado de sucesso
      expect(store.isLoading).toBe(false)
      expect(store.error).toBeNull()
      
      // 2. Verifica se os dados foram populados com o resultado do transformer
      expect(store.solicitations).toHaveLength(1)
      expect(store.solicitations[0].typeDisplay).toBe('Geral')

      // 3. Verifica lógica de paginação
      expect(store.totalCount).toBe(25)
      expect(store.pageSize).toBe(10)
      expect(store.totalPages).toBe(3) // Math.ceil(25/10) = 3

      // 4. Verifica se o transformer foi chamado com a flag correta
      expect(transformSolicitation).toHaveBeenCalledWith(
        expect.anything(), 
        'mySolicitations'
      )
    })

    it('deve lidar com erro na API', async () => {
      const store = useMySolicitationListStore()
      const errorMsg = 'Erro de conexão'
      vi.mocked(solicitationService.getMySolicitations).mockRejectedValue(new Error(errorMsg))

      await store.fetchAll()

      expect(store.isLoading).toBe(false)
      expect(store.solicitations).toEqual([])
      expect(store.error).toBe(errorMsg)
    })
    
    it('deve garantir que totalPages seja no mínimo 1 (evitar 0 ou NaN)', async () => {
       const store = useMySolicitationListStore()
       // Caso API retorne 0 itens
       const mockResponse = createPaginatedResponse(0, 10)
       vi.mocked(solicitationService.getMySolicitations).mockResolvedValue(mockResponse)
       vi.mocked(transformSolicitation).mockReturnValue({} as any)

       await store.fetchAll()

       expect(store.totalPages).toBe(1) // Lógica do "|| 1" no código
    })
  })

  describe('Getters e Computed', () => {
    it('hasNextPage deve retornar true apenas se houver próxima página', async () => {
      const store = useMySolicitationListStore()
      
      // Cenário 1: Página 1 de 3 -> Tem próxima
      store.pageNumber = 1
      store.totalPages = 3
      expect(store.hasNextPage).toBe(true)

      // Cenário 2: Página 3 de 3 -> Não tem próxima
      store.pageNumber = 3
      store.totalPages = 3
      expect(store.hasNextPage).toBe(false)
    })
  })

  describe('$reset', () => {
    it('deve resetar o estado para os valores iniciais', () => {
      const store = useMySolicitationListStore()
      
      store.solicitations = [{ id: 1 } as any]
      store.pageNumber = 5
      store.totalPages = 10
      store.error = 'Erro'

      store.$reset()

      expect(store.solicitations).toEqual([])
      expect(store.pageNumber).toBe(1)
      expect(store.totalPages).toBe(1)
      expect(store.error).toBeNull()
    })
  })
})