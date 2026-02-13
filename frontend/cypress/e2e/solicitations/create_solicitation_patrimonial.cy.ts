describe('Criar solicitação patrimonial', () => {
  it('deve criar uma solicitação patrimonial com 2 itens', () => {
    const quantidade1 = Cypress._.random(1, 3)
    const quantidade2 = Cypress._.random(1, 3)
    const preco1 = Cypress._.random(10, 150)
    const preco2 = Cypress._.random(10, 150)
    const justificativaItem1 = `Justificativa item 1 e2e ${Date.now()}`
    const justificativaItem2 = `Justificativa item 2 e2e ${Date.now()}`

    cy.loginSession('solicitante')
    cy.visit('/solicitacoes/criar/patrimonial')

    cy.url().should('include', '/solicitacoes/criar/patrimonial')
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

    cy.get('input[inputid="on_label_justification"]').should('have.length.at.least', 2)
    cy.get('input[inputid="on_label_justification"]').eq(0).type(justificativaItem1)
    cy.get('input[inputid="on_label_justification"]').eq(1).type(justificativaItem2)

    cy.intercept('POST', '**/api/solicitacao/patrimonial').as('createPatrimonialSolicitation')
    cy.contains('button', 'Solicitar').should('be.enabled').click()

    cy.wait('@createPatrimonialSolicitation').then(({ request, response }) => {
      expect(response?.statusCode).to.be.oneOf([200, 201])
      expect(request.body).to.be.an('object')
      expect(request.body).to.not.have.property('justificativaGeral')
      expect(request.body.itens).to.have.length(2)
      expect(request.body.itens[0].justificativa).to.eq(justificativaItem1)
      expect(request.body.itens[1].justificativa).to.eq(justificativaItem2)
    })

    cy.contains('Sua solicitação foi enviada.').should('be.visible')
    cy.contains('Não há itens na sua solicitação.').should('be.visible')
  })
})
