import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { reportService } from '../services/reportService'
import type {
  CategoryConsumptionResponse,
  CenterExpenseResponse,
  ItemDepartmentResponse,
  ItemsDepartmentFilters,
  ReportDateFilters,
  ReportType,
} from '../types'

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

  const activeReportType = ref<ReportType>(null)
  const centerData = ref<CenterExpenseResponse[]>([])
  const categoryData = ref<CategoryConsumptionResponse[]>([])

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

  /**
   * Gera o relatório baseado no tipo selecionado
   */
  const generateReport = async (type: ReportType, filters: ReportDateFilters) => {
    isLoading.value = true
    error.value = null
    activeReportType.value = type

    // Limpa dados anteriores para garantir que não mostre lixo
    centerData.value = []
    categoryData.value = []

    try {
      if (type === 'GASTOS_CENTRO') {
        centerData.value = await reportService.getCenterExpenses(filters)
      } else if (type === 'CONSUMO_CATEGORIA') {
        categoryData.value = await reportService.getCategoryConsumption(filters)
      }
    } catch (err: any) {
      error.value = err.message || 'Falha ao gerar o relatório.'
      activeReportType.value = null // Reseta se der erro
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
    activeReportType.value = null
    centerData.value = []
    categoryData.value = []
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
    activeReportType, 
    centerData, 
    categoryData,
    fetchItemsPerDepartment,
    generateReport,
    $reset,
  }
})
