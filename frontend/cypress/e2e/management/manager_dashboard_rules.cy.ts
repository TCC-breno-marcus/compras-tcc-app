const dashboardSuccessPayload = {
  kpis: {
    valorTotalEstimado: 125000.5,
    custoMedioSolicitacao: 25000.1,
    totalItensUnicos: 42,
    totalUnidadesSolicitadas: 300,
    totalDepartamentosSolicitantes: 8,
    totalSolicitacoes: 5,
  },
  valorPorDepartamento: {
    labels: ['DCOMP', 'DQI'],
    data: [80000, 45000.5],
  },
  valorPorCategoria: {
    labels: ['Diversos', 'Ferramentas'],
    data: [70000, 55000.5],
  },
  visaoGeralStatus: {
    labels: ['Pendente', 'Aguardando Ajustes'],
    data: [3, 2],
  },
  topItensPorQuantidade: [
    { itemId: 1, nome: 'Item A', catMat: '100', valor: 120 },
    { itemId: 2, nome: 'Item B', catMat: '200', valor: 80 },
  ],
  topItensPorValorTotal: [
    { itemId: 1, nome: 'Item A', catMat: '100', valor: 60000 },
    { itemId: 2, nome: 'Item B', catMat: '200', valor: 30000 },
  ],
}

const dashboardEmptyPayload = {
  kpis: {
    valorTotalEstimado: 0,
    custoMedioSolicitacao: 0,
    totalItensUnicos: 0,
    totalUnidadesSolicitadas: 0,
    totalDepartamentosSolicitantes: 0,
    totalSolicitacoes: 0,
  },
  valorPorDepartamento: {
    labels: [],
    data: [],
  },
  valorPorCategoria: {
    labels: [],
    data: [],
  },
  visaoGeralStatus: {
    labels: [],
    data: [],
  },
  topItensPorQuantidade: [],
  topItensPorValorTotal: [],
}

describe('Gestor - Dashboard regras críticas', () => {
  it('deve bloquear acesso para solicitante (unauthorized)', () => {
    cy.loginSession('solicitante')
    cy.visit('/gestor/dashboard')
    cy.url().should('include', '/unauthorized')
  })

  it('deve carregar dashboard para gestor com KPIs e gráficos', () => {
    cy.loginSession('gestor')
    cy.intercept('GET', '**/api/dashboard', {
      statusCode: 200,
      body: dashboardSuccessPayload,
    }).as('getDashboard')

    cy.visit('/gestor/dashboard')
    cy.wait('@getDashboard')

    cy.contains('h2', 'Painel do Gestor').should('be.visible')
    cy.contains('span', 'Total de Solicitações').should('be.visible')
    cy.contains('span', 'Departamentos Solicitantes').should('be.visible')
    cy.contains('span', 'Valor Total Estimado').should('be.visible')
    cy.contains('span', 'Custo Médio por Solicitação').should('be.visible')

    cy.contains('span', 'Itens De Maior Valor').should('exist')
    cy.contains('span', 'Itens Mais Solicitados').should('exist')
    cy.contains('span', 'Valor por Departamento').should('exist')
    cy.contains('span', 'Status das Solicitações').should('exist')

    cy.get('canvas').should('have.length.at.least', 4)
    cy.contains('R$').should('exist')
    cy.contains('42').should('exist')
  })

  it('deve exibir loading durante carregamento do dashboard', () => {
    cy.loginSession('gestor')
    cy.intercept('GET', '**/api/dashboard', (req) => {
      req.reply({
        delay: 800,
        statusCode: 200,
        body: dashboardSuccessPayload,
      })
    }).as('getDashboard')

    cy.visit('/gestor/dashboard')
    cy.get('.p-progressspinner').should('exist')
    cy.wait('@getDashboard')
    cy.get('.p-progressspinner').should('not.exist')
  })

  it('deve manter a tela estável quando API do dashboard falhar', () => {
    cy.loginSession('gestor')
    cy.intercept('GET', '**/api/dashboard', {
      statusCode: 500,
      body: { message: 'Erro ao carregar dashboard' },
    }).as('getDashboard')

    cy.visit('/gestor/dashboard')
    cy.wait('@getDashboard')

    cy.contains('h2', 'Painel do Gestor').should('be.visible')
    cy.contains('span', 'Itens De Maior Valor').should('be.visible')
    cy.get('.p-progressspinner').should('not.exist')
  })

  it('deve renderizar dashboard sem dados sem quebrar interface', () => {
    cy.loginSession('gestor')
    cy.intercept('GET', '**/api/dashboard', {
      statusCode: 200,
      body: dashboardEmptyPayload,
    }).as('getDashboard')

    cy.visit('/gestor/dashboard')
    cy.wait('@getDashboard')

    cy.contains('h2', 'Painel do Gestor').should('be.visible')
    cy.contains('span', 'Itens De Maior Valor').should('be.visible')
    cy.contains('Sem dados para exibir.').should('exist')
    cy.contains('Exibindo: R$').should('exist')
  })
})
