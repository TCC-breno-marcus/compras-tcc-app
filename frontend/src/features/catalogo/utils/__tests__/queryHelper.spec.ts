// filterUtils.test.ts (CORRIGIDO)
import { describe, it, expect, vi, beforeEach } from 'vitest'
import type { LocationQuery, LocationQueryValue } from 'vue-router'
import type { Categoria, CatalogoFilters } from '../../types'
import { mapQueryToFilters, getStatusFromQuery, applyPreFilters } from '../queryHelper'
import { categorysIdFilterPerName } from '../categoriaTransformer'

// Mock dos módulos
vi.mock('@/utils/queryHelper')
vi.mock('../categoriaTransformer', () => ({
  categorysIdFilterPerName: vi.fn(),
}))

describe('getStatusFromQuery', () => {
  it('deve retornar "ativo" quando value é "true"', () => {
    expect(getStatusFromQuery('true')).toBe('ativo')
  })

  it('deve retornar "inativo" quando value é "false"', () => {
    expect(getStatusFromQuery('false')).toBe('inativo')
  })

  it('deve retornar string vazia quando value é null', () => {
    expect(getStatusFromQuery(null)).toBe('')
  })

  it('deve retornar string vazia quando value é string vazia', () => {
    expect(getStatusFromQuery('')).toBe('')
  })

  it('deve retornar string vazia quando value é qualquer outra string', () => {
    expect(getStatusFromQuery('active')).toBe('')
    expect(getStatusFromQuery('1')).toBe('')
    expect(getStatusFromQuery('True')).toBe('')
    expect(getStatusFromQuery('FALSE')).toBe('')
  })

  it('deve retornar string vazia quando array contém "true" (função não trata arrays)', () => {
    expect(getStatusFromQuery(['true', 'false'])).toBe('')
  })

  it('deve retornar string vazia quando array contém "false" (função não trata arrays)', () => {
    expect(getStatusFromQuery(['false', 'true'])).toBe('')
  })

  it('deve retornar string vazia quando array contém valor inválido', () => {
    expect(getStatusFromQuery(['invalid'])).toBe('')
  })

  it('deve retornar string vazia para arrays (função não os processa)', () => {
    expect(getStatusFromQuery(['true', 'invalid'])).toBe('')
    expect(getStatusFromQuery(['false', 'invalid'])).toBe('')
  })
})

describe('mapQueryToFilters', () => {
  let queryHelper: any

  beforeEach(async () => {
    vi.clearAllMocks()
    queryHelper = await import('@/utils/queryHelper')

    // Setup default mocks
    vi.mocked(queryHelper.getFirstQueryValue).mockImplementation(
      (value: LocationQueryValue | LocationQueryValue[]) => {
        if (Array.isArray(value)) {
          return value[0] || ''
        }
        return value || ''
      },
    )
    vi.mocked(queryHelper.getQueryAsArrayOfNumbers).mockReturnValue([])
    vi.mocked(queryHelper.getSortOrderFromQuery).mockReturnValue(null)
  })

  it('deve mapear query vazia para filtros vazios', () => {
    const query: LocationQuery = {}
    const result = mapQueryToFilters(query)

    expect(result).toEqual({
      searchTerm: '',
      nome: '',
      descricao: '',
      catMat: '',
      especificacao: '',
      categoriaId: [],
      status: '',
      sortOrder: null,
      pageNumber: '',
      pageSize: '',
    })
  })

  it('deve mapear todos os campos da query corretamente', () => {
    vi.mocked(queryHelper.getFirstQueryValue).mockImplementation(
      (value: LocationQueryValue | LocationQueryValue[]) => {
        if (value === 'search') return 'search'
        if (value === 'testNome') return 'testNome'
        if (value === 'testDesc') return 'testDesc'
        if (value === '123') return '123'
        if (value === 'testSpec') return 'testSpec'
        if (value === '1') return '1'
        if (value === '10') return '10'
        return ''
      },
    )
    vi.mocked(queryHelper.getQueryAsArrayOfNumbers).mockReturnValue([1, 2, 3])
    vi.mocked(queryHelper.getSortOrderFromQuery).mockReturnValue('asc')

    const query: LocationQuery = {
      searchTerm: 'search',
      nome: 'testNome',
      descricao: 'testDesc',
      catmat: '123',
      especificacao: 'testSpec',
      categoriaId: ['1', '2', '3'],
      isActive: 'true',
      sortOrder: 'asc',
      pageNumber: '1',
      pageSize: '10',
    }

    const result = mapQueryToFilters(query)

    expect(result.categoriaId).toEqual([1, 2, 3])
    expect(result.status).toBe('ativo')
    expect(result.sortOrder).toBe('asc')
  })

  it('deve usar getStatusFromQuery para mapear isActive', () => {
    const query: LocationQuery = {
      isActive: 'false',
    }

    const result = mapQueryToFilters(query)

    expect(result.status).toBe('inativo')
  })

  it('deve lidar com query parcialmente preenchida', () => {
    vi.mocked(queryHelper.getFirstQueryValue).mockImplementation(
      (value: LocationQueryValue | LocationQueryValue[]) => {
        if (value === 'caneta') return 'caneta'
        return ''
      },
    )

    const query: LocationQuery = {
      nome: 'caneta',
    }

    const result = mapQueryToFilters(query)

    expect(result.nome).toBe('caneta')
    expect(result.descricao).toBe('')
    expect(result.catMat).toBe('')
    expect(result.categoriaId).toEqual([])
  })
})

