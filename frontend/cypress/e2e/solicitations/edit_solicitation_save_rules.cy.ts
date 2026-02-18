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

describe('Regras de validação e salvamento na edição de solicitação', () => {
  beforeEach(() => {
    cy.loginSession('solicitante')
    cy.visit('/')
  })

  it('não deve salvar com lista de itens vazia', () => {
    getUserSigla().then((sigla) => {
      const solicitation = buildSolicitation({
        requesterSigla: sigla,
        itens: [
          {
            id: 301,
            nome: 'Item Único',
            catMat: 'CAT-301',
            quantidade: 1,
            precoSugerido: 100,
          },
        ],
      })
      mockSettings()
      mockDetailsPage(solicitation)
      cy.intercept('PATCH', `**/api/solicitacao/${solicitation.id}`).as('patchSolicitation')

      openDetails(solicitation.id)
      cy.contains('button', 'Editar').click()
      cy.get('button').filter(':has(.pi-trash)').first().click()
      cy.contains('button', 'Salvar').click()
      cy.get('@patchSolicitation.all').should('have.length', 0)
      cy.contains('Salvar Alterações?').should('not.exist')
    })
  })

  it('não deve salvar solicitação geral com justificativa geral vazia', () => {
    getUserSigla().then((sigla) => {
      const solicitation = buildSolicitation({
        requesterSigla: sigla,
        type: 'geral',
        justificativaGeral: 'Texto inicial',
      })
      mockSettings()
      mockDetailsPage(solicitation)
      cy.intercept('PATCH', `**/api/solicitacao/${solicitation.id}`).as('patchSolicitation')

      openDetails(solicitation.id)
      cy.contains('button', 'Editar').click()
      cy.get('#textarea_label').clear()
      cy.contains('button', 'Salvar').click()
      cy.get('@patchSolicitation.all').should('have.length', 0)
      cy.contains('Salvar Alterações?').should('not.exist')
    })
  })

  it('não deve salvar quando algum item tiver preço unitário 0', () => {
    getUserSigla().then((sigla) => {
      const solicitation = buildSolicitation({
        requesterSigla: sigla,
      })
      mockSettings()
      mockDetailsPage(solicitation)
      cy.intercept('PATCH', `**/api/solicitacao/${solicitation.id}`).as('patchSolicitation')

      openDetails(solicitation.id)
      cy.contains('button', 'Editar').click()
      cy.fillNumericInput('input#on_label_price', 0, 0)
      cy.contains('button', 'Salvar').click()
      cy.get('@patchSolicitation.all').should('have.length', 0)
      cy.contains('Salvar Alterações?').should('not.exist')
    })
  })

  it('não deve salvar quando algum item tiver quantidade inválida (<= 0)', () => {
    getUserSigla().then((sigla) => {
      const solicitation = buildSolicitation({
        requesterSigla: sigla,
        itens: [
          {
            id: 401,
            nome: 'Item Quantidade Zero',
            catMat: 'CAT-401',
            quantidade: 0,
            precoSugerido: 100,
          },
        ],
      })
      mockSettings()
      mockDetailsPage(solicitation)
      cy.intercept('PATCH', `**/api/solicitacao/${solicitation.id}`).as('patchSolicitation')

      openDetails(solicitation.id)
      cy.contains('button', 'Editar').click()
      cy.get('#textarea_label').type(' ajuste')
      cy.contains('button', 'Salvar').click()
      cy.get('@patchSolicitation.all').should('have.length', 0)
      cy.contains('Salvar Alterações?').should('not.exist')
    })
  })

  it('não deve salvar solicitação patrimonial sem justificativa por item', () => {
    getUserSigla().then((sigla) => {
      const solicitation = buildSolicitation({
        requesterSigla: sigla,
        type: 'patrimonial',
        itens: [
          {
            id: 501,
            nome: 'Item Patrimonial',
            catMat: 'CAT-501',
            quantidade: 1,
            precoSugerido: 100,
            justificativa: '',
          },
        ],
      })
      mockSettings()
      mockDetailsPage(solicitation)
      cy.intercept('PATCH', `**/api/solicitacao/${solicitation.id}`).as('patchSolicitation')

      openDetails(solicitation.id)
      cy.contains('button', 'Editar').click()
      cy.fillNumericInput('input#on_label_price', 0, 110)
      cy.contains('button', 'Salvar').click()
      cy.get('@patchSolicitation.all').should('have.length', 0)
      cy.contains('Salvar Alterações?').should('not.exist')
    })
  })

  it('deve salvar edição geral com payload correto', () => {
    getUserSigla().then((sigla) => {
      const solicitation = buildSolicitation({
        requesterSigla: sigla,
        type: 'geral',
      })
      const newJustification = `Justificativa atualizada ${Date.now()}`
      const newPrice = 222

      mockSettings()
      mockDetailsPage(solicitation)
      cy.intercept('PATCH', `**/api/solicitacao/${solicitation.id}`, (req) => {
        req.reply({
          statusCode: 200,
          body: {
            ...solicitation,
            justificativaGeral: newJustification,
            itens: solicitation.itens.map((item, index) => ({
              ...item,
              precoSugerido: index === 0 ? newPrice : item.precoSugerido,
            })),
          },
        })
      }).as('patchSolicitation')

      openDetails(solicitation.id)
      cy.contains('button', 'Editar').click()
      cy.get('#textarea_label').clear().type(newJustification)
      cy.fillNumericInput('input#on_label_price', 0, newPrice)
      cy.contains('button', 'Salvar').click()
      cy.get('.p-confirmdialog').contains('button', 'Salvar').click()

      cy.wait('@patchSolicitation').then(({ request, response }) => {
        expect(response?.statusCode).to.eq(200)
        expect(request.body.justificativaGeral).to.eq(newJustification)
        expect(request.body.itens).to.have.length(2)
        expect(request.body.itens[0].itemId).to.eq(solicitation.itens[0].id)
        expect(request.body.itens[0].valorUnitario).to.eq(newPrice)
      })

      cy.contains('A solicitação foi salva com sucesso.').should('exist')
      cy.contains('button', 'Editar').should('be.visible')
    })
  })

  it('deve salvar edição patrimonial com justificativa por item no payload', () => {
    getUserSigla().then((sigla) => {
      const solicitation = buildSolicitation({
        requesterSigla: sigla,
        type: 'patrimonial',
      })
      const newItemJustification = `Ajuste patrimonial ${Date.now()}`

      mockSettings()
      mockDetailsPage(solicitation)
      cy.intercept('PATCH', `**/api/solicitacao/${solicitation.id}`, (req) => {
        req.reply({
          statusCode: 200,
          body: {
            ...solicitation,
            itens: solicitation.itens.map((item, index) => ({
              ...item,
              justificativa: index === 0 ? newItemJustification : item.justificativa,
            })),
          },
        })
      }).as('patchSolicitation')

      openDetails(solicitation.id)
      cy.contains('button', 'Editar').click()
      cy.get('input[inputid="on_label_justification"]').eq(0).clear().type(newItemJustification)
      cy.contains('button', 'Salvar').click()
      cy.get('.p-confirmdialog').contains('button', 'Salvar').click()

      cy.wait('@patchSolicitation').then(({ request, response }) => {
        expect(response?.statusCode).to.eq(200)
        expect(request.body.itens[0].justificativa).to.eq(newItemJustification)
        expect(request.body.itens).to.have.length(2)
      })
    })
  })

  it('deve manter edição e dados em tela quando API falhar ao salvar', () => {
    getUserSigla().then((sigla) => {
      const solicitation = buildSolicitation({
        requesterSigla: sigla,
        type: 'geral',
      })
      const changedText = `Texto não deve ser perdido ${Date.now()}`

      mockSettings()
      mockDetailsPage(solicitation)
      cy.intercept('PATCH', `**/api/solicitacao/${solicitation.id}`, {
        statusCode: 500,
        body: { message: 'Erro interno' },
      }).as('patchSolicitation')

      openDetails(solicitation.id)
      cy.contains('button', 'Editar').click()
      cy.get('#textarea_label').clear().type(changedText)
      cy.contains('button', 'Salvar').click()
      cy.get('.p-confirmdialog').contains('button', 'Salvar').click()

      cy.wait('@patchSolicitation')
      cy.contains('Não foi possível salvar as alterações.').should('exist')
      cy.contains('button', 'Cancelar').should('be.visible')
      cy.get('#textarea_label').should('have.value', changedText)
    })
  })

  it('deve respeitar limite de quantidade por item no modo edição', () => {
    getUserSigla().then((sigla) => {
      const solicitation = buildSolicitation({
        requesterSigla: sigla,
      })

      mockSettings({ maxQuantidadePorItem: 1 })
      mockDetailsPage(solicitation)

      openDetails(solicitation.id)
      cy.contains('button', 'Editar').click()
      cy.fillNumericInput('input#on_label_qtde', 0, 2)
      cy.get('input#on_label_qtde').first().should('have.value', '1')
    })
  })
})
