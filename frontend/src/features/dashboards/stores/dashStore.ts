import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type { DashboardResponse } from '../types'
import { dashService } from '../services/dashService'

export const useDashboardStore = defineStore('gestorDashboard', () => {
  const dashboardData = ref<DashboardResponse | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const kpis = computed(() => dashboardData.value?.kpis)
  const valorPorDepartamento = computed(() => dashboardData.value?.valorPorDepartamento)
  const valorPorCategoria = computed(() => dashboardData.value?.valorPorCategoria)
  const visaoGeralStatus = computed(() => dashboardData.value?.visaoGeralStatus)
  const topItensPorQuantidade = computed(() => dashboardData.value?.topItensPorQuantidade)
  const topItensPorValorTotal = computed(() => dashboardData.value?.topItensPorValorTotal)

  /**
   * Função para buscar os dados do dashboard na API.
   */
  async function fetchGestorDashboard() {
    if (dashboardData.value) return

    isLoading.value = true
    error.value = null
    try {
      dashboardData.value = await dashService.getDashboards()
    } catch (err: any) {
      error.value = err.message
    } finally {
      isLoading.value = false
    }
  }

  return {
    isLoading,
    error,
    kpis,
    valorPorDepartamento,
    valorPorCategoria,
    visaoGeralStatus,
    topItensPorQuantidade,
    topItensPorValorTotal,
    fetchGestorDashboard,
  }
})
