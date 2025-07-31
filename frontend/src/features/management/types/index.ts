/**
 * Representa a estrutura de um único item do catálogo,
 * como ele vem da API.
 */
export interface Item {
  id: number
  nome: string
  catMat: string
  descricao: string
  especificacao: string
  categoria: Categoria
  linkImagem: string
  precoSugerido: number
  isActive: boolean
}

/**
 * Define todos os campos possíveis para editar um item.
 * Todas as propriedades são opcionais.
 */
export interface ItemParams {
  nome?: string
  catMat?: string
  descricao?: string
  especificacao?: string
  categoriaId?: number
  linkImagem?: string
  precoSugerido?: number
  isActive?: boolean
}

/**
 * Define todos os parâmetros possíveis para filtrar e paginar o catálogo.
 * Todas as propriedades são opcionais.
 */
export interface CatalogoParams {
  id?: number
  nome?: string
  catMat?: string
  descricao?: string
  especificacao?: string
  categoriaId?: string
  isActive?: boolean
  pageNumber?: number
  pageSize?: number
  sortOrder?: string
  search?: string
}

/**
 * Representa a estrutura padrão de uma resposta paginada da sua API.
 * É genérica para poder ser reutilizada com outros tipos de dados (ex: Solicitações).
 */
export interface PaginatedResponse<T> {
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
  items: T[]
}

/**
 * Representa a estrutura de uma categoria de item do catálogo.
 */
export interface Categoria {
  id: number
  nome: string
  descricao: string
  isActive: boolean
}

/**
 * Define todos os parâmetros possíveis para filtrar as categorias.
 * Todas as propriedades são opcionais.
 */
export interface CategoriaParams {
  id?: number
  nome?: string
  descricao?: string
  isActive?: boolean
}
