// src/features/management/services/catalogoService.ts

import { apiClient } from '@/service/apiClient'
import type {
  CatalogoParams,
  Item,
  ItemParams,
  PaginatedResponse,
} from '@/features/management/types'

interface ICatalogoService {
  getItens(params?: CatalogoParams): Promise<PaginatedResponse<Item>>
  getItemById(id: number): Promise<Item>
  getItensSemelhantes(id: number): Promise<Item[]>
  editarItem(id: number, params: ItemParams): Promise<Item>
  // createItem(data: Partial<Item>): Promise<AxiosResponse<Item>>; <-- Exemplo para o futuro
}

export const catalogoService: ICatalogoService = {
  /**
   * Busca itens do catálogo de forma paginada e com filtros.
   * @param params Um objeto com os filtros e paginação.
   */
  async getItens(params) {
    const response = await apiClient.get<PaginatedResponse<Item>>('/catalogo', { params })
    return response.data
  },

  /**
   * Busca dados de um item específico do catálogo através de seu ID.
   * @param id ID do item.
   */
  async getItemById(id) {
    const response = await apiClient.get<Item>(`/catalogo/${id}`)
    return response.data
  },

  /**
   * Busca items com mesmo nome do item com ID informado.
   * @param id ID do item.
   */
  async getItensSemelhantes(id) {
    const response = await apiClient.get<Item[]>(`/catalogo/${id}/itens-semelhantes`)
    return response.data
  },

  /**
   * Edita um item.
   * @param id ID do item a editar.
   * @param params Objeto com os dados a serem alterados.
   */
  async editarItem(id, params) {
    const response = await apiClient.put<Item>(`/catalogo/${id}`, params)
    return response.data
  },
}
