import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { UnitOrganizational } from '../types'
import { unitOrganizationalService } from '../services/unitOrganizationalService'

export const useUnitOrganizationalStore = defineStore(
  'unitOrganizational',
  () => {
    const centers = ref<UnitOrganizational[]>([])
    const departments = ref<UnitOrganizational[]>([])

    const fetchDepartments = async () => {
      try {
        const data = await unitOrganizationalService.getDepartments()
        departments.value = data
      } catch (error) {
        console.error('Falha na ação de fetchDepartments da store:', error)
      }
    }

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
