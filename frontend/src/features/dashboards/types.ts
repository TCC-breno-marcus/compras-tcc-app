// Interface para os cartões de KPI
export interface KpiData {
  valorTotalEstimado: number
  custoMedioSolicitacao: number
  totalItensUnicos: number
  totalUnidadesSolicitadas: number
  totalDepartamentosSolicitantes: number
  totalSolicitacoes: number
}

// Interface genérica para os dados de gráfico (Pizza, Barras)
export interface ChartData {
  labels: string[]
  data: number[]
}

// Interface para os dados de Top Itens
export interface TopItemData {
  itemId: number
  nome: string
  catMat: string
  valor: number
}

// A interface principal que o endpoint da API retorna
export interface DashboardResponse {
  kpis: KpiData
  valorPorDepartamento: ChartData
  valorPorCategoria: ChartData
  visaoGeralStatus: ChartData
  topItensPorQuantidade: TopItemData[]
  topItensPorValorTotal: TopItemData[]
}

// Interface do card de cada KPI
export interface CardKPI {
  title: string
  value: string
  icon: string
  color: string
}
