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

const openSelect = (inputId: string) => {
  cy.get(`#${inputId}`).parents('.p-select').first().click({ force: true })
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

  it('deve enviar statusId numérico ao filtrar por status válido', () => {
    mockTransparencyDependencies()

    cy.visit('/transparencia')
    cy.wait('@getPublicSolicitations')

    openSelect('status')
    cy.contains('.p-select-option', 'Pendente').click()
    cy.contains('button', 'Buscar').click()

    cy.wait('@getPublicSolicitations').then(({ request }) => {
      expect(request.query.statusId).to.eq('1')
    })
  })

  it('deve listar opções completas de departamento e categoria nos selects', () => {
    mockTransparencyDependencies()

    cy.visit('/transparencia')
    cy.wait('@getDepartments')
    cy.wait('@getCategories')

    openSelect('sigla-departamento')
    cy.contains('.p-select-option', 'Departamento de Computação').should('be.visible')
    cy.get('body').click(0, 0)

    openSelect('categoria')
    cy.contains('.p-select-option', 'Diversos').should('be.visible')
    cy.contains('.p-select-option', 'Mobiliário').should('be.visible')
  })

  it('deve limpar filtros e restaurar query padrão', () => {
    mockTransparencyDependencies()

    cy.visit('/transparencia?statusId=1&itemNome=Teclado&catMat=CAT-123&pageNumber=2&pageSize=10')
    cy.wait('@getPublicSolicitations')

    cy.contains('button', 'Limpar').click()

    cy.url().should('include', 'somenteSolicitacoesAtivas=true')
    cy.url().should('include', 'pageNumber=1')
    cy.url().should('include', 'pageSize=25')
    cy.url().should('not.include', 'statusId=')
    cy.url().should('not.include', 'itemNome=')
    cy.url().should('not.include', 'catMat=')

    cy.wait('@getPublicSolicitations').then(({ request }) => {
      expect(request.query.statusId).to.eq(undefined)
      expect(request.query.itemNome).to.eq(undefined)
      expect(request.query.catMat).to.eq(undefined)
      expect(request.query.pageNumber).to.eq('1')
      expect(request.query.pageSize).to.eq('25')
    })
  })

  it('deve exibir estado sem resultados quando API retornar lista vazia', () => {
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
      body: {
        pageNumber: 1,
        pageSize: 25,
        totalCount: 0,
        totalPages: 0,
        totalItensSolicitados: 0,
        valorTotalSolicitado: 0,
        data: [],
      },
    }).as('getPublicSolicitationsEmpty')

    cy.visit('/transparencia?itemNome=inexistente')

    cy.wait('@getPublicSolicitationsEmpty')
    cy.contains('Nenhum resultado encontrado para os filtros aplicados.').should('be.visible')
  })

  it('deve exibir mensagem de erro quando a consulta pública falhar', () => {
    cy.intercept('GET', '**/api/departamento*', {
      statusCode: 200,
      body: departments,
    }).as('getDepartments')

    cy.intercept('GET', '**/api/categoria*', {
      statusCode: 200,
      body: categories,
    }).as('getCategories')

    cy.intercept('GET', '**/api/dados-publicos/solicitacoes*', {
      statusCode: 500,
      body: { message: 'Erro interno' },
    }).as('getPublicSolicitationsError')

    cy.visit('/transparencia')

    cy.wait('@getPublicSolicitationsError')
    cy.contains('Não foi possível carregar os dados públicos no momento.').should('be.visible')
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

  it('deve exportar mantendo filtros ativos na requisição', () => {
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
    }).as('publicDataWithFiltersExport')

    cy.visit(
      '/transparencia?statusId=1&siglaDepartamento=DCOMP&categoriaNome=Diversos&itemNome=Teclado&itemsType=geral&valorMinimo=100&valorMaximo=500',
    )
    cy.wait('@publicDataWithFiltersExport')

    cy.contains('button', 'Exportar').click()
    cy.wait('@publicDataWithFiltersExport').then(({ request }) => {
      expect(request.query.formatoArquivo).to.eq('csv')
      expect(request.query.statusId).to.eq('1')
      expect(request.query.siglaDepartamento).to.eq('DCOMP')
      expect(request.query.categoriaNome).to.eq('Diversos')
      expect(request.query.itemNome).to.eq('Teclado')
      expect(request.query.itemsType).to.eq('geral')
      expect(request.query.valorMinimo).to.eq('100')
      expect(request.query.valorMaximo).to.eq('500')
    })

    cy.get('.p-splitbutton .p-splitbutton-dropdown').click()
    cy.contains('JSON (.json)').click()

    cy.wait('@publicDataWithFiltersExport').then(({ request }) => {
      expect(request.query.formatoArquivo).to.eq('json')
      expect(request.query.statusId).to.eq('1')
      expect(request.query.siglaDepartamento).to.eq('DCOMP')
      expect(request.query.categoriaNome).to.eq('Diversos')
      expect(request.query.itemNome).to.eq('Teclado')
      expect(request.query.itemsType).to.eq('geral')
      expect(request.query.valorMinimo).to.eq('100')
      expect(request.query.valorMaximo).to.eq('500')
    })
  })
})
