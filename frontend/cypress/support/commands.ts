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
    }
  }
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

export {}