// src/features/management/stores/catalogoStore.ts

import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { catalogoService } from '../services/catalogoService';
import type { Item, CatalogoFilters } from '../types';

export const useCatalogoStore = defineStore('catalogo', () => {
  const items = ref<Item[]>([]);
  const totalCount = ref<number>(0);
  const pageNumber = ref<number>(1);
  const pageSize = ref<number>(50);
  const totalPages = ref<number>(1);
  const loading = ref<boolean>(false);
  const error = ref<string | null>(null);

  /**
   * Busca os itens do catálogo na API e atualiza o estado.
   * @param filters Os parâmetros de filtro e paginação.
   */
  async function fetchItems(filters?: CatalogoFilters) {
    loading.value = true; 
    error.value = null;

    try {
      const response = await catalogoService.getItens(filters);
      
      items.value = response.items; 
      totalCount.value = response.totalCount;
      pageNumber.value = response.pageNumber;
      pageSize.value = response.pageSize;
      totalPages.value = Math.ceil(response.totalCount / response.pageSize) || 1;

    } catch (err) {
      error.value = 'Ocorreu um erro ao buscar os itens.';
      items.value = []; // Limpa em caso de erro para evitar mostrar dados antigos.
      console.error(err);
    } finally {
      loading.value = false;
    }
  }

  const hasNextPage = computed(() => pageNumber.value < totalPages.value);

  return {
    items,
    totalCount,
    pageNumber,
    pageSize,
    totalPages,
    loading,
    error,
    fetchItems,
    hasNextPage
  };
});