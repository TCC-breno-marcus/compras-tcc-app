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
 * Define todos os filtros do formulário filtrar o catálogo.
 * Todas as propriedades são opcionais.
 */
export interface CatalogoFilters {
  searchTerm: string
  nome: string
  descricao: string
  catMat: string
  especificacao: string
  categoriaId: number[]
  status: string
  sortOrder: 'asc' | 'desc' | null
  pageNumber?: string
  pageSize?: string
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
  id?: number[]
  nome?: string[]
  descricao?: string
  isActive?: boolean
}

/**
 * Define o objeto do formulário de um item.
 */
export interface ItemFormData {
  nome: string
  descricao: string
  catMat: string
  especificacao: string
  categoriaId: number | null
  precoSugerido: number
  isActive: boolean
}

/**
 * Representa um objeto da lista de históricos de um item
 */
export interface ItemHistoryEvent {
  id: string
  dataOcorrencia: string
  acao: string
  detalhes: string | null
  observacoes: string | null
  nomePessoa: string
}

export interface DeleteItemResponse {
  message: string
}