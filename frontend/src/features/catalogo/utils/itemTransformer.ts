import { toTitleCase } from '@/utils/stringUtils'
import type { Item } from '../types'

/**
 * Normaliza campos textuais de item para padrão de exibição da aplicação.
 * @param itemFromApi Item bruto retornado pelo backend.
 * @returns Item transformado para consumo na UI.
 */
export const transformItem = (itemFromApi: Item): Item => {
  return {
    ...itemFromApi,
    nome: toTitleCase(itemFromApi.nome),
  }
}
