type Status = {
  id: number
  nome: string
  descricao: string
}

type HistoryEvent = {
  id: string
  dataOcorrencia: string
  acao: string
  detalhes: string
  observacoes: string | null
  nomePessoa: string
}

const STATUS_MAP: Record<number, Status> = {
  1: { id: 1, nome: 'Pendente', descricao: 'Aguardando análise do gestor.' },
  2: { id: 2, nome: 'Aguardando Ajustes', descricao: 'Devolvida para ajustes.' },
  3: { id: 3, nome: 'Aprovada', descricao: 'Solicitação aprovada.' },
  4: { id: 4, nome: 'Rejeitada', descricao: 'Solicitação rejeitada.' },
  5: { id: 5, nome: 'Cancelada', descricao: 'Solicitação cancelada.' },
  6: { id: 6, nome: 'Encerrada', descricao: 'Solicitação encerrada.' },
}

const baseDepartment = {
  id: 1,
  nome: 'Departamento de Computação',
  sigla: 'DCOMP',
  email: 'dcomp@ufs.br',
  telefone: '79999999999',
  tipo: 'Departamento',
}

const baseSolicitation = ({
  id,
  externalId,
  statusId,
}: {
  id: number
  externalId: string
  statusId: number
}) => ({
  id,
  dataCriacao: '2026-02-01T10:00:00.000Z',
  externalId,
  justificativaGeral: 'Justificativa de teste',
  status: STATUS_MAP[statusId],
  solicitante: {
    id: '10',
    nome: 'Solicitante Padrão',
    email: 'solicitante@sistema.com',
    unidade: baseDepartment,
  },
  itens: [
    {
      id: 701,
      nome: 'Item Teste',
      catMat: 'CAT-701',
      linkImagem: '',
      quantidade: 1,
      precoSugerido: 100,
    },
  ],
  kpis: {
    valorTotalEstimado: 100,
    totalItensUnicos: 1,
    totalUnidades: 1,
  },
  valorPorCategoria: {
    labels: ['Diversos'],
    data: [100],
  },
  topItensPorValor: [
    {
      nome: 'Item Teste',
      catMat: 'CAT-701',
      valor: 100,
    },
  ],
})

const setupSolicitationsState = () => {
  const state = {
    solicitations: [
      baseSolicitation({ id: 1001, externalId: 'SOL-1001', statusId: 1 }), // pendente
      baseSolicitation({ id: 1002, externalId: 'SOL-1002', statusId: 6 }), // encerrada
      baseSolicitation({ id: 1003, externalId: 'SOL-1003', statusId: 5 }), // cancelada
    ],
    history: {
      1001: [
        {
          id: 'h1',
          dataOcorrencia: '2026-02-01T10:00:00.000Z',
          acao: 'Criacao',
          detalhes: "Solicitação criada com status 'Pendente'",
          observacoes: null,
          nomePessoa: 'Solicitante Padrão',
        },
      ] as HistoryEvent[],
      1002: [] as HistoryEvent[],
      1003: [] as HistoryEvent[],
    } as Record<number, HistoryEvent[]>,
  }

  cy.intercept('GET', '**/api/configuracao', {
    statusCode: 200,
    body: {
      prazoSubmissao: '2099-12-31T23:59:59.000Z',
      maxQuantidadePorItem: 10,
      maxItensDiferentesPorSolicitacao: 10,
      emailContatoPrincipal: 'contato@sistema.com',
      emailParaNotificacoes: 'notificacoes@sistema.com',
    },
  }).as('getSettings')

  cy.intercept('GET', '**/api/departamento*', {
    statusCode: 200,
    body: [baseDepartment],
  }).as('getDepartments')

  cy.intercept('GET', '**/api/usuario*', {
    statusCode: 200,
    body: {
      pageNumber: 1,
      pageSize: 50,
      totalCount: 1,
      totalPages: 1,
      data: [
        {
          id: 10,
          nome: 'Solicitante Padrão',
          email: 'solicitante@sistema.com',
          telefone: '79999999999',
          cpf: '00000000000',
          role: 'Solicitante',
          isActive: true,
          unidade: baseDepartment,
        },
      ],
    },
  }).as('getUsers')

  cy.intercept('GET', /\/api\/solicitacao(\?.*)?$/, (req) => {
    req.reply({
      statusCode: 200,
      body: {
        pageNumber: 1,
        pageSize: 10,
        totalCount: state.solicitations.length,
        totalPages: 1,
        data: state.solicitations,
      },
    })
  }).as('getSolicitationsList')

  cy.intercept('GET', '**/api/solicitacao/*/historico', (req) => {
    const id = Number(req.url.match(/solicitacao\/(\d+)\/historico/)?.[1])
    req.reply({
      statusCode: 200,
      body: state.history[id] || [],
    })
  }).as('getSolicitationHistory')

  cy.intercept('GET', '**/api/solicitacao/*', (req) => {
    const id = Number(req.url.match(/solicitacao\/(\d+)/)?.[1])
    const solicitation = state.solicitations.find((s) => s.id === id)
    if (!solicitation) {
      req.reply({ statusCode: 404, body: { message: 'Solicitação não encontrada.' } })
      return
    }
    req.reply({ statusCode: 200, body: solicitation })
  }).as('getSolicitationById')

  cy.intercept('PATCH', '**/api/solicitacao/*/status', (req) => {
    const id = Number(req.url.match(/solicitacao\/(\d+)\/status/)?.[1])
    const solicitation = state.solicitations.find((s) => s.id === id)
    if (!solicitation) {
      req.reply({ statusCode: 404, body: { message: 'Solicitação não encontrada.' } })
      return
    }

    const oldStatus = solicitation.status
    const newStatusId = req.body.novoStatusId as number
    const observation = (req.body.observacoes as string) || null
    solicitation.status = STATUS_MAP[newStatusId]

    state.history[id] = [
      ...(state.history[id] || []),
      {
        id: `h-${Date.now()}`,
        dataOcorrencia: '2026-02-02T10:00:00.000Z',
        acao: 'MudancaDeStatus',
        detalhes: `Status alterado de '${oldStatus.nome}' para '${solicitation.status.nome}'`,
        observacoes: observation,
        nomePessoa: 'Gestor Padrão',
      },
    ]

    req.reply({
      statusCode: 200,
      body: solicitation,
    })
  }).as('patchStatus')

  return state
}

