import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Item } from '@/features/catalogo/types'
import type { SolicitationItem } from '@/features/solicitations'

export const useSolicitationStore = defineStore('solicitation', () => {
  const solicitationItems = ref<SolicitationItem[]>([])
  const justification = ref<string>('')
  const solicitationType = ref<'geral' | 'patrimonial' | null>(null)

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

  return {
    solicitationItems,
    justification,
    solicitationType,
    addItem,
    removeItem,
    updateItemQuantity,
    clearSolicitation,
  }
})
