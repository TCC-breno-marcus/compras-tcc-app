import { defineStore } from 'pinia'
import { ref } from 'vue'
import { reportService } from '../services/reportService'
import type { ItemDepartmentResponse, ItemsDepartmentFilters } from '../types'

/**
 * Store para gerenciar estados de relatórios
 */
export const useReportStore = defineStore('report', () => {
  const itemsDepartment = ref<ItemDepartmentResponse[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

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
    } catch (err: any) {
      error.value = err.message || 'Falha ao carregar os itens solicitados por departamento.'
    } finally {
      isLoading.value = false
    }
  }

  const $reset = () => {
    itemsDepartment.value = []
    isLoading.value = false
    error.value = null
  }

  return {
    itemsDepartment,
    isLoading,
    error,
    fetchItemsPerDepartment,
    $reset,
  }
})
