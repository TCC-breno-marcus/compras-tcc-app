import { defineStore, storeToRefs } from 'pinia'
import { computed, ref } from 'vue'
import type { Solicitation } from '@/features/solicitations/types'
import { solicitationService } from '../services/solicitationService'
import { dataHasBeenChanged } from '@/utils/objectUtils'
import type { Item } from '@/features/catalogo/types'
import { useToast } from 'primevue'
import { useSettingStore } from '@/features/settings/stores/settingStore'

/**
 * Store para gerenciar estados da view Detalhes da Solicitação
 */
export const useSolicitationStore = defineStore('solicitation', () => {
  const currentSolicitation = ref<Solicitation | null>(null)
  const currentSolicitationBackup = ref<Solicitation | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const toast = useToast()

  const settingsStore = useSettingStore()
  const { settings } = storeToRefs(settingsStore)

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
   * @param payload Os novos dados da solicitação.
   */
  const update = async (payload: Solicitation) => {
    isLoading.value = true
    error.value = null

    const newData = {
      justificativaGeral: payload.justificativaGeral,
      itens: payload.itens.map((item) => {
        return {
          itemId: item.id,
          quantidade: item.quantidade,
          valorUnitario: item.precoSugerido,
          justificativa: item.justificativa,
        }
      }),
    }

    try {
      const response = await solicitationService.update(payload.id, newData)
      currentSolicitation.value = response
      currentSolicitationBackup.value = JSON.parse(JSON.stringify(response))
      toast.add({
        severity: 'success',
        summary: 'Sucesso',
        detail: 'A solicitação foi salva com sucesso.',
        life: 3000,
      })
      return true
    } catch (err: any) {
      error.value = err.message || 'Falha ao carregar a solicitação.'
      toast.add({
        severity: 'error',
        summary: 'Erro',
        detail: 'Não foi possível salvar as alterações.',
        life: 3000,
      })
      return false
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Adiciona um item à solicitação atual
   * @param item
   * @returns
   */
  const addItem = (item: Item) => {
    const itemExistente = currentSolicitation.value?.itens.find((i) => i.id === item.id)

    if (itemExistente) {
      const maxQuantity = settings.value?.maxQuantidadePorItem
      if (maxQuantity && itemExistente.quantidade >= maxQuantity) {
        return 'quantity_limit_exceeded'
      }
      itemExistente.quantidade++
      return 'incremented'
    } else {
      const maxItens = settings.value?.maxItensDiferentesPorSolicitacao
      const currentQttItens = currentSolicitation.value?.itens?.length || 0
      if (maxItens && currentQttItens >= maxItens) {
        return 'item_limit_exceeded'
      }
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
    currentSolicitationBackup,
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
