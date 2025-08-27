/**
 * Representa a estrutura padrão de uma resposta paginada da API.
 * É genérica para poder ser reutilizada com vários tipos de dados.
 */
export interface PaginatedResponse<T> {
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
  data: T[]
}