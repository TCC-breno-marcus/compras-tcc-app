import { ref } from 'vue'
import { defineStore } from 'pinia'
import { useToast } from 'primevue'
import type { ItemHistoryEvent } from '../types'
import { catalogoService } from '../services/catalogoService'

/**
 * Store para gerenciar estado de histórico de uma solicitação
 */
export const useItemHistoryStore = defineStore('historyItem', () => {
  const itemHistory = ref<ItemHistoryEvent[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const toast = useToast()

  /**
   * Busca o histórico de um item.
   * @param itemId O ID do item.
   */
  async function fetchItemHistory(itemId: number) {
    if (itemHistory.value.length > 0) return

    isLoading.value = true
    try {
      itemHistory.value = await catalogoService.getItemHistory(itemId)
    } catch (err: any) {
      error.value = err.message || 'Falha ao buscar histórico do item.'
      toast.add({
        severity: 'error',
        summary: 'Erro',
        detail: 'Não foi possível buscar o histórico do item.',
        life: 3000,
      })
      return false
    } finally {
      isLoading.value = false
    }
  }

  function clearHistory() {
    itemHistory.value = []
    isLoading.value = false
    error.value = null
  }

  return { itemHistory, isLoading, error, fetchItemHistory, clearHistory }
})
