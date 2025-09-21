import { ref } from 'vue'
import { defineStore } from 'pinia'
import { solicitationService } from '../services/solicitationService'
import { useToast } from 'primevue'
import type { SolicitationHistoryEvent } from '../types'

/**
 * Store para gerenciar estado de histórico de uma solicitação
 */
export const useHistoryStore = defineStore('history', () => {
  const solicitationHistory = ref<SolicitationHistoryEvent[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const toast = useToast()

  /**
   * Busca o histórico de uma solicitação.
   * @param solicitationId O ID da solicitação.
   */
  async function fetchSolicitationHistory(solicitationId: number) {
    if (solicitationHistory.value.length > 0) return

    isLoading.value = true
    try {
      solicitationHistory.value = await solicitationService.getSolicitationHistory(solicitationId)
    } catch (err: any) {
      error.value = err.message || 'Falha ao buscar histórico da solicitação.'
      toast.add({
        severity: 'error',
        summary: 'Erro',
        detail: 'Não foi possível buscar o histórico da solicitação.',
        life: 3000,
      })
      return false
    } finally {
      isLoading.value = false
    }
  }

  function clearHistory() {
    solicitationHistory.value = []
    isLoading.value = false
    error.value = null
  }

  return { solicitationHistory, isLoading, error, fetchSolicitationHistory, clearHistory }
})
