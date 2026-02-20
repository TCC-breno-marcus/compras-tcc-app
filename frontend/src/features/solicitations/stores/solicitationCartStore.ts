import { defineStore, storeToRefs } from 'pinia'
import { ref } from 'vue'
import type { Item } from '@/features/catalogo/types'
import type { CreateSolicitationPayload, SolicitationItem } from '@/features/solicitations/types'
import { solicitationService } from '../services/solicitationService'
import { useSettingStore } from '@/features/settings/stores/settingStore'

/**
 * Store do carrinho de solicitação durante o fluxo de criação.
 */
export const useSolicitationCartStore = defineStore('solicitationCart', () => {
  const solicitationItems = ref<SolicitationItem[]>([])
  const justification = ref<string>('')
  const solicitationType = ref<'geral' | 'patrimonial' | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const settingsStore = useSettingStore()
  const { settings } = storeToRefs(settingsStore)

  /**
   * Adiciona item no carrinho respeitando limites de quantidade por item e total de itens distintos.
   * @param item Item selecionado no catálogo.
   * @param type Tipo da solicitação em construção.
   * @returns Resultado da operação para feedback de UI.
   */
  const addItem = (item: Item, type: 'geral' | 'patrimonial') => {
    if (solicitationItems.value.length === 0) {
      solicitationType.value = type
    }

    const itemExistente = solicitationItems.value.find((i) => i.id === item.id)

    if (itemExistente) {
      const maxQuantity = settings.value?.maxQuantidadePorItem
      if (maxQuantity && itemExistente.quantidade >= maxQuantity) {
        return 'quantity_limit_exceeded'
      }
      itemExistente.quantidade++
      return 'incremented'
    } else {
      const maxItens = settings.value?.maxItensDiferentesPorSolicitacao
      if (maxItens && solicitationItems.value.length >= maxItens) {
        return 'item_limit_exceeded'
      }
      solicitationItems.value.push({ ...item, quantidade: 1 })
      return 'added'
    }
  }

  /**
   * Remove item do carrinho pelo ID.
   * @param itemId ID do item a remover.
   * @returns Resultado da operação para feedback de UI.
   */
  const removeItem = (itemId: number) => {
    solicitationItems.value = solicitationItems.value.filter((i) => i.id !== itemId)
    return 'removed'
  }

  /**
   * Atualiza quantidade de um item existente no carrinho.
   * @param itemId ID do item.
   * @param newQuantity Nova quantidade informada pelo usuário.
   */
  const updateItemQuantity = (itemId: number, newQuantity: number) => {
    const item = solicitationItems.value.find((i) => i.id === itemId)
    if (item) {
      item.quantidade = newQuantity
    }
  }

  /**
   * Monta payload conforme tipo de solicitação e envia para criação no backend.
   * Em solicitações gerais inclui justificativa global; em patrimoniais usa justificativa por item.
   * @param isGeneral Flag para determinar estrutura do payload.
   * @returns `true` quando criação conclui com sucesso.
   */
  const createSolicitation = async (isGeneral?: boolean) => {
    isLoading.value = true
    error.value = null

    let payload: CreateSolicitationPayload

    if (isGeneral) {
      payload = {
        type: 'geral',
        justificativaGeral: justification.value,
        itens: solicitationItems.value.map((item) => ({
          itemId: item.id!,
          quantidade: item.quantidade,
          valorUnitario: item.precoSugerido!,
        })),
      }
    } else {
      payload = {
        type: 'patrimonial',
        itens: solicitationItems.value.map((item) => ({
          itemId: item.id!,
          quantidade: item.quantidade,
          valorUnitario: item.precoSugerido!,
          justificativa: item.justificativa,
        })),
      }
    }

    try {
      await solicitationService.create(payload)
      $reset()
      return true
    } catch (err: any) {
      error.value = err.message || 'Ocorreu um erro desconhecido.'
      return false
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Limpa estado do carrinho e retorna store ao estado inicial.
   */
  const $reset = () => {
    solicitationItems.value = []
    justification.value = ''
    solicitationType.value = null
    isLoading.value = false
    error.value = null
  }

  return {
    solicitationItems,
    justification,
    solicitationType,
    error,
    isLoading,
    addItem,
    removeItem,
    updateItemQuantity,
    createSolicitation,
    $reset,
  }
})
