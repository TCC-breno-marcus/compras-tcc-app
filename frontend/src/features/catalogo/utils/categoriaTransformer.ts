import { toTitleCase } from '@/utils/stringUtils'
import type { Categoria } from '../types'

// Esta função recebe uma categoria da API e a retorna no formato padronizado
export const transformCategory = (categoryFromApi: Categoria): Categoria => {
  return {
    ...categoryFromApi,
    nome: toTitleCase(categoryFromApi.nome),
  }
}

/**
 * Filtra um array de categorias, retornando apenas os IDs daquelas cujo nome
 * está presente na lista de nomes fornecida.
 *
 * @param categorys O array de objetos Categoria a ser filtrado.
 * @param namesFilter Um array de strings com os nomes desejados.
 * @returns Um novo array contendo apenas as categorias que correspondem aos nomes.
 */
export const categorysIdFilterPerName = (categorys: Categoria[], namesFilter: string[]): number[] => {
  if (!namesFilter || namesFilter.length === 0) {
    return []
  }
  const nomesSet = new Set(namesFilter.map((name) => name.toLowerCase()))
  const categorysFilter = categorys.filter((c) => nomesSet.has(c.nome.toLowerCase()))
  return categorysFilter.map((c) => c.id)
}
