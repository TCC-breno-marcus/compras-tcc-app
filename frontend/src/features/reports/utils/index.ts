import type { ItemDepartmentResponse } from '../types'

/**
 * Arredonda um número para duas casas decimais
 */
const roundToTwoDecimals = (value: number): number => {
  return Math.round((value + Number.EPSILON) * 100) / 100
}

/**
 * Converte um array de ItemDepartmentResponse da API para o formato de exibição em tabela,
 * arredondando os campos de preço para duas casas decimais.
 */
export const transformItemDepartment = (
  items: ItemDepartmentResponse[],
): ItemDepartmentResponse[] => {
  return items.map((item) => ({
    ...item,
    valorTotalSolicitado: roundToTwoDecimals(item.valorTotalSolicitado || 0),
    precoMedio: roundToTwoDecimals(item.precoMedio || 0),
    precoMinimo: roundToTwoDecimals(item.precoMinimo || 0),
    precoMaximo: roundToTwoDecimals(item.precoMaximo || 0),
  }))
}

/**
 * Versão alternativa para transformar apenas um item
 */
export const transformSingleItemDepartment = (
  item: ItemDepartmentResponse,
): ItemDepartmentResponse => {
  return {
    ...item,
    valorTotalSolicitado: roundToTwoDecimals(item.valorTotalSolicitado || 0),
    precoMedio: roundToTwoDecimals(item.precoMedio || 0),
    precoMinimo: roundToTwoDecimals(item.precoMinimo || 0),
    precoMaximo: roundToTwoDecimals(item.precoMaximo || 0),
  }
}
