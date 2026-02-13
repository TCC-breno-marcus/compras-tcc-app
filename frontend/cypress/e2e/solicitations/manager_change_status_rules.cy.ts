type MockSolicitationOptions = {
  id?: number
  statusId?: number
  statusNome?: string
  requesterSigla: string
}

const statusDescriptionByName: Record<string, string> = {
  Pendente: 'Solicitação recém-criada, aguardando a análise do gestor.',
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
  id = 801,
  statusId = 1,
  statusNome = 'Pendente',
  requesterSigla,
}: MockSolicitationOptions) => ({
  id,
  dataCriacao: '2026-01-10T10:00:00.000Z',
  externalId: `SOL-${id}`,
  justificativaGeral: 'Justificativa de teste',
  status: {
    id: statusId,
    nome: statusNome,
    descricao: statusDescriptionByName[statusNome] || 'Status de teste',
  },
  solicitante: {
    id: '1',
    nome: 'Solicitante Padrão',
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
  itens: [
    {
      id: 901,
      nome: 'Item Teste A',
      catMat: 'CAT-901',
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
      nome: 'Item Teste A',
      catMat: 'CAT-901',
      valor: 100,
    },
  ],
})

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

const openDetails = (solicitationId: number) => {
  cy.visit(`/solicitacoes/${solicitationId}`)
  cy.wait('@getSolicitation')
  cy.wait('@getHistory')
  cy.wait('@getSettings')
}

const getStatusPencilButton = () =>
  cy
    .contains('span', 'Status')
    .parents('li')
    .find('button')
    .filter(':has(.pi-pencil)')
    .first()

describe('Gestor - ações críticas na tela de solicitação', () => {
  beforeEach(() => {
    cy.loginSession('gestor')
    cy.visit('/')
  })

  it('não deve exibir botão "Editar" para gestor', () => {
    getUserSigla().then((sigla) => {
      const solicitation = buildSolicitation({
        id: 810,
        requesterSigla: sigla,
        statusId: 1,
        statusNome: 'Pendente',
      })
      mockSettings()
      mockDetailsPage(solicitation)
      openDetails(solicitation.id)

      cy.contains('button', 'Editar').should('not.exist')
      getStatusPencilButton().should('be.visible')
    })
  })

  it('deve permitir abrir alteração de status quando status não for 5 ou 6', () => {
    getUserSigla().then((sigla) => {
      const editableStatuses = [
        { id: 1, nome: 'Pendente' },
        { id: 2, nome: 'Aguardando Ajustes' },
        { id: 3, nome: 'Aprovada' },
        { id: 4, nome: 'Rejeitada' },
      ]

      editableStatuses.forEach((status, idx) => {
        const solicitation = buildSolicitation({
          id: 820 + idx,
          requesterSigla: sigla,
          statusId: status.id,
          statusNome: status.nome,
        })
        mockSettings()
        mockDetailsPage(solicitation)
        openDetails(solicitation.id)

        getStatusPencilButton().should('not.be.disabled').click()
        cy.contains('Alterar Status').should('be.visible')
      })
    })
  })

  it('deve bloquear alteração de status quando status for 5 (Cancelada) ou 6 (Encerrada)', () => {
    getUserSigla().then((sigla) => {
      const blockedStatuses = [
        { id: 5, nome: 'Cancelada' },
        { id: 6, nome: 'Encerrada' },
      ]

      blockedStatuses.forEach((status, idx) => {
        const solicitation = buildSolicitation({
          id: 840 + idx,
          requesterSigla: sigla,
          statusId: status.id,
          statusNome: status.nome,
        })
        mockSettings()
        mockDetailsPage(solicitation)
        openDetails(solicitation.id)

        getStatusPencilButton().should('be.disabled')
      })
    })
  })

  it('deve exigir justificativa ao alterar para status diferente de Aprovada', () => {
    getUserSigla().then((sigla) => {
      const solicitation = buildSolicitation({
        id: 850,
        requesterSigla: sigla,
        statusId: 1,
        statusNome: 'Pendente',
      })
      mockSettings()
      mockDetailsPage(solicitation)
      cy.intercept('PATCH', `**/api/solicitacao/${solicitation.id}/status`).as('patchStatus')

      openDetails(solicitation.id)
      getStatusPencilButton().click()
      cy.contains('.status-option', 'Rejeitada').click()

      cy.contains('A justificativa para essa ação é obrigatória.').should('be.visible')
      cy.get('@patchStatus.all').should('have.length', 0)
    })
  })

  it('deve permitir aprovar sem justificativa e enviar PATCH de status', () => {
    getUserSigla().then((sigla) => {
      const solicitation = buildSolicitation({
        id: 860,
        requesterSigla: sigla,
        statusId: 1,
        statusNome: 'Pendente',
      })
      mockSettings()
      mockDetailsPage(solicitation)
      cy.intercept('PATCH', `**/api/solicitacao/${solicitation.id}/status`, (req) => {
        req.reply({
          statusCode: 200,
          body: {
            ...solicitation,
            status: {
              id: 3,
              nome: 'Aprovada',
              descricao: statusDescriptionByName.Aprovada,
            },
          },
        })
      }).as('patchStatus')

      openDetails(solicitation.id)
      getStatusPencilButton().click()
      cy.contains('.status-option', 'Aprovada').click()

      cy.wait('@patchStatus').then(({ request, response }) => {
        expect(response?.statusCode).to.eq(200)
        expect(request.body.novoStatusId).to.eq(3)
      })
      cy.contains('O status foi salvo com sucesso.').should('exist')
    })
  })

  it('deve alterar status com justificativa e tratar erro da API', () => {
    getUserSigla().then((sigla) => {
      const solicitation = buildSolicitation({
        id: 870,
        requesterSigla: sigla,
        statusId: 1,
        statusNome: 'Pendente',
      })
      mockSettings()
      mockDetailsPage(solicitation)
      cy.intercept('PATCH', `**/api/solicitacao/${solicitation.id}/status`, {
        statusCode: 500,
        body: { message: 'Falha ao atualizar status.' },
      }).as('patchStatus')

      openDetails(solicitation.id)
      getStatusPencilButton().click()
      cy.get('#observation').type('Solicitação precisa de ajustes.')
      cy.contains('.status-option', 'Aguardando Ajustes').click()

      cy.wait('@patchStatus')
      cy.contains('Falha ao atualizar status.').should('exist')
    })
  })

  it('não deve listar o status atual nas opções de alteração', () => {
    getUserSigla().then((sigla) => {
      const solicitation = buildSolicitation({
        id: 880,
        requesterSigla: sigla,
        statusId: 2,
        statusNome: 'Aguardando Ajustes',
      })
      mockSettings()
      mockDetailsPage(solicitation)
      openDetails(solicitation.id)

      getStatusPencilButton().click()
      cy.contains('.status-option', 'Aguardando Ajustes').should('not.exist')
    })
  })
})
