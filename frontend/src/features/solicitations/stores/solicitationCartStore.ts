import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Item } from '@/features/catalogo/types'
import type { CreateSolicitationPayload, SolicitationItem } from '@/features/solicitations'
import { solicitationService } from '../services/solicitationService'

export const useSolicitationCartStore = defineStore('solicitationCart', () => {
  const solicitationItems = ref<SolicitationItem[]>([])
  const justification = ref<string>('')
  const solicitationType = ref<'geral' | 'patrimonial' | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const addItem = (item: Item, type: 'geral' | 'patrimonial') => {
    if (solicitationItems.value.length === 0) {
      solicitationType.value = type
    }

    const itemExistente = solicitationItems.value.find((i) => i.id === item.id)

    if (itemExistente) {
      itemExistente.quantidade++
      return 'incremented'
    } else {
      solicitationItems.value.push({ ...item, quantidade: 1 })
      return 'added'
    }
  }

  const removeItem = (itemId: number) => {
    solicitationItems.value = solicitationItems.value.filter((i) => i.id !== itemId)
    return 'removed'
  }

  const updateItemQuantity = (itemId: number, newQuantity: number) => {
    const item = solicitationItems.value.find((i) => i.id === itemId)
    if (item) {
      item.quantidade = newQuantity
    }
  }

  const clearSolicitation = () => {
    solicitationItems.value = []
    justification.value = ''
    solicitationType.value = null
  }

  const createSolicitation = async (isGeneral?: boolean) => {
    isLoading.value = true
    error.value = null

    let payload: CreateSolicitationPayload

    if (isGeneral) {
      payload = {
        type: 'geral',
        justificativaGeral: justification.value,
        itens: solicitationItems.value.map((item) => ({
          id: item.id!,
          quantidade: item.quantidade,
          valorUnitario: item.precoSugerido!,
        })),
      }
    } else {
      payload = {
        type: 'patrimonial',
        itens: solicitationItems.value.map((item) => ({
          id: item.id!,
          quantidade: item.quantidade,
          valorUnitario: item.precoSugerido!,
          justificativa: item.justificativa,
        })),
      }
    }

    try {
      await solicitationService.create(payload)
      clearSolicitation()
      return true
    } catch (err: any) {
      error.value = err.message || 'Ocorreu um erro desconhecido.'
      return false
    } finally {
      isLoading.value = false
    }
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
    clearSolicitation,
    createSolicitation,
  }
})
