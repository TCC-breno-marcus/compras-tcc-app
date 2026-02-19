/**
 * @fileoverview Comandos customizados do Cypress para suporte a testes E2E.
 * Inclui gerenciamento de sessão de autenticação multi-perfil e utilitários de formulário.
 */

declare global {
  namespace Cypress {
    interface Chainable {
      /**
       * Realiza o login programático via API e gerencia a sessão do usuário.
       * Utiliza `cy.session` para persistir o estado de autenticação entre testes,
       * injetando o token e dados do usuário diretamente no localStorage.
       * * @param {('solicitante' | 'gestor' | 'admin')} [perfil='solicitante'] - O perfil de acesso desejado.
       * @example
       * cy.loginSession('admin')
       */
      loginSession(perfil?: 'solicitante' | 'gestor' | 'admin'): Chainable<void>

      /**
       * Utilitário para interagir com campos de entrada numérica (PrimeVue ou padrão).
       * Limpa o valor existente e insere um novo número, garantindo o disparo de eventos de blur.
       * * @param {string} selector - Seletor CSS do elemento de input.
       * @param {number} index - Índice do elemento (caso existam múltiplos na tela).
       * @param {number} value - O valor numérico a ser digitado.
       * @example
       * cy.fillNumericInput('input#quantidade', 0, 10)
       */
      fillNumericInput(selector: string, index: number, value: number): Chainable<void>

      /**
       * Mocka categorias e catálogo com um seed fixo para testes E2E.
       * Útil para ambientes sem base de itens populada.
       */
      mockCatalogSeedData(): Chainable<void>
    }
  }
}

type CatalogImportSeed = {
  nome: string
  descricao: string
  codigo: string
  especificacao: string
  link_imagem: string
  categoria_id: number
}

/**
 * Comando para autenticação persistente baseada em perfis.
 * Realiza requisições POST para /login e GET para /me para obter o estado completo do usuário.
 */
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
      // 1. Obter Token de Acesso
      cy.request({
        method: 'POST',
        url: '/api/auth/login',
        body: { email, password: senha },
      }).then(({ body }) => {
        const token = body.token as string

        // 2. Obter Detalhes do Usuário
        cy.request({
          method: 'GET',
          url: '/api/auth/me',
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }).then(({ body: user }) => {
          // 3. Persistir na Store da Aplicação (Pinia/LocalStorage)
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
      /**
       * Valida se a sessão ainda é válida tentando acessar uma rota protegida.
       */
      validate() {
        cy.visit('/solicitacoes/criar/geral')
        cy.url().should('include', '/solicitacoes/criar/geral')
      },
      cacheAcrossSpecs: true,
    },
  )
})

/**
 * Preenche campos numéricos garantindo a limpeza prévia do valor.
 * Útil para componentes de input do PrimeVue que podem ter máscaras ou comportamentos específicos.
 */
Cypress.Commands.add('fillNumericInput', (selector, index, value) => {
  cy.get(selector)
    .eq(index)
    .should('be.visible')
    .click()
    .type('{selectall}{backspace}')
    .type(`${value}`)
    .blur()
})

Cypress.Commands.add('mockCatalogSeedData', () => {
  const categories = [
    { id: 1, nome: 'Componentes Eletrônicos', descricao: 'Componentes Eletrônicos', isActive: true },
    { id: 2, nome: 'Eletrodomésticos', descricao: 'Eletrodomésticos', isActive: true },
    { id: 3, nome: 'Ferramentas', descricao: 'Ferramentas', isActive: true },
    { id: 4, nome: 'Diversos', descricao: 'Diversos', isActive: true },
    { id: 5, nome: 'Materiais de Laboratório', descricao: 'Materiais de Laboratório', isActive: true },
    { id: 6, nome: 'Mobiliário', descricao: 'Mobiliário', isActive: true },
    { id: 7, nome: 'Reagentes Químicos', descricao: 'Reagentes Químicos', isActive: true },
  ]

  cy.fixture<CatalogImportSeed[]>('catalog_import_seed.json').then((seed) => {
    const items = seed.map((entry, index) => {
      const category = categories.find((item) => item.id === entry.categoria_id) || categories[0]
      return {
        id: index + 1,
        nome: entry.nome,
        catMat: entry.codigo,
        descricao: entry.descricao,
        especificacao: entry.especificacao || '',
        categoria: category,
        linkImagem: entry.link_imagem || '',
        precoSugerido: 100,
        isActive: true,
      }
    })

    cy.intercept('GET', '**/api/categoria*', (req) => {
      const url = new URL(req.url)
      const nameFilters = url.searchParams.getAll('nome').map((item) => item.toLowerCase())
      const filteredCategories =
        nameFilters.length === 0
          ? categories
          : categories.filter((category) => nameFilters.includes(category.nome.toLowerCase()))

      req.reply({
        statusCode: 200,
        body: filteredCategories,
      })
    }).as('getCategories')

    cy.intercept('GET', /\/api\/catalogo(\?.*)?$/, (req) => {
      const url = new URL(req.url)
      const categoryIds = url.searchParams
        .getAll('categoriaId')
        .map((item) => Number(item))
        .filter((item) => Number.isFinite(item))

      const searchTerm = (url.searchParams.get('searchTerm') || '').toLowerCase()
      const isActiveFilter = url.searchParams.get('isActive')
      const pageNumber = Number(url.searchParams.get('pageNumber') || 1)
      const pageSize = Number(url.searchParams.get('pageSize') || 50)

      let filteredItems = [...items]

      if (categoryIds.length > 0) {
        filteredItems = filteredItems.filter((item) => categoryIds.includes(item.categoria.id))
      }

      if (searchTerm) {
        filteredItems = filteredItems.filter(
          (item) =>
            item.nome.toLowerCase().includes(searchTerm) ||
            item.catMat.toLowerCase().includes(searchTerm) ||
            item.descricao.toLowerCase().includes(searchTerm),
        )
      }

      if (isActiveFilter === 'true') {
        filteredItems = filteredItems.filter((item) => item.isActive)
      } else if (isActiveFilter === 'false') {
        filteredItems = filteredItems.filter((item) => !item.isActive)
      }

      const start = (pageNumber - 1) * pageSize
      const paginated = filteredItems.slice(start, start + pageSize)
      const totalCount = filteredItems.length
      const totalPages = Math.ceil(totalCount / pageSize) || 1

      req.reply({
        statusCode: 200,
        body: {
          pageNumber,
          pageSize,
          totalCount,
          totalPages,
          data: paginated,
        },
      })
    }).as('getCatalog')
  })
})

export {}
