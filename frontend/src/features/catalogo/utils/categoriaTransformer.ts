import { toTitleCase } from '@/utils/stringUtils'
import type { Categoria } from '../types'

// Esta função recebe uma categoria da API e a retorna no formato padronizado
export function transformCategory(categoryFromApi: Categoria): Categoria {
  return {
    ...categoryFromApi,
    nome: toTitleCase(categoryFromApi.nome),
  }
}
