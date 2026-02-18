const getAuthToken = (): Cypress.Chainable<string> =>
  cy.window().then((win) => {
    const auth = JSON.parse(win.localStorage.getItem('auth') || '{}')
    return auth?.token as string
  })

const getEditableSolicitationByApi = () =>
  getAuthToken().then((token) =>
    cy
      .request({
        method: 'GET',
        url: '/api/solicitacao/minhas-solicitacoes?pageNumber=1&pageSize=10&statusIds=1&statusIds=2',
        headers: {
          Authorization: `Bearer ${token}`,
        },
      })
      .then(({ body }) => body.data || []),
  )

const createGeneralSolicitationIfNeeded = () =>
  getEditableSolicitationByApi().then((solicitations) => {
    if (solicitations.length > 0) {
      return solicitations[0].id as number
    }

    cy.visit('/solicitacoes/criar/geral')
    cy.get('button[aria-label="Adicionar à Solicitação"]:visible').first().click()
    cy.fillNumericInput('input#on_label_price', 0, 123)
    cy.get('#textarea_label').type(`Solicitação criada para smoke edit ${Date.now()}`)

    cy.intercept('POST', '**/api/solicitacao/geral').as('createGeneralForEdit')
    cy.contains('button', 'Solicitar').should('be.enabled').click()
    cy.wait('@createGeneralForEdit').its('response.statusCode').should('be.oneOf', [200, 201])

    return getEditableSolicitationByApi().then((newList) => {
      expect(newList.length, 'deve existir solicitação editável após criação').to.be.greaterThan(0)
      return newList[0].id as number
    })
  })

describe('Edição de solicitação - smoke real', () => {
  it('deve editar e salvar uma solicitação real existente', () => {
    cy.loginSession('solicitante')
    cy.visit('/')

    createGeneralSolicitationIfNeeded().then((id) => {
      cy.intercept('PATCH', `**/api/solicitacao/${id}`).as('patchSolicitation')

      cy.visit(`/solicitacoes/${id}`)
      cy.contains('button', 'Editar').should('be.visible').click()

      cy.fillNumericInput('input#on_label_price', 0, 321)
      cy.contains('button', 'Salvar').should('be.enabled').click()
      cy.get('.p-confirmdialog').contains('button', 'Salvar').click()

      cy.wait('@patchSolicitation').then(({ response }) => {
        expect(response?.statusCode).to.be.oneOf([200, 204])
      })

      cy.contains('A solicitação foi salva com sucesso.').should('exist')
    })
  })
})
