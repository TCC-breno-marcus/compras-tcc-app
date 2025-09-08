import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { reportService } from '../services/reportService'
import type { ItemDepartmentResponse, ItemsDepartmentFilters } from '../types'

/**
 * Store para gerenciar estados de relatórios
 */
export const useReportStore = defineStore('report', () => {
  const itemsDepartment = ref<ItemDepartmentResponse[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const totalCount = ref<number>(0)
  const pageNumber = ref<number>(1)
  const pageSize = ref<number>(10)
  const totalPages = ref<number>(1)
  
  /**
   * Busca os itens solicitados organizados por departamento.
   * @param filters Os possíveis filtros.
   */
  const fetchItemsPerDepartment = async (filters: ItemsDepartmentFilters) => {
    isLoading.value = true
    error.value = null

    try {
      const response = await reportService.getItemsPerDepartment(filters)
      itemsDepartment.value = response.data
      totalCount.value = response.totalCount
      pageNumber.value = response.pageNumber
      pageSize.value = response.pageSize
      totalPages.value = Math.ceil(response.totalCount / response.pageSize) || 1
    } catch (err: any) {
      error.value = err.message || 'Falha ao carregar os itens solicitados por departamento.'
    } finally {
      isLoading.value = false
    }
  }

  const hasNextPage = computed(() => pageNumber.value < totalPages.value)

  const $reset = () => {
    itemsDepartment.value = []
    isLoading.value = false
    error.value = null
    totalCount.value = 0
    pageNumber.value = 1
    pageSize.value = 10
    totalPages.value = 1
  }

  return {
    itemsDepartment,
    isLoading,
    error,
    totalCount,
    totalPages,
    pageNumber,
    pageSize,
    hasNextPage,
    fetchItemsPerDepartment,
    $reset,
  }
})
