type SolicitationType = 'geral' | 'patrimonial'

type MockSolicitationOptions = {
  id?: number
  type?: SolicitationType
  statusId?: number
  statusNome?: string
  requesterSigla: string
  requesterName?: string
  justificativaGeral?: string
  itens?: Array<{
    id: number
    nome: string
    catMat: string
    quantidade: number
    precoSugerido: number
    justificativa?: string
  }>
}

const statusDescriptionByName: Record<string, string> = {
  Pendente: 'Solicitação aguardando análise do gestor.',
  'Aguardando Ajustes': 'Devolvida ao solicitante para correção ou mais informações.',
  Aprovada: 'A solicitação foi aceita pelo gestor e seguirá para o próximo fluxo.',
  Rejeitada: 'O pedido foi permanentemente negado pelo gestor.',
  Cancelada: 'Encerrada antecipadamente pelo solicitante ou gestor.',
  Encerrada: 'Estado de arquivamento para solicitações de ciclos anteriores.',
}

const getUserSigla = () =>
  cy.window().then((win) => {
    const auth = JSON.parse(win.localStorage.getItem('auth') || '{}')
    return auth?.user?.unidade?.sigla || 'DCOMP'
  })

const mockSettings = ({
  prazoSubmissao = '2099-12-31T23:59:59.000Z',
  maxQuantidadePorItem = 10,
  maxItensDiferentesPorSolicitacao = 10,
} = {}) => {
  cy.intercept('GET', '**/api/configuracao', {
    statusCode: 200,
    body: {
      prazoSubmissao,
      maxQuantidadePorItem,
      maxItensDiferentesPorSolicitacao,
      emailContatoPrincipal: 'contato@sistema.com',
      emailParaNotificacoes: 'notificacoes@sistema.com',
    },
  }).as('getSettings')
}

const buildSolicitation = ({
  id = 101,
  type = 'geral',
  statusId = 1,
  statusNome = 'Pendente',
  requesterSigla,
  requesterName = 'Solicitante Padrão',
  justificativaGeral = 'Justificativa inicial',
  itens,
}: MockSolicitationOptions) => {
  const baseItens = [
    {
      id: 201,
      nome: 'Item Teste A',
      catMat: 'CAT-001',
      linkImagem: '',
      quantidade: 1,
      precoSugerido: 100,
      justificativa: type === 'patrimonial' ? 'Justificativa inicial item A' : undefined,
    },
    {
      id: 202,
      nome: 'Item Teste B',
      catMat: 'CAT-002',
      linkImagem: '',
      quantidade: 1,
      precoSugerido: 50,
      justificativa: type === 'patrimonial' ? 'Justificativa inicial item B' : undefined,
    },
  ]

  return {
    id,
    dataCriacao: '2026-01-10T10:00:00.000Z',
    externalId: `SOL-${id}`,
    justificativaGeral: type === 'geral' ? justificativaGeral : '',
    status: {
      id: statusId,
      nome: statusNome,
      descricao: statusDescriptionByName[statusNome] || 'Status de teste',
    },
    solicitante: {
      id: '1',
      nome: requesterName,
      email: 'solicitante@sistema.com',
      unidade: {
        id: 1,
        nome: 'Departamento de Computação',
        sigla: requesterSigla,
        email: 'dcomp@ufs.br',
        telefone: '79999999999',
        tipo: 'Departamento',
      },
    },
    itens: (itens || baseItens).map((item) => ({
      ...item,
      linkImagem: '',
    })),
    kpis: {
      valorTotalEstimado: 150,
      totalItensUnicos: 2,
      totalUnidades: 2,
    },
    valorPorCategoria: {
      labels: ['Diversos'],
      data: [150],
    },
    topItensPorValor: [
      {
        nome: 'Item Teste A',
        catMat: 'CAT-001',
        valor: 100,
      },
    ],
  }
}

const mockDetailsPage = (solicitation: ReturnType<typeof buildSolicitation>) => {
  cy.intercept('GET', `**/api/solicitacao/${solicitation.id}`, {
    statusCode: 200,
    body: solicitation,
  }).as('getSolicitation')

  cy.intercept('GET', `**/api/solicitacao/${solicitation.id}/historico`, {
    statusCode: 200,
    body: [],
  }).as('getHistory')
}

const openDetails = (solicitationId = 101) => {
  cy.visit(`/solicitacoes/${solicitationId}`)
  cy.wait('@getSolicitation')
  cy.wait('@getHistory')
  cy.wait('@getSettings')
}

describe('Regras de visibilidade na edição de solicitação', () => {
  beforeEach(() => {
    cy.loginSession('solicitante')
    cy.visit('/')
  })

  it('deve exibir botão "Editar" apenas para status editáveis (Pendente e Aguardando Ajustes)', () => {
    getUserSigla().then((sigla) => {
      const pending = buildSolicitation({
        id: 101,
        requesterSigla: sigla,
        statusId: 1,
        statusNome: 'Pendente',
      })

      mockSettings()
      mockDetailsPage(pending)
      openDetails(101)
      cy.contains('button', 'Editar').should('be.visible')

      const awaiting = buildSolicitation({
        id: 102,
        requesterSigla: sigla,
        statusId: 2,
        statusNome: 'Aguardando Ajustes',
      })

      mockSettings()
      mockDetailsPage(awaiting)
      openDetails(102)
      cy.contains('button', 'Editar').should('be.visible')
    })
  })

  it('não deve exibir botão "Editar" para status não editáveis', () => {
    getUserSigla().then((sigla) => {
      const statuses = [
        { id: 3, nome: 'Aprovada' },
        { id: 4, nome: 'Rejeitada' },
        { id: 5, nome: 'Cancelada' },
        { id: 6, nome: 'Encerrada' },
      ]

      statuses.forEach((status, idx) => {
        const solicitation = buildSolicitation({
          id: 200 + idx,
          requesterSigla: sigla,
          statusId: status.id,
          statusNome: status.nome,
        })
        mockSettings()
        mockDetailsPage(solicitation)
        openDetails(solicitation.id)
        cy.contains('button', 'Editar').should('not.exist')
      })
    })
  })

  it('não deve exibir botão "Editar" com prazo de ajustes expirado', () => {
    getUserSigla().then((sigla) => {
      const solicitation = buildSolicitation({
        requesterSigla: sigla,
        statusId: 1,
        statusNome: 'Pendente',
      })
      mockSettings({ prazoSubmissao: '2000-01-01T00:00:00.000Z' })
      mockDetailsPage(solicitation)
      openDetails(solicitation.id)

      cy.contains('Prazo para ajustes encerrado.').should('exist')
      cy.contains('button', 'Editar').should('not.exist')
    })
  })

  it('não deve exibir botão "Editar" quando a solicitação não pertence à unidade do usuário', () => {
    getUserSigla().then((sigla) => {
      const solicitation = buildSolicitation({
        requesterSigla: sigla === 'OUTRA' ? 'DCOMP' : 'OUTRA',
        statusId: 1,
        statusNome: 'Pendente',
      })
      mockSettings()
      mockDetailsPage(solicitation)
      openDetails(solicitation.id)
      cy.contains('button', 'Editar').should('not.exist')
    })
  })
})
