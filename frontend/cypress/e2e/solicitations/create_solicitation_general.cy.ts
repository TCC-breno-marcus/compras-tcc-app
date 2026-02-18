describe('Criar solicitação geral', () => {

  it('deve criar uma solicitação geral com 2 itens', () => {
    const quantidade1 = Cypress._.random(1, 3)
    const quantidade2 = Cypress._.random(1, 3)
    const preco1 = Cypress._.random(10, 150)
    const preco2 = Cypress._.random(10, 150)
    const justificativaGeral = `Justificativa geral e2e ${Date.now()}`

    cy.loginSession('solicitante')
    cy.visit('/solicitacoes/criar/geral')

    cy.url().should('include', '/solicitacoes/criar/geral')
    cy.contains('h3', 'Buscar Itens').should('be.visible')

    cy.get('button[aria-label="Adicionar à Solicitação"]:visible')
      .should('exist')
      .first()
      .click()

    cy.get('button[aria-label="Adicionar à Solicitação"]:visible').eq(1).click()

    cy.get('input#on_label_qtde').should('have.length.at.least', 2)
    cy.get('input#on_label_price').should('have.length.at.least', 2)

    cy.fillNumericInput('input#on_label_qtde', 0, quantidade1)
    cy.fillNumericInput('input#on_label_qtde', 1, quantidade2)
    cy.fillNumericInput('input#on_label_price', 0, preco1)
    cy.fillNumericInput('input#on_label_price', 1, preco2)

    cy.get('#textarea_label').should('be.visible').type(justificativaGeral)

    cy.intercept('POST', '**/api/solicitacao/geral').as('createGeneralSolicitation')
    cy.contains('button', 'Solicitar').should('be.enabled').click()

    cy.wait('@createGeneralSolicitation').then(({ request, response }) => {
      expect(response?.statusCode).to.be.oneOf([200, 201])
    })

    cy.contains('Sua solicitação foi enviada.').should('be.visible')
    cy.contains('Não há itens na sua solicitação.').should('be.visible')
  })
})
