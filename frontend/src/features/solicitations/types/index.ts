import type { Item } from '@/features/catalogo/types' // Importe o tipo base

export interface SolicitationItem extends Item {
  quantity: number
  justification?: string
}

/**
 * Representa a estrutura de uma solicitação
 */
export interface Solicitation {
  id: number
  solicitante: string
  descricao: string
  type: string
  status: boolean
}

/**
 * Representa a estrutura do contexto (estados) das páginas de solicitação.
 */
export interface SolicitationContext {
  dialogMode: string // 'management' or 'selection'
  isGeneral: boolean
}
