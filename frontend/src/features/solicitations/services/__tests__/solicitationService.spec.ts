import { describe, it, expect, vi, beforeEach } from 'vitest'
import { solicitationService } from '../solicitationService'
import { apiClient } from '@/services/apiClient'
import type { 
  CreateSolicitationPayload, 
  Solicitation,
  SolicitationHistoryEvent 
} from '../../types'
import type { PaginatedResponse } from '@/types'


vi.mock('@/services/apiClient', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    patch: vi.fn(),
    delete: vi.fn(),
  },
}))

describe('SolicitationService', () => {
  
  const createMockSolicitation = (overrides?: Partial<Solicitation>): Solicitation => ({
    id: 1,
    dataCriacao: '2023-01-01',
    justificativaGeral: 'Teste Geral',
    externalId: 'SOL-001',
    status: { id: 1, nome: 'Pendente', descricao: 'Aguardando' },
    solicitante: { id: '1', nome: 'Tester', email: 'test@co.com', unidade: {} as any },
    itens: [],
    // Campos obrigatórios pela sua interface:
    kpis: { valorTotalEstimado: 0, totalItensUnicos: 0, totalUnidades: 0 },
    valorPorCategoria: { labels: [], datasets: [] },
    topItensPorValor: [],
    ...overrides
  } as Solicitation)

  const createPaginatedResponse = (items: Solicitation[]): PaginatedResponse<Solicitation> => ({
    data: items,
    pageNumber: 1,
    pageSize: 10,
    totalCount: items.length,
    totalPages: 1
  })

  beforeEach(() => {
    vi.clearAllMocks()
  })

  // --- TESTES DE CREATE (POST) ---
  describe('create', () => {
    it('deve enviar requisição correta para solicitação GERAL', async () => {
      // Mock da resposta do backend (201 Created)
      const mockResponse = createMockSolicitation({ id: 100 })
      vi.mocked(apiClient.post).mockResolvedValue({ data: mockResponse })

      const payload: CreateSolicitationPayload = {
        type: 'geral',
        justificativaGeral: 'Preciso de um monitor',
        itens: [{ itemId: 1, quantidade: 2, valorUnitario: 500 }]
      }

      const result = await solicitationService.create(payload)

      expect(result).toEqual(mockResponse)
      // Verifica se a URL e o Body estão corretos (sem o campo 'type' que é removido no service)
      expect(apiClient.post).toHaveBeenCalledWith('/solicitacao/geral', {
        justificativaGeral: 'Preciso de um monitor',
        itens: [{ itemId: 1, quantidade: 2, valorUnitario: 500 }]
      })
    })

    it('deve enviar requisição correta para solicitação PATRIMONIAL', async () => {
      const mockResponse = createMockSolicitation({ id: 101 })
      vi.mocked(apiClient.post).mockResolvedValue({ data: mockResponse })

      const payload: CreateSolicitationPayload = {
        type: 'patrimonial',
        itens: [{ itemId: 5, quantidade: 1, valorUnitario: 3000 }]
      }

      await solicitationService.create(payload)

      expect(apiClient.post).toHaveBeenCalledWith('/solicitacao/patrimonial', {
        itens: [{ itemId: 5, quantidade: 1, valorUnitario: 3000 }]
      })
    })

    it('deve lançar erro tratado quando a API falha', async () => {
      vi.mocked(apiClient.post).mockRejectedValue(new Error('Erro de Rede'))

      const payload = { type: 'geral', justificativaGeral: '', itens: [] } as any

      await expect(solicitationService.create(payload))
        .rejects
        .toThrow('Não foi possível criar a solicitação.')
    })
  })

  // --- TESTES DE UPDATE (PATCH) ---
  describe('update', () => {
    it('deve atualizar uma solicitação existente', async () => {
      const mockResponse = createMockSolicitation({ justificativaGeral: 'Editado' })
      vi.mocked(apiClient.patch).mockResolvedValue({ data: mockResponse })

      const result = await solicitationService.update(1, { justificativaGeral: 'Editado' })

      expect(result.justificativaGeral).toBe('Editado')
      expect(apiClient.patch).toHaveBeenCalledWith('/solicitacao/1', { justificativaGeral: 'Editado' })
    })
  })

  // --- TESTES DE GET BY ID ---
  describe('getById', () => {
    it('deve buscar solicitação pelo ID', async () => {
      const mockResponse = createMockSolicitation()
      vi.mocked(apiClient.get).mockResolvedValue({ data: mockResponse })

      const result = await solicitationService.getById(123)

      expect(result.id).toBe(1)
      expect(apiClient.get).toHaveBeenCalledWith('/solicitacao/123')
    })
  })

  // --- TESTES DE GET MY SOLICITATIONS (FILTROS) ---
  describe('getMySolicitations', () => {
    it('deve converter filtros complexos em Query Params', async () => {
      const mockResponse = createPaginatedResponse([])
      vi.mocked(apiClient.get).mockResolvedValue({ data: mockResponse })

      const filters = {
        pageNumber: '1',
        pageSize: '10',
        externalId: 'SOL',
        statusIds: [1, 2],
        dateRange: [new Date('2023-01-01'), new Date('2023-01-31')]
      }

      await solicitationService.getMySolicitations(filters as any)

      // Inspeciona os argumentos da chamada
      const [url, config] = vi.mocked(apiClient.get).mock.calls[0]
      const params = config?.params as URLSearchParams

      expect(url).toBe('/solicitacao/minhas-solicitacoes')
      expect(params.get('pageNumber')).toBe('1')
      expect(params.get('externalId')).toBe('SOL')
      // Verifica arrays e datas transformadas
      expect(params.getAll('statusIds')).toEqual(['1', '2'])
      expect(params.get('dataInicial')).toBe('2023-01-01')
      expect(params.get('dataFinal')).toBe('2023-01-31')
    })

    it('deve ignorar filtros nulos ou vazios', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: createPaginatedResponse([]) })

      await solicitationService.getMySolicitations({ externalId: '' } as any)

      const config = vi.mocked(apiClient.get).mock.calls[0][1]
      const params = config?.params as URLSearchParams
      
      // Não deve ter enviado externalId vazio
      expect(params.has('externalId')).toBe(false)
    })
  })

  // --- TESTES DE GET ALL SOLICITATIONS (ADMIN/GESTOR) ---
  describe('getAllSolicitations', () => {
    it('deve chamar o endpoint correto', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: createPaginatedResponse([]) })

      await solicitationService.getAllSolicitations()

      expect(apiClient.get).toHaveBeenCalledWith('/solicitacao', expect.anything())
    })
  })

  // --- TESTES DE HISTÓRICO ---
  describe('getSolicitationHistory', () => {
    it('deve retornar array de eventos históricos', async () => {
      const mockHistory: SolicitationHistoryEvent[] = [
        { id: '1', acao: 'Criação', dataOcorrencia: '2023', detalhes: null, nomePessoa: 'User', observacoes: null }
      ]
      vi.mocked(apiClient.get).mockResolvedValue({ data: mockHistory })

      const result = await solicitationService.getSolicitationHistory(99)

      expect(result).toHaveLength(1)
      expect(result[0].acao).toBe('Criação')
      expect(apiClient.get).toHaveBeenCalledWith('/solicitacao/99/historico')
    })
  })

  // --- TESTES DE ATUALIZAR STATUS ---
  describe('updateStatus', () => {
    it('deve enviar payload de atualização de status', async () => {
      const mockResponse = createMockSolicitation({ status: { id: 3, nome: 'Aprovada', descricao: '' } })
      vi.mocked(apiClient.patch).mockResolvedValue({ data: mockResponse })

      await solicitationService.updateStatus(1, { novoStatusId: 3, observacoes: 'Aprovado OK' })

      expect(apiClient.patch).toHaveBeenCalledWith(
        '/solicitacao/1/status', 
        { novoStatusId: 3, observacoes: 'Aprovado OK' }
      )
    })
  })
})