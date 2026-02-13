declare global {
  namespace Cypress {
    interface Chainable {
      loginSession(perfil?: 'solicitante' | 'gestor' | 'admin'): Chainable<void>
      fillNumericInput(selector: string, index: number, value: number): Chainable<void>
    }
  }
}

Cypress.Commands.add('loginSession', (perfil: 'solicitante' | 'gestor' | 'admin' = 'solicitante') => {
  const usuarios = {
    solicitante: { email: 'solicitante@sistema.com', senha: '123456', nome: 'Solicitante Padrão' },
    gestor: { email: 'gestor@sistema.com', senha: '123456', nome: 'Gestor Padrão' },
    admin: { email: 'admin@sistema.com', senha: '123456', nome: 'Admin Padrão' }
  } as const

  const { email, senha, nome } = usuarios[perfil]

  cy.session(
    [perfil],
    () => {
    cy.visit('/login')
    cy.get('input[type="email"]').type(email)
    cy.get('input[type="password"]').type(senha)
    cy.get('button[type="submit"]').click()
      cy.url().should('eq', `${Cypress.config().baseUrl}/`)
      cy.contains(`Olá, ${nome}`).should('be.visible')
    },
    {
      validate() {
        cy.visit('/')
        cy.url().should('eq', `${Cypress.config().baseUrl}/`)
      },
    },
  )
})

Cypress.Commands.add('fillNumericInput', (selector, index, value) => {
  cy.get(selector)
    .eq(index)
    .should('be.visible')
    .click()
    .type('{selectall}{backspace}')
    .type(`${value}`)
    .blur()
})

export {}

