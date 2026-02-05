describe('Feature: Criar Solicitação Geral', () => {
  
  beforeEach(() => {
    // 1. Mock do Auth e Settings
    cy.window().then((win) => {
      win.localStorage.setItem('token', 'fake-jwt-token')
      win.localStorage.setItem('user', JSON.stringify({ name: 'Tester', role: 'admin' }))
    })

    // 2. Intercepts (Mocks da API)
    
    // Mock do Catálogo
    cy.intercept('GET', '**/api/catalogo*', {
      statusCode: 200,
      body: {
        content: [
          { 
            id: 1, 
            nome: 'Notebook Dell Latitude', 
            precoSugerido: 5000, 
            catMat: '123', 
            isActive: true 
          }
        ],
        totalElements: 1,
        totalPages: 1
      }
    }).as('getCatalog')

    // Mock das Settings (IMPORTANTE: CartStore precisa disso)
    cy.intercept('GET', '**/api/settings', {
      statusCode: 200,
      body: { maxItensDiferentesPorSolicitacao: 10, maxQuantidadePorItem: 5 }
    }).as('getSettings')

    // Mock do POST
    cy.intercept('POST', '**/api/solicitacao/geral', {
      statusCode: 201,
      body: { id: 100 }
    }).as('createSolicitation')

    cy.visit('/solicitacoes/nova/geral')
  })

  it('deve adicionar item e criar solicitação usando seletores inteligentes', () => {
    // Aguarda catálogo carregar
    cy.wait('@getCatalog')
    cy.contains('Notebook Dell Latitude').should('be.visible')

    // --- PASSO 1: Adicionar Item (Usando aria-label existente) ---
    // Como você tem um botão na listagem principal com esse aria-label:
    cy.get('button[aria-label="Adicionar à Solicitação"]')
      .first()
      .click()
    
    // Verifica toast de sucesso (PrimeVue gera classe .p-toast-message)
    cy.get('.p-toast-message-success')
      .should('be.visible')
      .and('contain.text', 'adicionado à solicitação')

    // --- PASSO 2: Preencher Justificativa ---
    // AQUI É O PULO DO GATO: Se o input não tem ID fixo, 
    // buscamos pelo placeholder ou classe genérica do PrimeVue dentro da área "Sua Solicitação"
    
    // Opção A: Se o textarea tiver placeholder
    // cy.get('textarea[placeholder="Justificativa da solicitação"]').type('Minha justificativa')
    
    // Opção B: Pegar o primeiro textarea da página (se só tiver um visível principal)
    cy.get('textarea').first().type('Preciso deste notebook para desenvolvimento.')

    // --- PASSO 3: Enviar ---
    // Procura o botão pelo texto. Suponho que dentro de MyCurrentSolicitation tenha um botão "Criar" ou "Salvar"
    // Ajuste o texto abaixo para o que aparece na sua tela
    cy.contains('button', 'Criar Solicitação').click()

    // --- PASSO 4: Asserções ---
    cy.wait('@createSolicitation').then((interception) => {
      const body = interception.request.body
      expect(body.justificativaGeral).to.equal('Preciso deste notebook para desenvolvimento.')
      expect(body.itens[0].itemId).to.equal(1)
    })
    
    // Verifica Toast final
    cy.get('.p-toast-message-success')
      .should('contain.text', 'Sucesso')
  })
})