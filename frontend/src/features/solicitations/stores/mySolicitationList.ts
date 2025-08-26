import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import type { Solicitation, SolicitationListItem } from '@/features/solicitations'
import { solicitationService } from '../services/solicitationService'
import { transformSolicitation } from '../utils'

/**
 * Store para gerenciar estados da view Detalhes da Solicitação
 */
export const useMySolicitationListStore = defineStore('mySolicitationList', () => {
  const solicitations = ref<SolicitationListItem[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const totalCount = ref<number>(0)
  const pageNumber = ref<number>(1)
  const pageSize = ref<number>(10)
  const totalPages = ref<number>(1)

  /**
   * Busca minhas solicitações.
   */
  const fetchAll = async () => {
    isLoading.value = true
    error.value = null

    try {
      const response = await solicitationService.getMySolicitations()
      solicitations.value = response.data.map(transformSolicitation)
      totalCount.value = response.totalCount
      pageNumber.value = response.pageNumber
      pageSize.value = response.pageSize
      totalPages.value = Math.ceil(response.totalCount / response.pageSize) || 1
    } catch (err: any) {
      error.value = err.message || 'Falha ao carregar minhas solicitações.'
    } finally {
      isLoading.value = false
    }
  }

  const hasNextPage = computed(() => pageNumber.value < totalPages.value)

  const $reset = () => {
    solicitations.value = []
    isLoading.value = false
    error.value = null
    totalCount.value = 0
    pageNumber.value = 1
    pageSize.value = 10
    totalPages.value = 1
  }

  return {
    solicitations,
    isLoading,
    error,
    totalCount,
    totalPages,
    pageNumber,
    pageSize,
    fetchAll,
    hasNextPage,
    $reset,
  }
})
