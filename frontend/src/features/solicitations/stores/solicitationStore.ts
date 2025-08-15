import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Item } from '@/features/catalogo/types'
import type { CreateSolicitationPayload, SolicitationItem } from '@/features/solicitations'
import { solicitationService } from '../services/solicitationService'

export const useSolicitationStore = defineStore('solicitation', () => {
  const solicitationItems = ref<SolicitationItem[]>([])
  const justification = ref<string>('')
  const solicitationType = ref<'geral' | 'patrimonial' | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  function addItem(item: Item, type: 'geral' | 'patrimonial') {
    if (solicitationItems.value.length === 0) {
      solicitationType.value = type
    }

    const itemExistente = solicitationItems.value.find((i) => i.id === item.id)

    if (itemExistente) {
      itemExistente.quantity++
      return 'incremented'
    } else {
      solicitationItems.value.push({ ...item, quantity: 1 })
      return 'added'
    }
  }

  function removeItem(itemId: number) {
    solicitationItems.value = solicitationItems.value.filter((i) => i.id !== itemId)
    return 'removed'
  }

  function updateItemQuantity(itemId: number, newQuantity: number) {
    const item = solicitationItems.value.find((i) => i.id === itemId)
    if (item) {
      item.quantity = newQuantity
    }
  }

  function clearSolicitation() {
    solicitationItems.value = []
    justification.value = ''
    solicitationType.value = null
  }

  async function createSolicitation(isGeneral?: boolean) {
    isLoading.value = true
    error.value = null

    let payload: CreateSolicitationPayload

    if (isGeneral) {
      payload = {
        type: 'geral',
        justificativaGeral: justification.value,
        itens: solicitationItems.value.map((item) => ({
          itemId: item.id,
          quantidade: item.quantity,
          valorUnitario: item.precoSugerido,
        })),
      }
    } else {
      payload = {
        type: 'patrimonial',
        itens: solicitationItems.value.map((item) => ({
          itemId: item.id,
          quantidade: item.quantity,
          valorUnitario: item.precoSugerido,
          justificativa: item.justification,
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
