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
    department: solicitation.solicitante?.unidade?.sigla,
    ...(solicitationListType === 'allSolicitations' && {
      requester: solicitation.solicitante.nome,
    }),
  }
}

/**
 * Busca metadados visuais de status de solicitação por ID.
 * @param statusId ID do status.
 * @returns Configuração de status para UI ou `null` quando não encontrado.
 */
export const getSolicitationStatusOptions = (statusId: number) => {
  return SOLICITATION_STATUS.find((status) => status.id === statusId) || null
}
