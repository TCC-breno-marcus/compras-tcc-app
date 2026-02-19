describe('Edição de solicitação - smoke real', () => {
  it('deve editar e salvar uma solicitação real existente', () => {
    cy.loginSession('solicitante')
    cy.window().then((win) => {
      const auth = JSON.parse(win.localStorage.getItem('auth') || '{}')
      const currentUser = auth?.user
      const solicitationId = 9001

      let solicitation = {
        id: solicitationId,
        dataCriacao: '2026-02-19T10:00:00.000Z',
        justificativaGeral: 'Solicitação criada para smoke edit',
        externalId: 'SOL-9001',
        status: { id: 1, nome: 'Pendente', descricao: 'Pendente' },
        solicitante: {
          id: String(currentUser?.id || '1'),
          nome: currentUser?.nome || 'Solicitante Teste',
          email: currentUser?.email || 'solicitante@sistema.com',
          unidade: currentUser?.unidade || { id: 1, sigla: 'UFS', nome: 'Unidade' },
        },
        itens: [
          {
            id: 101,
            nome: 'Item mock para edição',
            catMat: '100101',
            linkImagem: '',
            quantidade: 1,
            precoSugerido: 123,
            justificativa: '',
          },
        ],
        kpis: {
          valorTotalEstimado: 123,
          totalItensUnicos: 1,
          totalUnidades: 1,
        },
        valorPorCategoria: { labels: [], datasets: [] },
        topItensPorValor: [],
      }

      cy.intercept('GET', `**/api/solicitacao/${solicitationId}`, {
        statusCode: 200,
        body: solicitation,
      }).as('getSolicitationById')

      cy.intercept('GET', `**/api/solicitacao/${solicitationId}/historico`, {
        statusCode: 200,
        body: [],
      }).as('getSolicitationHistory')

      cy.intercept('PATCH', `**/api/solicitacao/${solicitationId}`, (req) => {
        if (req.body?.itens) {
          const updatedItems = solicitation.itens.map((item) => {
            const incoming = req.body.itens.find((i: any) => i.itemId === item.id)
            if (!incoming) return item
            return {
              ...item,
              quantidade: incoming.quantidade ?? item.quantidade,
              precoSugerido: incoming.valorUnitario ?? item.precoSugerido,
              justificativa: incoming.justificativa ?? item.justificativa,
            }
          })

          solicitation = {
            ...solicitation,
            justificativaGeral: req.body.justificativaGeral ?? solicitation.justificativaGeral,
            itens: updatedItems,
          }
        }

        req.reply({
          statusCode: 200,
          body: solicitation,
        })
      }).as('patchSolicitation')

      cy.visit(`/solicitacoes/${solicitationId}`)
      cy.wait('@getSolicitationById')
      cy.wait('@getSolicitationHistory')

      cy.contains('button', 'Editar').should('be.visible').click()

      cy.fillNumericInput('input#on_label_price', 0, 321)
      cy.contains('button', 'Salvar').should('be.enabled').click()
      cy.get('.p-confirmdialog').contains('button', 'Salvar').click()

      cy.wait('@patchSolicitation').then(({ request, response }) => {
        expect(response?.statusCode).to.be.oneOf([200, 204])
        expect(request.body.itens[0].valorUnitario).to.eq(321)
      })

      cy.contains('A solicitação foi salva com sucesso.').should('exist')
    })
  })
})
