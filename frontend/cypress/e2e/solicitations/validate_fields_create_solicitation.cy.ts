describe('Validações de criação de solicitação geral', () => {
  beforeEach(() => {
    cy.loginSession('solicitante')
    cy.mockCatalogSeedData()
    cy.visit('/solicitacoes/criar/geral')
    cy.wait('@getCatalog')
  })

  it('deve manter o botão "Solicitar" desativado quando a lista de itens estiver vazia', () => {
    cy.contains('Não há itens na sua solicitação.').should('be.visible')
    cy.contains('button', 'Solicitar').should('be.disabled')
  })

  it('deve manter o botão "Solicitar" desativado quando a justificativa geral estiver vazia', () => {
    cy.get('button[aria-label="Adicionar à Solicitação"]:visible').first().click()
    cy.get('input#on_label_qtde').should('exist')
    cy.get('#textarea_label').should('be.empty')
    cy.contains('button', 'Solicitar').should('be.disabled')
  })

})
