
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { categorysIdFilterPerName } from '../categoriaTransformer'
import type { Categoria } from '../../types'
import { transformCategory } from '../categoriaTransformer'

// Mock do módulo stringUtils
vi.mock('@/utils/stringUtils', () => ({
  toTitleCase: vi.fn((str: string) => {
    if (!str) return ''
    return str.toLowerCase().replace(/(?:^|\s)\w/g, (match) => match.toUpperCase())
  })
}))

describe('transformCategory', () => {
  let toTitleCaseMock: ReturnType<typeof vi.fn>

  beforeEach(async () => {
    vi.clearAllMocks()
    const stringUtilsMocked = await import('@/utils/stringUtils')
    toTitleCaseMock = stringUtilsMocked.toTitleCase as ReturnType<typeof vi.fn>
  })

  const createMockCategoria = (overrides?: Partial<Categoria>): Categoria => ({
    id: 1,
    nome: 'MATERIAL DE ESCRITÓRIO',
    descricao: 'Categoria para materiais de escritório',
    isActive: true,
    ...overrides,
  })

  it('deve transformar o nome da categoria usando toTitleCase', () => {
    const categoria = createMockCategoria()
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('MATERIAL DE ESCRITÓRIO')
    expect(result.nome).toBe('Material De Escritório')
  })

  it('deve manter todas as outras propriedades inalteradas', () => {
    const categoria = createMockCategoria()
    const result = transformCategory(categoria)

    expect(result.id).toBe(categoria.id)
    expect(result.descricao).toBe(categoria.descricao)
    expect(result.isActive).toBe(categoria.isActive)
  })

  it('deve retornar um novo objeto, não modificar o original', () => {
    const categoria = createMockCategoria()
    const originalNome = categoria.nome
    const result = transformCategory(categoria)

    expect(result).not.toBe(categoria)
    expect(categoria.nome).toBe(originalNome)
    expect(result.nome).toBe('Material De Escritório')
  })

  it('deve transformar nome em minúsculas', () => {
    const categoria = createMockCategoria({ nome: 'informática' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('informática')
    expect(result.nome).toBe('Informática')
  })

  it('deve transformar nome com maiúsculas e minúsculas misturadas', () => {
    const categoria = createMockCategoria({ nome: 'MaTeRiAl De EsCrItÓrIo' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('MaTeRiAl De EsCrItÓrIo')
    expect(result.nome).toBe('Material De Escritório')
  })

  it('deve lidar com nome vazio', () => {
    const categoria = createMockCategoria({ nome: '' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('')
    expect(result.nome).toBe('')
  })

  it('deve lidar com nome contendo caracteres especiais', () => {
    const categoria = createMockCategoria({ nome: 'MATERIAL/ESCRITÓRIO-GERAL' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('MATERIAL/ESCRITÓRIO-GERAL')
  })

  it('deve lidar com nome contendo números', () => {
    const categoria = createMockCategoria({ nome: 'CATEGORIA 123 TESTE' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('CATEGORIA 123 TESTE')
    expect(result.nome).toBe('Categoria 123 Teste')
  })

  it('deve lidar com nome contendo acentos', () => {
    const categoria = createMockCategoria({ nome: 'ELETRÔNICOS E COMUNICAÇÃO' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('ELETRÔNICOS E COMUNICAÇÃO')
    expect(result.nome).toBe('Eletrônicos E Comunicação')
  })

  it('deve lidar com isActive false', () => {
    const categoria = createMockCategoria({ isActive: false })
    const result = transformCategory(categoria)

    expect(result.isActive).toBe(false)
  })

  it('deve lidar com descricao vazia', () => {
    const categoria = createMockCategoria({ descricao: '' })
    const result = transformCategory(categoria)

    expect(result.descricao).toBe('')
  })

  it('deve lidar com descricao longa', () => {
    const longDescription = 'A'.repeat(1000)
    const categoria = createMockCategoria({ descricao: longDescription })
    const result = transformCategory(categoria)

    expect(result.descricao).toBe(longDescription)
    expect(result.descricao.length).toBe(1000)
  })

  it('deve chamar toTitleCase apenas uma vez', () => {
    const categoria = createMockCategoria()
    transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledTimes(1)
  })

  it('deve transformar múltiplas categorias mantendo independência', () => {
    const categoria1 = createMockCategoria({ id: 1, nome: 'CATEGORIA UM' })
    const categoria2 = createMockCategoria({ id: 2, nome: 'CATEGORIA DOIS' })

    const result1 = transformCategory(categoria1)
    const result2 = transformCategory(categoria2)

    expect(result1.id).toBe(1)
    expect(result1.nome).toBe('Categoria Um')
    expect(result2.id).toBe(2)
    expect(result2.nome).toBe('Categoria Dois')
  })

  it('deve lidar com nome contendo múltiplos espaços', () => {
    const categoria = createMockCategoria({ nome: 'MATERIAL  DE   ESCRITÓRIO' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('MATERIAL  DE   ESCRITÓRIO')
  })

  it('deve lidar com id zero', () => {
    const categoria = createMockCategoria({ id: 0 })
    const result = transformCategory(categoria)

    expect(result.id).toBe(0)
  })

  it('deve lidar com id negativo', () => {
    const categoria = createMockCategoria({ id: -1 })
    const result = transformCategory(categoria)

    expect(result.id).toBe(-1)
  })

  it('deve lidar com nome muito longo', () => {
    const longName = 'CATEGORIA '.repeat(100)
    const categoria = createMockCategoria({ nome: longName })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith(longName)
  })

  it('deve lidar com nome contendo apenas espaços', () => {
    const categoria = createMockCategoria({ nome: '   ' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('   ')
  })

  it('deve lidar com nome de uma única palavra', () => {
    const categoria = createMockCategoria({ nome: 'LIMPEZA' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('LIMPEZA')
    expect(result.nome).toBe('Limpeza')
  })

  it('deve lidar com nome de uma única letra', () => {
    const categoria = createMockCategoria({ nome: 'A' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('A')
    expect(result.nome).toBe('A')
  })

  it('deve lidar com preposições e artigos', () => {
    const categoria = createMockCategoria({ nome: 'MATERIAL DE ESCRITÓRIO DA EMPRESA' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('MATERIAL DE ESCRITÓRIO DA EMPRESA')
    expect(result.nome).toBe('Material De Escritório Da Empresa')
  })

  it('deve lidar com pontuação no nome', () => {
    const categoria = createMockCategoria({ nome: 'MATERIAL, ESCRITÓRIO E OUTROS' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('MATERIAL, ESCRITÓRIO E OUTROS')
  })

  it('deve lidar com parênteses no nome', () => {
    const categoria = createMockCategoria({ nome: 'MATERIAL (ESCRITÓRIO)' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('MATERIAL (ESCRITÓRIO)')
  })

  it('deve preservar a estrutura do objeto spread', () => {
    const categoria = createMockCategoria({
      id: 99,
      nome: 'TESTE',
      descricao: 'Descrição de teste',
      isActive: false,
    })
    const result = transformCategory(categoria)

    expect(result).toHaveProperty('id')
    expect(result).toHaveProperty('nome')
    expect(result).toHaveProperty('descricao')
    expect(result).toHaveProperty('isActive')
    expect(Object.keys(result)).toEqual(['id', 'nome', 'descricao', 'isActive'])
  })

  it('deve lidar com nome contendo quebra de linha', () => {
    const categoria = createMockCategoria({ nome: 'MATERIAL\nDE ESCRITÓRIO' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('MATERIAL\nDE ESCRITÓRIO')
  })

  it('deve lidar com nome contendo tabulação', () => {
    const categoria = createMockCategoria({ nome: 'MATERIAL\tDE ESCRITÓRIO' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('MATERIAL\tDE ESCRITÓRIO')
  })

  it('deve lidar com nome começando com número', () => {
    const categoria = createMockCategoria({ nome: '123 MATERIAL DE ESCRITÓRIO' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('123 MATERIAL DE ESCRITÓRIO')
  })

  it('deve lidar com nome começando com caractere especial', () => {
    const categoria = createMockCategoria({ nome: '&MATERIAL DE ESCRITÓRIO' })
    const result = transformCategory(categoria)

    expect(toTitleCaseMock).toHaveBeenCalledWith('&MATERIAL DE ESCRITÓRIO')
  })
})

describe('categorysIdFilterPerName', () => {
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

  // Array separado para testes de case-insensitive
  const mockCategoriesWithDuplicates: Categoria[] = [
    createMockCategoria(1, 'Material de Escritório'),
    createMockCategoria(2, 'Informática'),
    createMockCategoria(3, 'Limpeza'),
    createMockCategoria(4, 'Alimentos'),
    createMockCategoria(5, 'material de escritório'), 
  ]

  it('deve retornar array vazio quando namesFilter é null', () => {
    expect(categorysIdFilterPerName(mockCategories, null as any)).toEqual([])
  })

  it('deve retornar array vazio quando namesFilter é undefined', () => {
    expect(categorysIdFilterPerName(mockCategories, undefined as any)).toEqual([])
  })

  it('deve retornar array vazio quando namesFilter está vazio', () => {
    expect(categorysIdFilterPerName(mockCategories, [])).toEqual([])
  })

  it('deve retornar ID de categoria que corresponde ao nome', () => {
    const result = categorysIdFilterPerName(mockCategories, ['Informática'])
    expect(result).toEqual([2])
  })

  it('deve retornar IDs de múltiplas categorias', () => {
    const result = categorysIdFilterPerName(mockCategories, [
      'Material de Escritório',
      'Informática',
      'Limpeza',
    ])
    expect(result).toEqual([1, 2, 3])
  })

  it('deve ser case-insensitive na comparação de nomes', () => {
    const result = categorysIdFilterPerName(mockCategoriesWithDuplicates, [
      'MATERIAL DE ESCRITÓRIO',
      'informática',
      'LiMpEzA',
    ])
    expect(result).toContain(1)
    expect(result).toContain(2)
    expect(result).toContain(3)
    expect(result).toContain(5)
    expect(result.length).toBe(4)
  })

  it('deve encontrar categorias independente do case', () => {
    const result = categorysIdFilterPerName(mockCategories, [
      'material de escritório',
      'INFORMÁTICA', 
    ])
    expect(result).toEqual([1, 2])
  })

  it('deve ignorar nomes que não existem nas categorias', () => {
    const result = categorysIdFilterPerName(mockCategories, [
      'Informática',
      'Categoria Inexistente',
      'Limpeza',
    ])
    expect(result).toEqual([2, 3])
  })

  it('deve retornar array vazio quando nenhum nome corresponde', () => {
    const result = categorysIdFilterPerName(mockCategories, [
      'Categoria Inexistente',
      'Outra Inexistente',
    ])
    expect(result).toEqual([])
  })

  it('deve lidar com array de categorias vazio', () => {
    const result = categorysIdFilterPerName([], ['Material de Escritório'])
    expect(result).toEqual([])
  })

  it('deve lidar com nomes duplicados no filtro', () => {
    const result = categorysIdFilterPerName(mockCategories, [
      'Informática',
      'Informática',
      'Informática',
    ])
    // Deve retornar apenas um ID, mesmo com duplicatas no filtro
    expect(result).toEqual([2])
  })

  it('deve retornar todos os IDs quando todas as categorias correspondem', () => {
    const result = categorysIdFilterPerName(mockCategories, [
      'Material de Escritório',
      'Informática',
      'Limpeza',
      'Alimentos',
    ])
    expect(result).toEqual([1, 2, 3, 4])
  })

  it('deve lidar com nomes contendo espaços extras', () => {
    const categoriesWithSpaces: Categoria[] = [
      createMockCategoria(1, '  Material de Escritório  '),
      createMockCategoria(2, 'Informática'),
    ]
    
    const result = categorysIdFilterPerName(categoriesWithSpaces, [
      'material de escritório',
    ])
    
    // toLowerCase() não remove espaços, então '  material de escritório  ' !== 'material de escritório'
    expect(result).toEqual([])
  })

  it('deve lidar com categoria que tem espaços e busca sem espaços extras', () => {
    const categoriesWithSpaces: Categoria[] = [
      createMockCategoria(1, '  Material de Escritório  '),
      createMockCategoria(2, 'Informática'),
    ]
    
    const result = categorysIdFilterPerName(categoriesWithSpaces, [
      '  material de escritório  ',
    ])
    
    expect(result).toEqual([1])
  })

  it('deve preservar a ordem das categorias do array original', () => {
    const result = categorysIdFilterPerName(mockCategories, [
      'Limpeza',
      'Material de Escritório',
      'Informática',
    ])
    expect(result).toEqual([1, 2, 3])
  })

  it('deve lidar com caracteres especiais no nome', () => {
    const specialCategories: Categoria[] = [
      createMockCategoria(1, 'Material/Escritório'),
      createMockCategoria(2, 'Informática & TI'),
    ]
    
    const result = categorysIdFilterPerName(specialCategories, [
      'Material/Escritório',
      'Informática & TI',
    ])
    
    expect(result).toEqual([1, 2])
  })

  it('deve lidar com acentos nos nomes', () => {
    const accentCategories: Categoria[] = [
      createMockCategoria(1, 'Eletrônicos'),
      createMockCategoria(2, 'Comunicação'),
    ]
    
    const result = categorysIdFilterPerName(accentCategories, [
      'eletrônicos',
      'comunicação',
    ])
    
    expect(result).toEqual([1, 2])
  })

  it('deve retornar múltiplos IDs quando há duplicatas case-insensitive', () => {
    const result = categorysIdFilterPerName(mockCategoriesWithDuplicates, [
      'Material de Escritório',
    ])
    
    // Ambos IDs 1 e 5 correspondem
    expect(result).toContain(1)
    expect(result).toContain(5)
    expect(result.length).toBe(2)
  })

  it('deve retornar apenas IDs únicos mesmo com duplicatas case-insensitive no filtro', () => {
    const result = categorysIdFilterPerName(mockCategoriesWithDuplicates, [
      'Material de Escritório',
      'MATERIAL DE ESCRITÓRIO',
      'material de escritório',
    ])
    
    // Ambos IDs 1 e 5 correspondem, mas cada um deve aparecer apenas uma vez
    expect(result).toContain(1)
    expect(result).toContain(5)
    expect(result.length).toBe(2)
  })

  it('deve lidar com nomes parcialmente corretos', () => {
    const result = categorysIdFilterPerName(mockCategories, [
      'Material', // Parcial, não deve corresponder
      'Informática', // Completo, deve corresponder
    ])
    
    expect(result).toEqual([2])
  })

  it('deve lidar com busca vazia em cada string', () => {
    const result = categorysIdFilterPerName(mockCategories, ['', 'Informática', ''])
    
    expect(result).toEqual([2])
  })

  it('deve ser case-insensitive para caracteres especiais', () => {
    const specialCategories: Categoria[] = [
      createMockCategoria(1, 'Informática & TI'),
    ]
    
    const result = categorysIdFilterPerName(specialCategories, [
      'INFORMÁTICA & TI',
    ])
    
    expect(result).toEqual([1])
  })

  it('deve lidar com categoria com nome de uma palavra', () => {
    const simpleCategories: Categoria[] = [
      createMockCategoria(1, 'Limpeza'),
      createMockCategoria(2, 'Alimentos'),
    ]
    
    const result = categorysIdFilterPerName(simpleCategories, [
      'limpeza',
      'ALIMENTOS',
    ])
    
    expect(result).toEqual([1, 2])
  })
})