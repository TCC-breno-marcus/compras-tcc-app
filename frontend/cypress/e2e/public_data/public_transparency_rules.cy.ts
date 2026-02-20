const departments = [
  {
    id: 1,
    nome: 'Departamento de Computação',
    sigla: 'DCOMP',
    email: 'dcomp@ufs.br',
    telefone: '79999999999',
    tipo: 'Departamento',
  },
]

const categories = [
  { id: 1, nome: 'Diversos', descricao: 'Diversos', isActive: true },
  { id: 2, nome: 'Mobiliário', descricao: 'Mobiliário', isActive: true },
]

const publicDataResponse = {
  pageNumber: 1,
  pageSize: 25,
  totalCount: 1,
  totalPages: 1,
  totalItensSolicitados: 3,
  valorTotalSolicitado: 500,
  data: [
    {
      id: 999,
      externalId: 'PUB-0001',
      dataCriacao: '2026-02-01T10:00:00Z',
      tipoSolicitacao: 'GERAL',
      statusId: 1,
      statusNome: 'Pendente',
      solicitanteNomeMascarado: 'J*** D***',
      solicitanteEmailMascarado: 'jo***@email.com',
      solicitanteTelefoneMascarado: '(79) 9****-1234',
      solicitanteCpfMascarado: '***.***.***-**',
      departamentoNome: 'Departamento de Computação',
      departamentoSigla: 'DCOMP',
      valorTotalSolicitacao: 500,
      itens: [
        {
          itemId: 1,
          itemNome: 'Teclado',
          catMat: 'CAT-123',
          categoriaNome: 'Diversos',
          quantidade: 1,
          valorUnitario: 500,
          valorTotal: 500,
          justificativa: 'Reposição',
        },
      ],
    },
  ],
}

const mockTransparencyDependencies = () => {
  cy.intercept('GET', '**/api/departamento*', {
    statusCode: 200,
    body: departments,
  }).as('getDepartments')

  cy.intercept('GET', '**/api/categoria*', {
    statusCode: 200,
    body: categories,
  }).as('getCategories')

  cy.intercept('GET', '**/api/dados-publicos/solicitacoes*', {
    statusCode: 200,
    body: publicDataResponse,
  }).as('getPublicSolicitations')
}

describe('Portal de Transparência - regras públicas', () => {
  it('deve permitir acesso pela tela de login sem autenticação', () => {
    mockTransparencyDependencies()

    cy.visit('/login')
    cy.contains('button', 'Acessar dados públicos').click()

    cy.url().should('include', '/transparencia')
    cy.contains('h1', 'Portal de Transparência').should('be.visible')
    cy.contains('button', 'Entrar').should('be.visible')

    cy.wait('@getDepartments')
    cy.wait('@getCategories')
    cy.wait('@getPublicSolicitations')
    cy.contains('PUB-0001').should('be.visible')
  })

  it('não deve enviar statusId quando query vier como "Todos os status"', () => {
    mockTransparencyDependencies()

    cy.visit('/transparencia?statusId=Todos+os+status')

    cy.wait('@getPublicSolicitations').then(({ request }) => {
      expect(request.query.statusId).to.eq(undefined)
    })
  })

  it('deve exportar CSV e JSON no splitbutton', () => {
    mockTransparencyDependencies()

    cy.intercept('GET', '**/api/dados-publicos/solicitacoes*', (req) => {
      if (req.query.formatoArquivo === 'csv') {
        req.reply({
          statusCode: 200,
          headers: {
            'content-type': 'text/csv;charset=utf-8',
          },
          body: 'id,externalId\n999,PUB-0001',
        })
        return
      }

      if (req.query.formatoArquivo === 'json') {
        req.reply({
          statusCode: 200,
          body: publicDataResponse,
        })
        return
      }

      req.reply({
        statusCode: 200,
        body: publicDataResponse,
      })
    }).as('publicDataWithExport')

    cy.visit('/transparencia')
    cy.wait('@publicDataWithExport')

    cy.contains('button', 'Exportar').click()
    cy.wait('@publicDataWithExport').then(({ request }) => {
      expect(request.query.formatoArquivo).to.eq('csv')
    })
    cy.contains('Arquivo CSV exportado com sucesso.').should('exist')

    cy.get('.p-splitbutton .p-splitbutton-dropdown').click()
    cy.contains('JSON (.json)').click()

    cy.wait('@publicDataWithExport').then(({ request }) => {
      expect(request.query.formatoArquivo).to.eq('json')
    })
    cy.contains('Arquivo JSON exportado com sucesso.').should('exist')
  })
})
