export interface PublicSolicitationItem {
  itemId: number
  itemNome: string
  catMat: string
  categoriaNome: string
  quantidade: number
  valorUnitario: number
  valorTotal: number
  justificativa?: string | null
}

export interface PublicSolicitation {
  id: number
  externalId?: string | null
  dataCriacao: string
  tipoSolicitacao: string
  statusId: number
  statusNome: string
  solicitanteNomeMascarado: string
  solicitanteEmailMascarado: string
  solicitanteTelefoneMascarado: string
  solicitanteCpfMascarado: string
  departamentoNome: string
  departamentoSigla: string
  valorTotalSolicitacao: number
  itens: PublicSolicitationItem[]
}

export interface PublicSolicitationQueryResult {
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
  totalItensSolicitados: number
  valorTotalSolicitado: number
  data: PublicSolicitation[]
}

export interface PublicSolicitationFilters {
  dataInicio?: string
  dataFim?: string
  statusId?: string
  statusNome?: string
  siglaDepartamento?: string
  categoriaNome?: string
  itemNome?: string
  catMat?: string
  itemsType?: 'geral' | 'patrimonial' | ''
  valorMinimo?: number | null
  valorMaximo?: number | null
  somenteSolicitacoesAtivas?: 'true' | 'false' | ''
  pageNumber?: string
  pageSize?: string
}
