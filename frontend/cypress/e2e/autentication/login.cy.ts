describe('Fluxo de Autenticação - Multi-perfil', () => {
  // Centralizamos os dados que variam entre os testes
  const profiles = [
    { email: 'solicitante@sistema.com', welcomeText: 'Olá, Solicitante Padrão' },
    { email: 'gestor@sistema.com', welcomeText: 'Olá, Gestor Padrão' },
    { email: 'admin@sistema.com', welcomeText: 'Olá, Admin Padrão' }
  ]

  profiles.forEach(({ email, welcomeText }) => {
    it(`deve realizar login como ${email.split('@')[0].toUpperCase()} e ver boas-vindas`, () => {
      cy.visit('/login')
      
      // Preenchimento comum a todos os perfis
      cy.get('input[type="email"]').type(email)
      cy.get('input[type="password"]').type('123456')
      cy.get('button[type="submit"]').click()

      // Asserções comuns
      cy.url().should('eq', `${Cypress.config().baseUrl}/`)
      cy.contains(welcomeText).should('be.visible')
    })
  })
})