/**
 * Representa os dados da unidade organizacional
 */
export interface UnitOrganizational {
  id: number
  nome: string
  sigla: string
  email: string
  telefone: string
  tipo: string
}

/**
 * Filtros do getDepartaments
 */
export interface departamentsFilters {
  nome: string
  sigla: string
  siglaCentro: string
}

/**
 * Filtros do getCenters
 */
export interface centersFilters {
  nome: string
  sigla: string
}
