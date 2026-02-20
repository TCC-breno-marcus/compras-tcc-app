import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { publicDataService } from '@/features/publicData/services/publicDataService'
import type {
  PublicSolicitation,
  PublicSolicitationFilters,
  PublicSolicitationQueryResult,
} from '@/features/publicData/types'

export const usePublicDataStore = defineStore('publicData', () => {
  const solicitations = ref<PublicSolicitation[]>([])
  const totalCount = ref(0)
  const totalPages = ref(1)
  const pageNumber = ref(1)
  const pageSize = ref(25)
  const totalItemsRequested = ref(0)
  const totalAmountRequested = ref(0)

  const isLoading = ref(false)
  const isExporting = ref(false)
  const error = ref<string | null>(null)

  const hasNextPage = computed(() => pageNumber.value < totalPages.value)

  const fetchPublicSolicitations = async (filters: PublicSolicitationFilters) => {
    isLoading.value = true
    error.value = null

    try {
      const response: PublicSolicitationQueryResult = await publicDataService.getSolicitations(filters)

      solicitations.value = response.data
      totalCount.value = response.totalCount
      totalPages.value = response.totalPages
      pageNumber.value = response.pageNumber
      pageSize.value = response.pageSize
      totalItemsRequested.value = response.totalItensSolicitados
      totalAmountRequested.value = response.valorTotalSolicitado
    } catch (err: any) {
      error.value = err?.message || 'Falha ao consultar os dados públicos.'
      solicitations.value = []
      totalCount.value = 0
      totalPages.value = 1
    } finally {
      isLoading.value = false
    }
  }

  const exportPublicSolicitations = async (
    filters: PublicSolicitationFilters,
    format: 'csv' | 'json',
  ) => {
    isExporting.value = true
    error.value = null

    try {
      return await publicDataService.exportSolicitations(filters, format)
    } catch (err: any) {
      error.value = err?.message || `Falha ao exportar dados públicos em ${format.toUpperCase()}.`
      throw err
    } finally {
      isExporting.value = false
    }
  }

  return {
    solicitations,
    totalCount,
    totalPages,
    pageNumber,
    pageSize,
    totalItemsRequested,
    totalAmountRequested,
    isLoading,
    isExporting,
    error,
    hasNextPage,
    fetchPublicSolicitations,
    exportPublicSolicitations,
  }
})