const getStatusPencilButton = () =>
  cy
    .contains('span', 'Status')
    .parents('li')
    .find('button')
    .filter(':has(.pi-pencil)')
    .first()

describe('Gestor - fluxo de iteração em solicitações', () => {
  beforeEach(() => {
    cy.loginSession('gestor')
  })

  it('deve iterar pendente e encerrada com regras corretas de edição/status/histórico/lista', () => {
    setupSolicitationsState()

    cy.visit('/gestor/solicitacoes')
    cy.wait('@getSolicitationsList')

    // Abre solicitação pendente
    cy.contains('tr', 'SOL-1001').within(() => {
      cy.get('button[aria-label="Ver Detalhes"]').click()
    })

    // Gestor não pode editar a solicitação (somente solicitante)
    cy.contains('button', 'Editar').should('not.exist')

    // Pode abrir o seletor de status
    getStatusPencilButton().should('not.be.disabled').click()
    cy.contains('Alterar Status').should('be.visible')

    // Status atual não deve aparecer nas opções
    cy.contains('.status-option', 'Pendente').should('not.exist')

    // Sem justificativa em status diferente de "Aprovada" deve falhar
    cy.contains('.status-option', 'Rejeitada').click()
    cy.contains('A justificativa para essa ação é obrigatória.').should('be.visible')
    cy.get('@patchStatus.all').should('have.length', 0)

    // Com justificativa, altera com sucesso
    const observation = 'Mudando para rejeitada por inconsistências.'
    cy.get('#observation').type(observation)
    cy.contains('.status-option', 'Rejeitada').click()

    cy.wait('@patchStatus').then(({ request, response }) => {
      expect(response?.statusCode).to.eq(200)
      expect(request.body.novoStatusId).to.eq(4)
      expect(request.body.observacoes).to.eq(observation)
    })

    cy.contains('O status foi salvo com sucesso.').should('exist')
    cy.contains('Rejeitada').should('exist')

    // Histórico deve refletir status antigo->novo e observação
    cy.contains('[role="tab"]', 'Histórico').click()
    cy.wait('@getSolicitationHistory')
    cy.contains('Status alterado de').should('exist')
    cy.contains('Pendente').should('exist')
    cy.contains('Rejeitada').should('exist')
    cy.contains(`Observações: ${observation}`).should('exist')

    // Volta para lista e valida status atualizado na linha
    cy.visit('/gestor/solicitacoes')
    cy.wait('@getSolicitationsList')
    cy.contains('tr', 'SOL-1001').within(() => {
      cy.contains('Rejeitada').should('exist')
    })

    // Abre encerrada e valida que não pode alterar status
    cy.contains('tr', 'SOL-1002').within(() => {
      cy.get('button[aria-label="Ver Detalhes"]').click()
    })
    getStatusPencilButton().should('be.disabled')

    // Abre cancelada e valida bloqueio também
    cy.visit('/gestor/solicitacoes')
    cy.wait('@getSolicitationsList')
    cy.contains('tr', 'SOL-1003').within(() => {
      cy.get('button[aria-label="Ver Detalhes"]').click()
    })
    getStatusPencilButton().should('be.disabled')
  })

  it('deve permitir aprovar sem justificativa e tratar erro de API de status', () => {
    setupSolicitationsState()

    // Primeiro cenário: aprovar sem justificativa
    cy.visit('/gestor/solicitacoes')
    cy.wait('@getSolicitationsList')
    cy.contains('tr', 'SOL-1001').within(() => {
      cy.get('button[aria-label="Ver Detalhes"]').click()
    })

    getStatusPencilButton().click()
    cy.contains('.status-option', 'Aprovada').click()
    cy.wait('@patchStatus').then(({ request }) => {
      expect(request.body.novoStatusId).to.eq(3)
      expect(request.body.observacoes).to.eq('')
    })

    // Segundo cenário: falha na API de status
    cy.intercept('PATCH', '**/api/solicitacao/*/status', {
      statusCode: 500,
      body: { message: 'Falha ao atualizar status.' },
    }).as('patchStatusError')

    getStatusPencilButton().click()
    cy.get('#observation').type('Tentativa com falha controlada')
    cy.contains('.status-option', 'Aguardando Ajustes').click()
    cy.wait('@patchStatusError')
    cy.contains('Falha ao atualizar status.').should('exist')
  })
})
