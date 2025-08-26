import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import type { Solicitation } from '@/features/solicitations'
import { solicitationService } from '../services/solicitationService'
import { dataHasBeenChanged } from '@/utils/objectUtils'
import type { Item } from '@/features/catalogo/types'

/**
 * Store para gerenciar estados da view Detalhes da Solicitação
 */
export const useSolicitationStore = defineStore('solicitation', () => {
  const currentSolicitation = ref<Solicitation | null>(null)
  const currentSolicitationBackup = ref<Solicitation | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  /**
   * Busca uma solicitação específica por ID na API.
   * @param id O ID da solicitação a ser buscada.
   */
  const fetchById = async (id: number) => {
    isLoading.value = true
    error.value = null
    currentSolicitation.value = null

    try {
      const response = await solicitationService.getById(id)
      currentSolicitation.value = response
      currentSolicitationBackup.value = JSON.parse(JSON.stringify(response))
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
  const update = async (payload: Partial<Solicitation>) => {
    // ... lógica para chamar o serviço de update
  }

  /**
   * Adiciona um item à solicitação atual
   * @param item
   * @param type
   * @returns
   */
  const addItem = (item: Item) => {
    const itemExistente = currentSolicitation.value?.itens.find((i) => i.id === item.id)

    if (itemExistente) {
      itemExistente.quantidade++
      return 'incremented'
    } else {
      currentSolicitation.value?.itens.push({ ...item, quantidade: 1 })
      return 'added'
    }
  }

  const removeItem = (itemId: number) => {
    if (currentSolicitation.value) {
      currentSolicitation.value.itens = currentSolicitation.value.itens.filter(
        (i) => i.id !== itemId,
      )
      return 'removed'
    }
  }

  const updateItemQuantity = (itemId: number, newQuantity: number) => {
    const item = currentSolicitation.value?.itens.find((i) => i.id === itemId)
    if (item) {
      item.quantidade = newQuantity
    }
  }

  const isDirty = computed(() => {
    if (!currentSolicitation.value || !currentSolicitationBackup.value) {
      return false
    }
    const is = dataHasBeenChanged<Solicitation>(
      currentSolicitationBackup.value,
      currentSolicitation.value,
    )
    return is
  })

  const $reset = () => {
    currentSolicitation.value = null
    currentSolicitationBackup.value = null
    isLoading.value = false
    error.value = null
  }

  return {
    currentSolicitation,
    isLoading,
    error,
    fetchById,
    update,
    addItem,
    removeItem,
    updateItemQuantity,
    isDirty,
    $reset,
  }
})
