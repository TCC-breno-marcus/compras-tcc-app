import type { Solicitation, SolicitationListItem } from '../types'

/**
 * Converte um objeto Solicitation da API para o formato de exibição da lista,
 * calculando os campos derivados.
 */
export const transformSolicitation = (
  solicitation: Solicitation,
  solicitationListType: 'allSolicitations' | 'mySolicitations',
): SolicitationListItem => {
  const items = solicitation.itens || []

  const itemsCount = items.length

  const totalItemsQuantity = items.reduce((total, item) => {
    return total + (item.quantidade || 0)
  }, 0)

  const totalEstimatedPrice = items.reduce((total, item) => {
    return total + (item.quantidade || 0) * (item.precoSugerido || 0)
  }, 0)

  const typeDisplay = solicitation.justificativaGeral ? 'Geral' : 'Patrimonial'

  return {
    ...solicitation,
    itemsCount,
    totalItemsQuantity,
    totalEstimatedPrice,
    typeDisplay,
    ...(solicitationListType === 'allSolicitations' && {
      requester: solicitation.solicitante.nome,
      department: solicitation.solicitante.unidade.sigla,
    }),
  }
}
