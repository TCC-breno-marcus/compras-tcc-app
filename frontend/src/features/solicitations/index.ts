import type { Item } from '@/features/catalogo/types' // Importe o tipo base

export interface SolicitationItem extends Item {
  quantity: number
  justification?: string
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
  itemId: number
  nomeDoItem: number
  catMat: string
  quantidade: number
  valorUnitario: number
  justificativa: string
}

/**
 * Retorno de uma solicitação.
 */
export interface SolicitationResult {
  id: string
  dataCriacao: string
  justificativaGeral: string
  solicitante: Solicitante
  itens: ItemSolicitationResponse[]
}
