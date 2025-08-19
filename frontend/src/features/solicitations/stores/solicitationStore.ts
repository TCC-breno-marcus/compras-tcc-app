import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Solicitation } from '@/features/solicitations'
import { solicitationService } from '../services/solicitationService'

export const useSolicitationStore = defineStore('solicitation', () => {
  const currentSolicitation = ref<Solicitation | null>(null)
  const currentSolicitationBackup = ref<Solicitation | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  /**
   * Busca uma solicitação específica por ID na API.
   * @param id O ID da solicitação a ser buscada.
   */
  async function fetchById(id: number) {
    isLoading.value = true
    error.value = null
    currentSolicitation.value = null
    currentSolicitationBackup.value = null

    try {
      const response = await solicitationService.getById(id)
      currentSolicitation.value = response
      currentSolicitationBackup.value = response
    } catch (err: any) {
      error.value = err.message || 'Falha ao carregar a solicitação.'
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Salva as alterações de uma solicitação no backend.
   * @param payload Os dados atualizados da solicitação.
   */
  async function update(payload: Partial<Solicitation>) {
    // ... lógica para chamar o serviço de update
  }

  function removeItem(itemId: number) {
    currentSolicitation.value?.itens.filter((i) => i.id !== itemId)
    return 'removed'
  }

  function updateItemQuantity(itemId: number, newQuantity: number) {
    const item = currentSolicitation.value?.itens.find((i) => i.id === itemId)
    if (item) {
      item.quantidade = newQuantity
    }
  }

  return {
    currentSolicitation,
    isLoading,
    error,
    fetchById,
    update,
    removeItem,
    updateItemQuantity,
  }
})
