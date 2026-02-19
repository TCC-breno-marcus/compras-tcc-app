describe('Regras críticas da criação de solicitação', () => {
  const futureDate = '2099-12-31T23:59:59.000Z'
  const pastDate = '2000-01-01T00:00:00.000Z'

  const mockSettings = ({
    prazoSubmissao = futureDate,
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

  it('deve manter o botão "Solicitar" desativado com prazo encerrado', () => {
    cy.loginSession('solicitante')
    cy.mockCatalogSeedData()
    mockSettings({ prazoSubmissao: pastDate })
    cy.visit('/solicitacoes/criar/geral')
    cy.wait('@getSettings')
    cy.wait('@getCatalog')

    cy.get('button[aria-label="Adicionar à Solicitação"]:visible').first().click()
    cy.fillNumericInput('input#on_label_price', 0, 50)
    cy.get('#textarea_label').type('Justificativa geral para teste de prazo')

    cy.contains('Prazo para envio encerrado.').should('exist')
    cy.contains('button', 'Solicitar').should('be.disabled')
  })

  it('deve respeitar os limites de configuração (itens diferentes e quantidade por item)', () => {
    cy.loginSession('solicitante')
    cy.mockCatalogSeedData()
    mockSettings({ maxItensDiferentesPorSolicitacao: 1, maxQuantidadePorItem: 1 })
    cy.visit('/solicitacoes/criar/geral')
    cy.wait('@getSettings')
    cy.wait('@getCatalog')

    cy.get('button[aria-label="Adicionar à Solicitação"]:visible').first().click()
    cy.get('button[aria-label="Adicionar à Solicitação"]:visible').eq(1).click()
    cy.get('input#on_label_qtde').should('have.length', 1)

    cy.get('button[aria-label="Adicionar à Solicitação"]:visible').first().click()
    cy.get('input#on_label_qtde').first().should('have.value', '1')

    cy.fillNumericInput('input#on_label_qtde', 0, 2)
    cy.get('input#on_label_qtde').first().should('have.value', '1')
  })

  it('deve exibir confirmação ao trocar o tipo de solicitação com carrinho em andamento (cancelar mantém carrinho)', () => {
    cy.loginSession('solicitante')
    cy.mockCatalogSeedData()
    mockSettings()
    cy.intercept('GET', '**/api/solicitacao/minhas-solicitacoes*', {
      statusCode: 200,
      body: {
        pageNumber: 1,
        pageSize: 10,
        totalCount: 0,
        totalPages: 1,
        data: [],
      },
    }).as('getMySolicitations')

    cy.visit('/solicitacoes/criar/geral')
    cy.wait('@getSettings')
    cy.wait('@getCatalog')
    cy.get('button[aria-label="Adicionar à Solicitação"]:visible').first().click()

    cy.get('.p-breadcrumb').contains('a', 'Solicitações').click()
    cy.wait('@getMySolicitations')

    cy.contains('button', 'Patrimonial').click()
    cy.contains('Solicitação Existente').should('be.visible')
    cy.contains('Você tem uma solicitação geral em andamento').should('be.visible')
    cy.contains('button', 'Cancelar').click()

    cy.url().should('include', '/solicitacoes')
    cy.contains('button', 'Geral').click()
    cy.url().should('include', '/solicitacoes/criar/geral')
    cy.get('input#on_label_qtde').should('have.length.at.least', 1)
  })

  it('deve descartar o carrinho e navegar ao aceitar troca de tipo', () => {
    cy.loginSession('solicitante')
    cy.mockCatalogSeedData()
    mockSettings()
    cy.intercept('GET', '**/api/solicitacao/minhas-solicitacoes*', {
      statusCode: 200,
      body: {
        pageNumber: 1,
        pageSize: 10,
        totalCount: 0,
        totalPages: 1,
        data: [],
      },
    }).as('getMySolicitations')

    cy.visit('/solicitacoes/criar/geral')
    cy.wait('@getSettings')
    cy.wait('@getCatalog')
    cy.get('button[aria-label="Adicionar à Solicitação"]:visible').first().click()

    cy.get('.p-breadcrumb').contains('a', 'Solicitações').click()
    cy.wait('@getMySolicitations')

    cy.contains('button', 'Patrimonial').click()
    cy.contains('button', 'Descartar e Iniciar Nova').click()

    cy.url().should('include', '/solicitacoes/criar/patrimonial')
    cy.contains('Não há itens na sua solicitação.').should('be.visible')
  })
})
