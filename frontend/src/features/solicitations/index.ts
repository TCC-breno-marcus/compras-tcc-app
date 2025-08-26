import type { Item } from '@/features/catalogo/types'

export type SolicitationItem = Partial<Item> & {
  quantidade: number
  justificativa?: string
}

/**
 * Representa um único item dentro de uma solicitação a ser enviada para a API.
 */
export interface CreateItemSolicitationPayload {
  itemId: number
  quantidade: number
  valorUnitario: number
  justificativa?: string
}

/**
 * Payload para criar uma solicitação do tipo Geral.
 */
export interface CreateSolicitationGeneralPayload {
  type: 'geral'
  justificativaGeral: string
  itens: CreateItemSolicitationPayload[]
}

/**
 * Payload para criar uma solicitação do tipo Patrimonial.
 */
export interface CreateSolicitationPatrimonialPayload {
  type: 'patrimonial'
  itens: CreateItemSolicitationPayload[]
}

export type CreateSolicitationPayload =
  | CreateSolicitationGeneralPayload
  | CreateSolicitationPatrimonialPayload

/**
 * Representa os dados de um solicitante no retorno de uma solicitação
 */
export interface Solicitante {
  id: string
  nome: string
  email: string
  departamento: string
}

/**
 * Representa um item no retorno de uma solicitação
 */
export interface ItemSolicitationResponse {
  id: number
  nome: string
  catMat: string
  linkImagem: string
  quantidade: number
  precoSugerido: number
  justificativa: string
}

/**
 * Retorno de uma solicitação.
 */
export interface SolicitationResult {
  id: number
  dataCriacao: string
  justificativaGeral: string
  externalId: string
  solicitante: Solicitante
  itens: SolicitationItem[]
}

/**
 * Representa uma solicitação no front.
 */
export interface Solicitation {
  id: number
  dataCriacao: string
  justificativaGeral: string
  externalId: string
  solicitante: Solicitante
  itens: SolicitationItem[]
}

/**
 * Representa uma solicitação exibida na tabela de listar solicitações
 */
export interface SolicitationListItem extends Solicitation {
  itemsCount: number
  totalItemsQuantity: number
  totalEstimatedPrice: number
  typeDisplay: 'Geral' | 'Patrimonial'
}

/**
 *
 */
export interface MySolicitationFilters {
  externalId: string
  tipo: 'Geral' | 'Patrimonial' | ''
  dataInicial: Date | null
  dataFinal: Date | null
  sortOrder: string | null
}
