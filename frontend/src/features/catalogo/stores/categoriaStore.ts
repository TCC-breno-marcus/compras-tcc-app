import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Categoria, CategoriaParams } from '../types'
import { categoriaService } from '../services/categoriaService'

export const useCategoriaStore = defineStore('categoria', () => {
  const categorias = ref<Categoria[]>([])
  const loading = ref<boolean>(false)
  const error = ref<string | null>(null)

  const categoriasOrdenadas = computed(() => {
    return [...categorias.value].sort((a, b) => a.nome.localeCompare(b.nome))
  })

  /**
   * Busca as categorias de itens na API e atualiza o estado.
   * @param filters Os parâmetros de filtro.
   */
  const fetch = async (filters?: CategoriaParams) => {
    // Categorias já carregadas, pulando a chamada de API.
    if (!filters && categorias.value.length > 0) {
      return
    }

    loading.value = true
    error.value = null

    try {
      const response = await categoriaService.getAll(filters)
      categorias.value = response
    } catch (err) {
      error.value = 'Ocorreu um erro ao buscar as categorias.'
      categorias.value = [] // Limpa em caso de erro para evitar mostrar dados antigos.
      console.error(err)
    } finally {
      loading.value = false
    }
  }

  return {
    categorias: categoriasOrdenadas,
    loading,
    error,
    fetch,
  }
})
