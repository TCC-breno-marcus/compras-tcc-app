type CatalogItem = {
  id: number
  nome: string
  catMat: string
  descricao: string
  especificacao: string
  categoria: {
    id: number
    nome: string
    descricao: string
    isActive: boolean
  }
  linkImagem: string
  precoSugerido: number
  isActive: boolean
}

type ItemHistoryEvent = {
  id: string
  dataOcorrencia: string
  acao: string
  detalhes: string | null
  observacoes: string | null
  nomePessoa: string
}

const baseCategories = [
  { id: 1, nome: 'Diversos', descricao: 'Diversos', isActive: true },
  { id: 2, nome: 'Ferramentas', descricao: 'Ferramentas', isActive: true },
]

const seedItems = (): CatalogItem[] => [
  {
    id: 1,
    nome: 'Item Inicial',
    catMat: '100001',
    descricao: 'Descrição do item inicial',
    especificacao: 'Especificação A',
    categoria: baseCategories[0],
    linkImagem: '',
    precoSugerido: 90,
    isActive: true,
  },
  {
    id: 2,
    nome: 'Outro Item',
    catMat: '100002',
    descricao: 'Descrição do outro item',
    especificacao: 'Especificação B',
    categoria: baseCategories[1],
    linkImagem: '',
    precoSugerido: 150,
    isActive: false,
  },
]

const setupCatalogState = () => {
  const state = {
    items: seedItems(),
    nextId: 3,
    history: {
      1: [
        {
          id: 'h-1',
          dataOcorrencia: '2026-02-01T10:00:00.000Z',
          acao: 'Criacao',
          detalhes: "Item criado com nome 'Item Inicial'",
          observacoes: null,
          nomePessoa: 'Gestor Padrão',
        },
      ] as ItemHistoryEvent[],
      2: [] as ItemHistoryEvent[],
    } as Record<number, ItemHistoryEvent[]>,
  }

  cy.intercept('GET', '**/api/categoria*', {
    statusCode: 200,
    body: baseCategories,
  }).as('getCategories')

  cy.intercept('GET', /\/api\/catalogo(\?.*)?$/, (req) => {
    const url = new URL(req.url)
    const searchTerm = (url.searchParams.get('searchTerm') || '').toLowerCase()
    const isActiveFilter = url.searchParams.get('isActive')

    let data = [...state.items]
    if (searchTerm) {
      data = data.filter(
        (item) =>
          item.nome.toLowerCase().includes(searchTerm) ||
          item.catMat.toLowerCase().includes(searchTerm),
      )
    }

    if (isActiveFilter === 'true') {
      data = data.filter((item) => item.isActive)
    } else if (isActiveFilter === 'false') {
      data = data.filter((item) => !item.isActive)
    }

    req.reply({
      statusCode: 200,
      body: {
        pageNumber: 1,
        pageSize: 50,
        totalCount: data.length,
        totalPages: 1,
        data,
      },
    })
  }).as('getCatalog')

  cy.intercept('GET', /\/api\/catalogo\/\d+\/itens-semelhantes$/, {
    statusCode: 200,
    body: [],
  }).as('getSimilarItems')

  cy.intercept('GET', /\/api\/catalogo\/\d+\/historico$/, (req) => {
    const id = Number(req.url.match(/catalogo\/(\d+)\/historico/)?.[1])
    req.reply({
      statusCode: 200,
      body: state.history[id] || [],
    })
  }).as('getItemHistory')

  cy.intercept('GET', /\/api\/catalogo\/\d+$/, (req) => {
    const id = Number(req.url.match(/catalogo\/(\d+)$/)?.[1])
    const item = state.items.find((i) => i.id === id)
    if (!item) {
      req.reply({ statusCode: 404, body: { message: 'Item não encontrado.' } })
      return
    }
    req.reply({ statusCode: 200, body: item })
  }).as('getItemById')

  cy.intercept('POST', '**/api/catalogo', (req) => {
    const category = baseCategories.find((c) => c.id === req.body.categoriaId) || baseCategories[0]
    const created: CatalogItem = {
      id: state.nextId++,
      nome: req.body.nome,
      catMat: req.body.catMat,
      descricao: req.body.descricao,
      especificacao: req.body.especificacao || '',
      categoria: category,
      linkImagem: '',
      precoSugerido: req.body.precoSugerido || 0,
      isActive: !!req.body.isActive,
    }
    state.items.unshift(created)
    state.history[created.id] = [
      {
        id: `h-${Date.now()}`,
        dataOcorrencia: '2026-02-02T10:00:00.000Z',
        acao: 'Criacao',
        detalhes: `Item criado com nome '${created.nome}'`,
        observacoes: null,
        nomePessoa: 'Gestor Padrão',
      },
    ]
    req.reply({ statusCode: 200, body: created })
  }).as('createItem')

  cy.intercept('PUT', /\/api\/catalogo\/\d+$/, (req) => {
    const id = Number(req.url.match(/catalogo\/(\d+)$/)?.[1])
    const index = state.items.findIndex((i) => i.id === id)
    if (index < 0) {
      req.reply({ statusCode: 404, body: { message: 'Item não encontrado.' } })
      return
    }
    const current = state.items[index]
    const newCategory =
      baseCategories.find((c) => c.id === req.body.categoriaId) || current.categoria
    const updated: CatalogItem = {
      ...current,
      nome: req.body.nome ?? current.nome,
      catMat: req.body.catMat ?? current.catMat,
      descricao: req.body.descricao ?? current.descricao,
      especificacao: req.body.especificacao ?? current.especificacao,
      precoSugerido: req.body.precoSugerido ?? current.precoSugerido,
      isActive: req.body.isActive ?? current.isActive,
      categoria: newCategory,
    }
    state.items[index] = updated
    state.history[id] = [
      ...(state.history[id] || []),
      {
        id: `h-${Date.now()}`,
        dataOcorrencia: '2026-02-03T11:00:00.000Z',
        acao: 'Edicao',
        detalhes: `Item atualizado para '${updated.nome}'`,
        observacoes: null,
        nomePessoa: 'Gestor Padrão',
      },
    ]
    req.reply({ statusCode: 200, body: updated })
  }).as('editItem')

  cy.intercept('DELETE', /\/api\/catalogo\/\d+$/, (req) => {
    const id = Number(req.url.match(/catalogo\/(\d+)$/)?.[1])
    state.items = state.items.filter((i) => i.id !== id)
    req.reply({
      statusCode: 200,
      body: { message: 'Item excluído com sucesso.' },
    })
  }).as('deleteItem')

  return state
}

