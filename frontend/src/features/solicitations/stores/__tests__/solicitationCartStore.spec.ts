import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useSolicitationCartStore } from '../solicitationCartStore'
import { useSettingStore } from '@/features/settings/stores/settingStore'
import { solicitationService } from '../../services/solicitationService'
import type { Item } from '@/features/catalogo/types'

// --- MOCKS ---
vi.mock('../../services/solicitationService', () => ({
  solicitationService: {
    create: vi.fn(),
  },
}))

describe('Store: solicitationCart', () => {
  
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  // --- Factories ---
  const createMockItem = (id: number, preco = 100): Item => ({
    id,
    nome: `Item ${id}`,
    catMat: '12345',
    descricao: 'Descrição do item',
    especificacao: 'Especificação',
    categoria: { id: 1, nome: 'Categoria', descricao: '', isActive: true },
    linkImagem: '',
    precoSugerido: preco,
    isActive: true
  })

  describe('Adição de Itens (addItem)', () => {
    
    it('deve adicionar um novo item e definir o tipo da solicitação', () => {
      const cartStore = useSolicitationCartStore()
      const item = createMockItem(1)

      const result = cartStore.addItem(item, 'geral')

      expect(result).toBe('added')
      expect(cartStore.solicitationItems).toHaveLength(1)
      expect(cartStore.solicitationItems[0].quantidade).toBe(1)
      expect(cartStore.solicitationType).toBe('geral')
    })

    it('deve incrementar a quantidade se o item já existir', () => {
      const cartStore = useSolicitationCartStore()
      const item = createMockItem(1)

      cartStore.addItem(item, 'geral') // Adiciona 1ª vez
      const result = cartStore.addItem(item, 'geral') // Adiciona 2ª vez

      expect(result).toBe('incremented')
      expect(cartStore.solicitationItems).toHaveLength(1)
      expect(cartStore.solicitationItems[0].quantidade).toBe(2)
    })

    it('deve respeitar o limite de quantidade por item (Configuração)', () => {
      const cartStore = useSolicitationCartStore()
      const settingStore = useSettingStore()
      
      // Configura o limite na store de settings
      settingStore.settings = { maxQuantidadePorItem: 2 } as any
      
      const item = createMockItem(1)

      cartStore.addItem(item, 'geral') // qtd: 1
      cartStore.addItem(item, 'geral') // qtd: 2 (Limite atingido)
      
      const result = cartStore.addItem(item, 'geral') // Tentativa de ir para 3

      expect(result).toBe('quantity_limit_exceeded')
      expect(cartStore.solicitationItems[0].quantidade).toBe(2)
    })

    it('deve respeitar o limite de itens diferentes por solicitação (Configuração)', () => {
      const cartStore = useSolicitationCartStore()
      const settingStore = useSettingStore()

      // Configura limite de 2 itens diferentes
      settingStore.settings = { maxItensDiferentesPorSolicitacao: 2 } as any

      cartStore.addItem(createMockItem(1), 'patrimonial')
      cartStore.addItem(createMockItem(2), 'patrimonial')
      
      // Tenta adicionar o 3º item diferente
      const result = cartStore.addItem(createMockItem(3), 'patrimonial')

      expect(result).toBe('item_limit_exceeded')
      expect(cartStore.solicitationItems).toHaveLength(2)
    })

    it('não deve aplicar limites se settings for null/undefined', () => {
      const cartStore = useSolicitationCartStore()
      // settingStore.settings começa null por padrão

      const item = createMockItem(1)
      // Adiciona 50 vezes (exagero proposital)
      for(let i=0; i<50; i++) cartStore.addItem(item, 'geral')

      expect(cartStore.solicitationItems[0].quantidade).toBe(50)
    })
  })

  describe('Remoção e Atualização', () => {
    it('deve remover um item pelo ID', () => {
      const cartStore = useSolicitationCartStore()
      cartStore.addItem(createMockItem(1), 'geral')
      
      const result = cartStore.removeItem(1)

      expect(result).toBe('removed')
      expect(cartStore.solicitationItems).toHaveLength(0)
    })

    it('deve atualizar a quantidade de um item diretamente', () => {
      const cartStore = useSolicitationCartStore()
      cartStore.addItem(createMockItem(1), 'geral')

      cartStore.updateItemQuantity(1, 10)

      expect(cartStore.solicitationItems[0].quantidade).toBe(10)
    })

    it('não deve quebrar se tentar atualizar item inexistente', () => {
      const cartStore = useSolicitationCartStore()
      expect(() => cartStore.updateItemQuantity(999, 5)).not.toThrow()
    })
  })

  describe('Criação de Solicitação (createSolicitation)', () => {
    it('deve enviar payload correto para solicitação GERAL', async () => {
      const cartStore = useSolicitationCartStore()
      const item = createMockItem(1, 150.50)
      
      cartStore.addItem(item, 'geral')
      cartStore.justification = 'Preciso disso'

      vi.mocked(solicitationService.create).mockResolvedValue({} as any)

      const success = await cartStore.createSolicitation(true) // isGeneral = true

      expect(success).toBe(true)
      expect(solicitationService.create).toHaveBeenCalledWith({
        type: 'geral',
        justificativaGeral: 'Preciso disso',
        itens: [{
          itemId: 1,
          quantidade: 1,
          valorUnitario: 150.50
        }]
      })
      // Verifica se resetou após sucesso
      expect(cartStore.solicitationItems).toHaveLength(0)
    })

    it('deve enviar payload correto para solicitação PATRIMONIAL', async () => {
      const cartStore = useSolicitationCartStore()
      const item = createMockItem(2, 2000)
      
      cartStore.addItem(item, 'patrimonial')
      // Adiciona justificativa específica no item (comum em patrimonial)
      cartStore.solicitationItems[0].justificativa = 'Uso pessoal'

      vi.mocked(solicitationService.create).mockResolvedValue({} as any)

      const success = await cartStore.createSolicitation(false) // isGeneral = false

      expect(success).toBe(true)
      expect(solicitationService.create).toHaveBeenCalledWith({
        type: 'patrimonial',
        itens: [{
          itemId: 2,
          quantidade: 1,
          valorUnitario: 2000,
          justificativa: 'Uso pessoal'
        }]
      })
    })

    it('deve lidar com erros na criação', async () => {
      const cartStore = useSolicitationCartStore()
      cartStore.addItem(createMockItem(1), 'geral')

      vi.mocked(solicitationService.create).mockRejectedValue(new Error('Erro API'))

      const success = await cartStore.createSolicitation(true)

      expect(success).toBe(false)
      expect(cartStore.error).toBe('Erro API')
      expect(cartStore.isLoading).toBe(false)
      // Não deve resetar o carrinho em caso de erro, para o usuário tentar de novo
      expect(cartStore.solicitationItems).toHaveLength(1)
    })
  })

  describe('$reset', () => {
    it('deve limpar todo o estado do carrinho', () => {
      const cartStore = useSolicitationCartStore()
      
      // Suja o estado
      cartStore.addItem(createMockItem(1), 'geral')
      cartStore.justification = 'Bla'
      cartStore.error = 'Erro'

      cartStore.$reset()

      expect(cartStore.solicitationItems).toEqual([])
      expect(cartStore.justification).toBe('')
      expect(cartStore.solicitationType).toBeNull()
      expect(cartStore.error).toBeNull()
    })
  })
})