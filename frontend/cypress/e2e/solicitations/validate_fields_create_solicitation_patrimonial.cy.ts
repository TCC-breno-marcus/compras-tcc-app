describe('Validações de criação de solicitação patrimonial', () => {
  beforeEach(() => {
    cy.loginSession('solicitante')
    cy.visit('/solicitacoes/criar/patrimonial')
  })

  it('deve manter o botão "Solicitar" desativado quando a lista de itens estiver vazia', () => {
    cy.contains('Não há itens na sua solicitação.').should('be.visible')
    cy.contains('button', 'Solicitar').should('be.disabled')
  })

  it('deve manter o botão "Solicitar" desativado quando houver itens sem justificativa', () => {
    cy.get('button[aria-label="Adicionar à Solicitação"]:visible').first().click()
    cy.get('input[inputid="on_label_justification"]').should('have.length.at.least', 1)
    cy.contains('button', 'Solicitar').should('be.disabled')
  })

  it('deve manter o botão "Solicitar" desativado até preencher justificativa dos 2 itens', () => {
    cy.get('button[aria-label="Adicionar à Solicitação"]:visible').first().click()
    cy.get('button[aria-label="Adicionar à Solicitação"]:visible').eq(1).click()

    cy.get('input[inputid="on_label_justification"]').should('have.length.at.least', 2)
    cy.get('input[inputid="on_label_justification"]').eq(0).type('Justificativa item 1')
    cy.contains('button', 'Solicitar').should('be.disabled')

    cy.get('input[inputid="on_label_justification"]').eq(1).type('Justificativa item 2')
    cy.contains('button', 'Solicitar').should('be.enabled')
  })
})