const visitCatalog = () => {
  cy.visit('/gestor/catalogo')
  cy.wait('@getCatalog')
}

const selectCategory = (name: string, dialogAlias = '@createDialog') => {
  cy.get(dialogAlias)
    .find('#categoria-filter, [inputid="categoria-filter"]')
    .first()
    .scrollIntoView()
    .click({ force: true })

  cy.get('body').contains('.p-select-option', name, { timeout: 8000 }).click({ force: true })
}

const openItemDetailsByName = (name: string) => {
  cy.contains('.item-card', name)
    .first()
    .find('.image-preview-container')
    .click({ force: true })
  cy.wait('@getItemById')
}

describe('Gestor - Gerenciar Catálogo (regras críticas)', () => {
  it('deve bloquear acesso para solicitante', () => {
    cy.loginSession('solicitante')
    cy.visit('/gestor/catalogo')
    cy.url().should('include', '/unauthorized')
  })

  it('deve criar item sem imagem com sucesso', () => {
    cy.loginSession('gestor')
    setupCatalogState()
    visitCatalog()

    const newName = `Item criado e2e ${Date.now()}`
    const newCatMat = '123456'

    cy.contains('button', 'Criar').click()
    cy.get('.p-dialog:visible').as('createDialog')
    cy.get('@createDialog').contains('.p-dialog-header', 'Criar Novo Item').should('be.visible')

    cy.get('@createDialog').contains('.p-dialog-footer button', 'Criar').should('be.disabled')

    cy.get('@createDialog').find('input#nome', { timeout: 8000 }).should('be.visible').type(newName)
    cy.get('@createDialog').find('input#catMat', { timeout: 8000 }).should('be.visible').type(newCatMat)
    cy.get('@createDialog')
      .find('textarea#descricao, textarea[inputid="descricao"]')
      .first()
      .type('Descrição do item criado via e2e.')

    selectCategory('Diversos', '@createDialog')
    cy.get('@createDialog').find('input#precoSugerido, input[inputid="precoSugerido"]').first().click()
    cy.get('@createDialog')
      .find('input#precoSugerido, input[inputid="precoSugerido"]')
      .first()
      .type('{selectall}{backspace}200')
      .blur()

    cy.get('@createDialog').contains('.p-dialog-footer button', 'Criar').click()
    cy.wait('@createItem').then(({ request, response }) => {
      expect(response?.statusCode).to.eq(200)
      expect(request.body.nome).to.eq(newName)
      expect(request.body.catMat).to.eq(newCatMat)
      expect(request.body.linkImagem).to.eq('')
    })

    cy.contains('O item foi criado com sucesso.').should('exist')
    cy.wait('@getCatalog')
    cy.get('input#simple-search, input[inputid="simple-search"]').first().clear().type(newCatMat)
    cy.contains('button', 'Buscar').click()
    cy.wait('@getCatalog').then(({ request, response }) => {
      expect(request.query.searchTerm).to.eq(newCatMat)
      expect(response?.statusCode).to.eq(200)
      expect(response?.body?.data?.some((item: CatalogItem) => item.catMat === newCatMat)).to.eq(true)
    })
    cy.contains('.item-card', `CATMAT ${newCatMat}`).should('exist')
  })

  it('deve editar item e refletir no histórico', () => {
    cy.loginSession('gestor')
    setupCatalogState()
    visitCatalog()

    openItemDetailsByName('Item Inicial')
    cy.wait('@getSimilarItems')
    cy.wait('@getItemHistory')

    cy.contains('button', 'Editar').click()
    cy.get('#nome').clear().type('Item Inicial Editado')
    cy.fillNumericInput('input#precoSugerido', 0, 333)
    cy.contains('button', 'Salvar').click()
    cy.get('.p-confirmdialog').contains('button', 'Salvar').click()

    cy.wait('@editItem').then(({ request, response }) => {
      expect(response?.statusCode).to.eq(200)
      expect(request.body.nome).to.eq('Item Inicial Editado')
      expect(request.body.precoSugerido).to.eq(333)
    })
    cy.contains('O item foi salvo com sucesso.').should('exist')

    cy.contains('[role="tab"]', 'Histórico').click()
    cy.wait('@getItemHistory')
    cy.contains('Item atualizado para').should('exist')
    cy.contains('Item Inicial Editado').should('exist')
  })

  it('deve validar campos obrigatórios na edição e bloquear save inválido', () => {
    cy.loginSession('gestor')
    setupCatalogState()
    visitCatalog()

    openItemDetailsByName('Item Inicial')
    cy.contains('button', 'Editar').click()
    cy.get('#nome').clear()
    cy.contains('button', 'Salvar').click()
    cy.get('.p-confirmdialog').should('not.exist')
    cy.contains('O campo "Nome" não pode ser vazio.').should('exist')
  })

  it('deve filtrar catálogo por termo de busca', () => {
    cy.loginSession('gestor')
    setupCatalogState()
    visitCatalog()

    cy.get('input#simple-search, input[inputid="simple-search"]').first().type('Outro')
    cy.contains('button', 'Buscar').click()
    cy.wait('@getCatalog').then(({ request }) => {
      expect(request.query.searchTerm).to.eq('Outro')
    })
    cy.contains('.item-card', 'Outro Item').should('exist')
    cy.contains('.item-card', 'Item Inicial').should('not.exist')

    cy.contains('button', 'Limpar').first().click()
    cy.wait('@getCatalog')
    cy.contains('.item-card', 'Item Inicial').should('exist')
    cy.contains('.item-card', 'Outro Item').should('exist')
  })

  it('deve excluir item com sucesso', () => {
    cy.loginSession('gestor')
    setupCatalogState()
    visitCatalog()

    openItemDetailsByName('Outro Item')
    cy.contains('button', 'Editar').click()
    cy.contains('button', 'Excluir').click()
    cy.get('.p-confirmdialog').contains('button', 'Excluir').click()

    cy.wait('@deleteItem').its('response.statusCode').should('eq', 200)
    cy.contains('Item excluído com sucesso.').should('exist')
    cy.wait('@getCatalog')
    cy.contains('.item-card', 'Outro Item').should('not.exist')
  })

  it('deve tratar erro de API ao salvar edição', () => {
    cy.loginSession('gestor')
    setupCatalogState()
    cy.intercept('PUT', /\/api\/catalogo\/\d+$/, {
      statusCode: 500,
      body: { message: 'Erro interno ao atualizar item.' },
    }).as('editItemError')
    visitCatalog()

    openItemDetailsByName('Item Inicial')
    cy.contains('button', 'Editar').click()
    cy.get('#nome').clear().type('Item com erro')
    cy.contains('button', 'Salvar').click()
    cy.get('.p-confirmdialog').contains('button', 'Salvar').click()

    cy.wait('@editItemError')
    cy.contains('Não foi possível salvar as alterações.').should('exist')
    cy.contains('button', 'Cancelar').should('be.visible')
  })
})
