import { toTitleCase } from '@/utils/stringUtils'
import type { Item } from '../types'

// Esta função recebe um item da API e o retorna no formato padronizado
export function transformItem(itemFromApi: Item): Item {
  return {
    ...itemFromApi,
    nome: toTitleCase(itemFromApi.nome),
  }
}
