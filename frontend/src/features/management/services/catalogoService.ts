// src/features/management/services/catalogoService.ts

import { apiClient } from '@/service/apiClient';
import type { CatalogoParams, Item, PaginatedResponse } from '@/features/management/types';
import type { AxiosResponse } from 'axios';

interface ICatalogoService {
  getItens(params?: CatalogoParams): Promise<AxiosResponse<PaginatedResponse<Item>>>;
  // getItemById(id: number): Promise<AxiosResponse<Item>>; <-- Exemplo para o futuro
  // createItem(data: Partial<Item>): Promise<AxiosResponse<Item>>; <-- Exemplo para o futuro
}

export const catalogoService: ICatalogoService = {
  /**
   * Busca itens do catálogo de forma paginada e com filtros.
   * @param params Um objeto com os filtros e paginação.
   */
  getItens(params) {
    return apiClient.get('/catalogo', { params });
  }
};