describe('applyPreFilters', () => {
  const createMockCategoria = (id: number, nome: string): Categoria => ({
    id,
    nome,
    descricao: `Descrição ${nome}`,
    isActive: true,
  })

  const mockCategories: Categoria[] = [
    createMockCategoria(1, 'Material de Escritório'),
    createMockCategoria(2, 'Informática'),
    createMockCategoria(3, 'Limpeza'),
    createMockCategoria(4, 'Alimentos'),
  ]

  const createBaseFilters = (): CatalogoFilters => ({
    searchTerm: '',
    nome: '',
    descricao: '',
    catMat: '',
    especificacao: '',
    categoriaId: [],
    status: '',
    sortOrder: null,
    pageNumber: '',
    pageSize: '',
  })

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('deve retornar filtros inalterados quando não há preFilters', () => {
    const filtersFromUrl = createBaseFilters()
    const preFilters = {}

    const result = applyPreFilters(filtersFromUrl, mockCategories, preFilters)

    expect(result).toEqual(filtersFromUrl)
  })

  it('deve aplicar categoryNames quando fornecido e URL não tem categorias', () => {
    // Configura o Mock para retornar [1, 2]
    vi.mocked(categorysIdFilterPerName).mockReturnValue([1, 2])

    const filtersFromUrl = createBaseFilters()
    const preFilters = {
      categoryNames: ['Material de Escritório', 'Informática'],
    }

    const result = applyPreFilters(filtersFromUrl, mockCategories, preFilters)

    // Verifica se a função mockada foi chamada corretamente
    expect(categorysIdFilterPerName).toHaveBeenCalledWith(mockCategories, [
      'Material de Escritório',
      'Informática',
    ])
    expect(result.categoriaId).toEqual([1, 2])
  })

  it('deve calcular interseção quando URL e props têm categorias', () => {
    vi.mocked(categorysIdFilterPerName).mockReturnValue([1, 2, 3])

    const filtersFromUrl: CatalogoFilters = {
      ...createBaseFilters(),
      categoriaId: [2, 3, 4],
    }
    const preFilters = {
      categoryNames: ['Material de Escritório', 'Informática', 'Limpeza'],
    }

    const result = applyPreFilters(filtersFromUrl, mockCategories, preFilters)

    expect(result.categoriaId).toEqual([2, 3])
  })

  it('deve retornar array vazio quando não há interseção entre categorias', () => {
    vi.mocked(categorysIdFilterPerName).mockReturnValue([1, 2])

    const filtersFromUrl: CatalogoFilters = {
      ...createBaseFilters(),
      categoriaId: [3, 4],
    }
    const preFilters = {
      categoryNames: ['Material de Escritório', 'Informática'],
    }

    const result = applyPreFilters(filtersFromUrl, mockCategories, preFilters)

    expect(result.categoriaId).toEqual([])
  })

  it('deve aplicar status quando fornecido', () => {
    const filtersFromUrl = createBaseFilters()
    const preFilters = {
      status: 'ativo',
    }

    const result = applyPreFilters(filtersFromUrl, mockCategories, preFilters)

    expect(result.status).toBe('ativo')
  })

  it('deve sobrescrever status da URL com status das props', () => {
    const filtersFromUrl: CatalogoFilters = {
      ...createBaseFilters(),
      status: 'inativo',
    }
    const preFilters = {
      status: 'ativo',
    }

    const result = applyPreFilters(filtersFromUrl, mockCategories, preFilters)

    expect(result.status).toBe('ativo')
  })

  it('deve aplicar categoryNames e status simultaneamente', () => {
    vi.mocked(categorysIdFilterPerName).mockReturnValue([1, 2])

    const filtersFromUrl = createBaseFilters()
    const preFilters = {
      categoryNames: ['Material de Escritório', 'Informática'],
      status: 'ativo',
    }

    const result = applyPreFilters(filtersFromUrl, mockCategories, preFilters)

    expect(result.categoriaId).toEqual([1, 2])
    expect(result.status).toBe('ativo')
  })

  it('deve manter outros filtros da URL inalterados', () => {
    vi.mocked(categorysIdFilterPerName).mockReturnValue([1])

    const filtersFromUrl: CatalogoFilters = {
      ...createBaseFilters(),
      searchTerm: 'caneta',
      nome: 'teste',
      descricao: 'descrição teste',
      sortOrder: 'asc',
      pageNumber: '1',
      pageSize: '10',
    }
    const preFilters = {
      categoryNames: ['Material de Escritório'],
    }

    const result = applyPreFilters(filtersFromUrl, mockCategories, preFilters)

    expect(result.searchTerm).toBe('caneta')
    expect(result.nome).toBe('teste')
    expect(result.descricao).toBe('descrição teste')
    expect(result.sortOrder).toBe('asc')
    expect(result.pageNumber).toBe('1')
    expect(result.pageSize).toBe('10')
  })

  it('deve não modificar categoriaId quando categoryNames está vazio', () => {
    const filtersFromUrl: CatalogoFilters = {
      ...createBaseFilters(),
      categoriaId: [1, 2, 3],
    }
    const preFilters = {
      categoryNames: [],
    }

    const result = applyPreFilters(filtersFromUrl, mockCategories, preFilters)

    expect(result.categoriaId).toEqual([1, 2, 3])
  })

  it('deve não chamar categorysIdFilterPerName quando categoryNames não é fornecido', () => {
    const filtersFromUrl = createBaseFilters()
    const preFilters = {
      status: 'ativo',
    }

    applyPreFilters(filtersFromUrl, mockCategories, preFilters)

    expect(categorysIdFilterPerName).not.toHaveBeenCalled()
  })

  it('deve não chamar categorysIdFilterPerName quando categoryNames está vazio', () => {
    const filtersFromUrl = createBaseFilters()
    const preFilters = {
      categoryNames: [],
    }

    applyPreFilters(filtersFromUrl, mockCategories, preFilters)

    expect(categorysIdFilterPerName).not.toHaveBeenCalled()
  })

  it('deve retornar novo objeto, não modificar o original', () => {
    vi.mocked(categorysIdFilterPerName).mockReturnValue([1])

    const filtersFromUrl = createBaseFilters()
    const preFilters = {
      categoryNames: ['Material de Escritório'],
      status: 'ativo',
    }

    const result = applyPreFilters(filtersFromUrl, mockCategories, preFilters)

    expect(result).not.toBe(filtersFromUrl)
    expect(filtersFromUrl.categoriaId).toEqual([]) // Confirma que o original está vazio
    expect(filtersFromUrl.status).toBe('')
  })

  it('deve lidar com apenas uma categoria na interseção', () => {
    vi.mocked(categorysIdFilterPerName).mockReturnValue([1, 2])

    const filtersFromUrl: CatalogoFilters = {
      ...createBaseFilters(),
      categoriaId: [1],
    }
    const preFilters = {
      categoryNames: ['Material de Escritório', 'Informática'],
    }

    const result = applyPreFilters(filtersFromUrl, mockCategories, preFilters)

    expect(result.categoriaId).toEqual([1])
  })

  it('deve preservar ordem das categorias da URL na interseção', () => {
    vi.mocked(categorysIdFilterPerName).mockReturnValue([1, 2, 3])

    const filtersFromUrl: CatalogoFilters = {
      ...createBaseFilters(),
      categoriaId: [3, 2, 1],
    }
    const preFilters = {
      categoryNames: ['Material de Escritório', 'Informática', 'Limpeza'],
    }

    const result = applyPreFilters(filtersFromUrl, mockCategories, preFilters)

    expect(result.categoriaId).toEqual([3, 2, 1])
  })

  it('deve lidar com categorias duplicadas na URL', () => {
    vi.mocked(categorysIdFilterPerName).mockReturnValue([1, 2])

    const filtersFromUrl: CatalogoFilters = {
      ...createBaseFilters(),
      categoriaId: [1, 1, 2, 2],
    }
    const preFilters = {
      categoryNames: ['Material de Escritório', 'Informática'],
    }

    const result = applyPreFilters(filtersFromUrl, mockCategories, preFilters)

    expect(result.categoriaId).toEqual([1, 2])
  })
})
