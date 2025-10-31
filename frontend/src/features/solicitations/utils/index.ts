import { SOLICITATION_STATUS } from '../constants'
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

  const typeDisplay = solicitation.justificativaGeral ? 'Geral' : 'Patrimonial'

  return {
    ...solicitation,
    typeDisplay,
    ...(solicitationListType === 'allSolicitations' && {
      requester: solicitation.solicitante.nome,
      department: solicitation.solicitante.unidade.sigla,
    }),
  }
}

export const getSolicitationStatusOptions = (statusId: number) => {
  return SOLICITATION_STATUS.find((status) => status.id === statusId) || null
}
