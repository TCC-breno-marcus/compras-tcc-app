import type { Item } from '@/features/catalogo/types'
import type { UnitOrganizational } from '../unitOrganizational/types'

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
  unidade: UnitOrganizational
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
 * Representa um objeto de status da solicitação
 */
export interface SolicitationStatus {
  id: number
  nome: string
  descricao: string
}

/**
 * Representa o body da requisição de atualizar status de uma solicitação
 */
export interface SolicitationStatusPayload {
  novoStatusId: number
  observacoes: string
}

/**
 * Representa uma solicitação no front.
 */
export interface Solicitation {
  id: number
  dataCriacao: string
  justificativaGeral: string
  externalId: string
  status: SolicitationStatus
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
  requester?: string
  department?: string
}

/**
 * Filtros de parâmetros do Get My Solicitations
 */
export interface MySolicitationFilters {
  externalId: string
  tipo: 'Geral' | 'Patrimonial' | ''
  dateRange: (Date | null)[] | Date | null
  sortOrder: string | null
  pageSize: string
  pageNumber: string
  statusIds: number[]
}

/**
 * Filtro de parâmetros do Get Solicitations
 */
export interface SolicitationFilters {
  externalId: string
  tipo: 'Geral' | 'Patrimonial' | ''
  dateRange: (Date | null)[] | Date | null
  sortOrder: string | null
  pageSize: string
  pageNumber: string
  statusIds: number[]
  pessoaId: string
  siglaDepartamento: string
}

/**
 * Representa um objeto da lista de históricos de uma solicitação
 */
export interface SolicitationHistoryEvent {
  id: string
  dataOcorrencia: string
  acao: string
  detalhes: string | null
  observacoes: string | null
  nomePessoa: string
}
