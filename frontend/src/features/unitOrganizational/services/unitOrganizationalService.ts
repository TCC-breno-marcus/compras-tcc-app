import { apiClient } from '@/services/apiClient'
import axios from 'axios'
import type { centersFilters, departamentsFilters, UnitOrganizational } from '../types'

interface IUnitOrganizationalService {
  getCenters(filters?: centersFilters): Promise<UnitOrganizational[]>
  getDepartments(filters?: departamentsFilters): Promise<UnitOrganizational[]>
}

export const unitOrganizationalService: IUnitOrganizationalService = {
  async getCenters() {
    try {
      const response = await apiClient.get('/centro')
      return response.data
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        throw new Error(error.response.data.message)
      }
      throw new Error('Erro de conexão com a API ao tentar buscar centros.')
    }
  },

  async getDepartments() {
    try {
      const response = await apiClient.get('/departamento')
      return response.data
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        throw new Error(error.response.data.message)
      }
      throw new Error('Erro de conexão com a API ao tentar buscar departamentos.')
    }
  },
}
