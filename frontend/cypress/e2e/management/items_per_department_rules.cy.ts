const baseDepartments = [
  {
    id: 1,
    nome: 'Departamento de Computação',
    sigla: 'DCOMP',
    email: 'dcomp@ufs.br',
    telefone: '79999999999',
    tipo: 'Departamento',
  },
]

const baseCategories = [
  {
    id: 1,
    nome: 'Diversos',
    descricao: 'Diversos',
    isActive: true,
  },
]

const buildItemsDepartmentResponse = (withDuplicateDepartment = false) => {
  const deptDemand = [
    {
      unidade: baseDepartments[0],
      quantidadeTotal: 4,
      justificativa: 'Uso em laboratório.',
    },
  ]

  if (withDuplicateDepartment) {
    deptDemand.push({
      unidade: baseDepartments[0],
      quantidadeTotal: 2,
      justificativa: 'Duplicado proposital para regressão.',
    })
  }

  return {
    pageNumber: 1,
    pageSize: 50,
    totalCount: 1,
    totalPages: 1,
    data: [
      {
        id: 101,
        nome: 'Item de Teste',
        catMat: 'CAT-101',
        linkImagem: '',
        categoriaNome: 'Diversos',
        quantidadeTotalSolicitada: 6,
        valorTotalSolicitado: 600,
        precoMedio: 100,
        precoMinimo: 90,
        precoMaximo: 110,
        numeroDeSolicitacoes: 2,
        demandaPorDepartamento: deptDemand,
      },
    ],
  }
}

const mockBaseEndpoints = (withDuplicateDepartment = false) => {
  cy.intercept('GET', '**/api/departamento*', {
    statusCode: 200,
    body: baseDepartments,
  }).as('getDepartments')

  cy.intercept('GET', '**/api/categoria*', {
    statusCode: 200,
    body: baseCategories,
  }).as('getCategories')

  cy.intercept('GET', '**/api/relatorio/itens-departamento*', {
    statusCode: 200,
    body: buildItemsDepartmentResponse(withDuplicateDepartment),
  }).as('getItemsPerDepartment')
}

const openItemsByDepartment = (withDuplicateDepartment = false) => {
  mockBaseEndpoints(withDuplicateDepartment)
  cy.visit('/gestor/departamento')
  cy.wait('@getItemsPerDepartment')
}

describe('Gestor - Itens por departamento (regras críticas)', () => {
  it('deve bloquear acesso para solicitante (unauthorized)', () => {
    cy.loginSession('solicitante')
    cy.visit('/gestor/departamento')
    cy.url().should('include', '/unauthorized')
  })

  it('não deve exibir departamentos duplicados para o mesmo item', () => {
    cy.loginSession('gestor')
    openItemsByDepartment(true)

    cy.contains('span', 'Detalhes').click()
    cy.contains('p', 'Distribuição por Departamento')
      .parent()
      .within(() => {
        // Considera apenas a linha do departamento (ignora justificativa e labels de outras seções)
        cy.get('li > div > span.text-color-secondary:visible').then(($spans) => {
          const departmentLines = [...$spans]
            .map((el) => el.textContent?.trim() || '')
            .filter((text) => text.length > 0)
          const uniqueDepartmentLines = new Set(departmentLines)

          expect(uniqueDepartmentLines.size).to.eq(departmentLines.length)
        })
      })
  })

  it('deve exportar XLSX com sucesso', () => {
    cy.loginSession('gestor')

    cy.intercept('GET', '**/api/relatorio/itens-departamento/geral/exportar*', {
      statusCode: 200,
      headers: {
        'content-type':
          'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8',
      },
      body: 'excel-content',
    }).as('exportExcel')

    openItemsByDepartment(false)
    cy.contains('button', 'Exportar').click()

    cy.wait('@exportExcel').then(({ request, response }) => {
      expect(request.query.formatoArquivo).to.eq('excel')
      expect(response?.statusCode).to.eq(200)
    })
    cy.contains('Relatório EXCEL exportado com sucesso.').should('exist')
  })

  it('deve exportar CSV com sucesso', () => {
    cy.loginSession('gestor')

    cy.intercept('GET', '**/api/relatorio/itens-departamento/geral/exportar*', {
      statusCode: 200,
      headers: {
        'content-type': 'text/csv;charset=utf-8',
      },
      body: 'item,departamento,quantidade\nItem de Teste,DCOMP,6',
    }).as('exportCsv')

    openItemsByDepartment(false)
    cy.get('.p-splitbutton .p-splitbutton-dropdown').click()
    cy.contains('CSV (.csv)').click()

    cy.wait('@exportCsv').then(({ request, response }) => {
      expect(request.query.formatoArquivo).to.eq('csv')
      expect(response?.statusCode).to.eq(200)
    })
    cy.contains('Relatório CSV exportado com sucesso.').should('exist')
  })
})
