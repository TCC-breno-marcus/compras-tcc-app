import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { UnitOrganizational } from '../types'
import { unitOrganizationalService } from '../services/unitOrganizationalService'

/**
 * Store para cache de centros e departamentos organizacionais.
 */
export const useUnitOrganizationalStore = defineStore(
  'unitOrganizational',
  () => {
    const centers = ref<UnitOrganizational[]>([])
    const departments = ref<UnitOrganizational[]>([])

    /**
     * Busca departamentos na API e atualiza estado local.
     */
    const fetchDepartments = async () => {
      try {
        const data = await unitOrganizationalService.getDepartments()
        departments.value = data
      } catch (error) {
        console.error('Falha na ação de fetchDepartments da store:', error)
      }
    }

    /**
     * Busca centros na API e atualiza estado local.
     */
    const fetchCenters = async () => {
      try {
        const data = await unitOrganizationalService.getCenters()
        centers.value = data
      } catch (error) {
        console.error('Falha na ação de fetchCenters da store:', error)
      }
    }

    return {
      centers,
      departments,
      fetchCenters,
      fetchDepartments,
    }
  },
  {
    persist: true,
  },
)
