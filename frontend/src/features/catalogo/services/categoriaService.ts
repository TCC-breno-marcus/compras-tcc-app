import { apiClient } from '@/services/apiClient'
import type { Categoria, CategoriaParams } from '../types'
import { transformCategory } from '../utils/categoriaTransformer'

interface ICategoriaService {
  getAll(filters?: CategoriaParams): Promise<Categoria[]>
  getById(id: number): Promise<Categoria>
}

export const categoriaService: ICategoriaService = {
  /**
   * Busca categorias dos itens com possíveis filtros.
   * @param filters Um objeto com os filtros.
   */
  async getAll(filters) {
    const params = new URLSearchParams()

    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (key === 'nomes' && Array.isArray(value) && value.length > 0) {
          value.forEach((id) => params.append(key, id.toString()))
        } else if (value !== null && value !== undefined && value !== '') {
          params.set(key, String(value))
        }
      })
    }

    const response = await apiClient.get<Categoria[]>('/categoria', { params })
    response.data = response.data.map(transformCategory)
    return response.data
  },

  /**
   * Busca dados de uma categoria de item específica através de seu ID.
   * @param id ID da categoria.
   */
  async getById(id) {
    const response = await apiClient.get<Categoria>(`/categoria/${id}`)
    const categoryTransformed = transformCategory(response.data)
    return categoryTransformed
  },
}
