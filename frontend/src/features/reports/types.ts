import type { Item } from '@/features/catalogo/types'
import type { UnitOrganizational } from '../unitOrganizational/types'

export type SolicitationItem = Partial<Item> & {
  quantidade: number
  justificativa?: string
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
 * Filtro de parâmetros do Get Itens Solicitados por Departamento
 */
export interface ItemsDepartmentFilters {
  searchTerm: string
  categoriaNome: string
  itemsType: 'geral' | 'patrimonial' | null
  siglaDepartamento: string
  sortOrder: string | null
  pageSize: string
  pageNumber: string
}

export type ItemQuantityPerDepartment = {
  unidade: UnitOrganizational
  quantidadeTotal: number
  justificativa: string
}

export type ItemDepartmentResponse = Partial<Item> & {
  categoriaNome: string
  quantidadeTotalSolicitada: number
  valorTotalSolicitado: number
  precoMedio: number
  precoMinimo: number
  precoMaximo: number
  numeroDeSolicitacoes: number
  demandaPorDepartamento: ItemQuantityPerDepartment[]
}
