import { apiClient } from '@/services/apiClient'
import type { Categoria, CategoriaParams } from '@/features/management/types'
import { transformCategory } from '../utils/categoriaTransformer'

interface ICategoriaService {
  getAll(params?: CategoriaParams): Promise<Categoria[]>
  getById(id: number): Promise<Categoria>
}

export const categoriaService: ICategoriaService = {
  /**
   * Busca categorias dos itens com possíveis filtros.
   * @param params Um objeto com os filtros.
   */
  async getAll(params) {
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
