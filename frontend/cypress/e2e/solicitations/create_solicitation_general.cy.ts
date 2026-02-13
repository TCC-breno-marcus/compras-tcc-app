describe('Fluxo de Autenticação', () => {
  beforeEach(() => {
    cy.intercept('POST', '**/api/auth/login', (req) => {
      req.continue((res) => {
        // Se a requisição falhar ou retornar erro, você verá no log do terminal do Cypress
        if (res.statusCode >= 400 || !res) {
          console.log('❌ FALHA NO LOGIN:', {
            url: req.url,
            status: res?.statusCode,
            body: res?.body,
            error: res?.body || 'Resposta de erro sem mensagem detalhada', // Captura a mensagem de erro se disponível
          })
        }
      })
    }).as('loginAttempt')
  })

  it('deve realizar login com sucesso e redirecionar para a home', () => {
    // 1. Acessa a página de login
    cy.visit('/login')

    // 2. Preenche as credenciais
    // Ajuste os seletores 'input[type="email"]' ou 'input[type="password"]'
    // se o seu componente PrimeVue usar IDs específicos
    cy.get('input').first().type('solicitante@sistema.com') // Exemplo de e-mail
    cy.get('input').last().type('123456') // Exemplo de senha

    // 3. Clica no botão de entrar
    cy.get('button[type="submit"]').click()

    // 4. Valida se a URL mudou para a home (opcional)
    cy.url().should('include', '/')

    // 5. Valida o texto de boas-vindas na página Home
    // O comando 'contains' procura o texto exato dentro da página
    cy.contains('Olá, Solicitante Padrão').should('be.visible')
  })
})
