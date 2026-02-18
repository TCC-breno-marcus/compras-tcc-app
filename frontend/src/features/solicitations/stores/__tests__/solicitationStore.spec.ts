import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useSolicitationStore } from '../solicitationStore'
import { solicitationService } from '../../services/solicitationService'
import { useSettingStore } from '@/features/settings/stores/settingStore'
import { dataHasBeenChanged } from '@/utils/objectUtils'
import type { Item } from '@/features/catalogo/types'
import type { Solicitation } from '../../types'

// --- MOCKS ---

// 1. Service
vi.mock('../../services/solicitationService', () => ({
  solicitationService: {
    getById: vi.fn(),
    update: vi.fn(),
    updateStatus: vi.fn(),
  },
}))

// 2. Utils (dataHasBeenChanged)
vi.mock('@/utils/objectUtils', () => ({
  dataHasBeenChanged: vi.fn(),
}))

// 3. PrimeVue Toast
const toastAddMock = vi.fn()
vi.mock('primevue', () => ({
  useToast: () => ({
    add: toastAddMock,
  }),
}))

describe('Store: solicitation (Detalhes)', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  // --- Factories (Corrigidas com Tipagem Completa) ---

  const createMockItem = (id: number): Item => ({
    id,
    nome: `Item ${id}`,
    catMat: '12345',
    descricao: 'Descrição do item',
    especificacao: 'Especificação',
    categoria: { id: 1, nome: 'Categoria', descricao: '', isActive: true },
    linkImagem: '',
    precoSugerido: 100,
    isActive: true,
  })

  // Factory corrigida para atender UnitOrganizational e ChartData
  const createMockSolicitation = (id: number): Solicitation => ({
    id,
    dataCriacao: '2023-01-01',
    justificativaGeral: 'Justificativa Original',
    externalId: `SOL-${id}`,
    status: { id: 1, nome: 'Pendente', descricao: 'Aguardando' },
    solicitante: {
      id: '1',
      nome: 'User',
      email: 'user@example.com',
      unidade: {
        id: 1,
        nome: 'Unidade Mock',
        sigla: 'UND',
        // Campos obrigatórios adicionados:
        email: 'unidade@test.com',
        telefone: '9999-9999',
        tipo: 'Departamento',
      },
    },
    itens: [
      {
        ...createMockItem(1),
        quantidade: 1,
        justificativa: 'Justificativa Item',
      },
    ],
    kpis: { valorTotalEstimado: 100, totalItensUnicos: 1, totalUnidades: 1 },
    // Correção ChartData: usa 'data' em vez de 'datasets'
    valorPorCategoria: { labels: [], data: [] },
    topItensPorValor: [],
  })

  describe('fetchById', () => {
    it('deve buscar solicitação e criar backup', async () => {
      const store = useSolicitationStore()
      const mockData = createMockSolicitation(10)

      vi.mocked(solicitationService.getById).mockResolvedValue(mockData)

      await store.fetchById(10)

      expect(store.currentSolicitation).toEqual(mockData)
      expect(store.currentSolicitationBackup).toEqual(mockData)
      expect(store.isLoading).toBe(false)
    })

    it('deve lidar com erro no fetch', async () => {
      const store = useSolicitationStore()
      vi.mocked(solicitationService.getById).mockRejectedValue(new Error('Erro Fatal'))

      await store.fetchById(10)

      expect(store.currentSolicitation).toBeNull()
      expect(store.error).toBe('Erro Fatal')
    })
  })

  describe('update (Salvar Edição)', () => {
    it('deve transformar dados e enviar update corretamente', async () => {
      const store = useSolicitationStore()
      const original = createMockSolicitation(10)

      store.currentSolicitation = original

      const updatedData = { ...original, justificativaGeral: 'Editado' }
      vi.mocked(solicitationService.update).mockResolvedValue(updatedData)

      // Usar '!' pois sabemos que não é null aqui
      const success = await store.update(store.currentSolicitation!)

      expect(solicitationService.update).toHaveBeenCalledWith(10, {
        justificativaGeral: 'Justificativa Original',
        itens: [
          {
            itemId: 1,
            quantidade: 1,
            valorUnitario: 100,
            justificativa: 'Justificativa Item',
          },
        ],
      })

      expect(success).toBe(true)
      expect(store.currentSolicitation).toEqual(updatedData)
      expect(store.currentSolicitationBackup).toEqual(updatedData)

      expect(toastAddMock).toHaveBeenCalledWith(expect.objectContaining({ severity: 'success' }))
    })

    it('deve lidar com erro ao salvar', async () => {
      const store = useSolicitationStore()
      store.currentSolicitation = createMockSolicitation(10)

      vi.mocked(solicitationService.update).mockRejectedValue(new Error('Erro Salvar'))

      await store.update(store.currentSolicitation!)

      expect(store.error).toBe('Erro Salvar')
      expect(toastAddMock).toHaveBeenCalledWith(expect.objectContaining({ severity: 'error' }))
    })
  })

  describe('Gerenciamento de Itens (Add/Remove/Update)', () => {
    it('deve adicionar item novo', () => {
      const store = useSolicitationStore()
      store.currentSolicitation = createMockSolicitation(10)

      const result = store.addItem(createMockItem(2))

      expect(result).toBe('added')
      expect(store.currentSolicitation!.itens).toHaveLength(2)
    })

    it('deve incrementar quantidade de item existente', () => {
      const store = useSolicitationStore()
      store.currentSolicitation = createMockSolicitation(10) // Tem item 1 (qtd: 1)

      const result = store.addItem(createMockItem(1))

      expect(result).toBe('incremented')
      expect(store.currentSolicitation!.itens[0].quantidade).toBe(2)
    })

    it('deve respeitar limites (Configuração)', () => {
      const store = useSolicitationStore()
      store.currentSolicitation = createMockSolicitation(10)

      const settingStore = useSettingStore()
      settingStore.settings = { maxQuantidadePorItem: 1 } as any

      const result = store.addItem(createMockItem(1))

      expect(result).toBe('quantity_limit_exceeded')
      expect(store.currentSolicitation!.itens[0].quantidade).toBe(1)
    })

    it('deve remover item', () => {
      const store = useSolicitationStore()
      store.currentSolicitation = createMockSolicitation(10)

      store.removeItem(1)

      expect(store.currentSolicitation!.itens).toHaveLength(0)
    })

    it('deve atualizar quantidade diretamente', () => {
      const store = useSolicitationStore()
      store.currentSolicitation = createMockSolicitation(10)

      store.updateItemQuantity(1, 50)

      expect(store.currentSolicitation!.itens[0].quantidade).toBe(50)
    })
  })

  describe('isDirty (Computed)', () => {
    it('deve usar dataHasBeenChanged para detectar mudanças', () => {
      const store = useSolicitationStore()
      store.currentSolicitation = createMockSolicitation(1)
      store.currentSolicitationBackup = createMockSolicitation(1)

      // Caso 1: Mock retorna true (Mudou)
      vi.mocked(dataHasBeenChanged).mockReturnValue(true)
      expect(store.isDirty).toBe(true)
      store.currentSolicitation = { ...store.currentSolicitation }

      // Caso 2: Mock retorna false (Igual)
      vi.mocked(dataHasBeenChanged).mockReturnValue(false)
      expect(store.isDirty).toBe(false)

      expect(dataHasBeenChanged).toHaveBeenCalled()
    })

    it('deve retornar false se não houver solicitação carregada', () => {
      const store = useSolicitationStore()
      store.currentSolicitation = null
      expect(store.isDirty).toBe(false)
    })
  })

  describe('updateStatus', () => {
    it('deve chamar service e atualizar estado', async () => {
      const store = useSolicitationStore()
      store.currentSolicitation = createMockSolicitation(10)

      const responseWithNewStatus: Solicitation = {
        ...store.currentSolicitation,
        status: { id: 3, nome: 'Aprovado', descricao: 'Status Aprovado' },
      }

      vi.mocked(solicitationService.updateStatus).mockResolvedValue(responseWithNewStatus)

      await store.updateStatus(3, 'Aprovado')

      expect(solicitationService.updateStatus).toHaveBeenCalledWith(10, {
        novoStatusId: 3,
        observacoes: 'Aprovado',
      })
      expect(store.currentSolicitation!.status.id).toBe(3)
      expect(toastAddMock).toHaveBeenCalledWith(expect.objectContaining({ severity: 'success' }))
    })
  })
})
