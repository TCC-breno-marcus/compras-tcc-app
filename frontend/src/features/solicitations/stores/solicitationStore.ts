import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Item } from '@/features/catalogo/types'
import type { SolicitationItem } from '@/features/solicitations'

export const useSolicitationStore = defineStore('solicitation', () => {
  const solicitationItems = ref<SolicitationItem[]>([])
  const justification = ref<string>('')

  function addItem(item: Item) {
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
  }

  return {
    solicitationItems,
    justification,
    addItem,
    removeItem,
    updateItemQuantity,
    clearSolicitation,
  }
})
