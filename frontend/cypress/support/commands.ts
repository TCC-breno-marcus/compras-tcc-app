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
    solicitante: { email: 'solicitante@sistema.com', senha: '123456' },
    gestor: { email: 'gestor@sistema.com', senha: '123456' },
    admin: { email: 'admin@sistema.com', senha: '123456' },
  } as const

  const { email, senha } = usuarios[perfil]

  cy.session(
    [perfil],
    () => {
      cy.request({
        method: 'POST',
        url: '/api/auth/login',
        body: { email, password: senha },
      }).then(({ body }) => {
        const token = body.token as string

        cy.request({
          method: 'GET',
          url: '/api/auth/me',
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }).then(({ body: user }) => {
          cy.visit('/', {
            onBeforeLoad(win) {
              win.localStorage.clear()
              win.localStorage.setItem(
                'auth',
                JSON.stringify({
                  user,
                  token,
                }),
              )
            },
          })
        })
      })
    },
    {
      validate() {
        cy.visit('/solicitacoes/criar/geral')
        cy.url().should('include', '/solicitacoes/criar/geral')
      },
      cacheAcrossSpecs: true,
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
