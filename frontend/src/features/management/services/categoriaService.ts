import { apiClient } from '@/services/apiClient'
import type { Categoria, CategoriaParams } from '@/features/management/types'

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
    return response.data
  },

  /**
   * Busca dados de uma categoria de item específica através de seu ID.
   * @param id ID da categoria.
   */
  async getById(id) {
    const response = await apiClient.get<Categoria>(`/categoria/${id}`)
    return response.data
  },
}
