// transformItem.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { transformItem } from '../itemTransformer'
import type { Item } from '../../types'
import * as stringUtils from '@/utils/stringUtils'

// Mock do módulo stringUtils
vi.mock('@/utils/stringUtils', () => ({
  toTitleCase: vi.fn((str: string) => {
    if (!str) return ''
    return str.toLowerCase().replace(/(?:^|\s)\w/g, (match) => match.toUpperCase())
  })
}))

describe('transformItem', () => {
  let toTitleCaseMock: ReturnType<typeof vi.fn>

  beforeEach(async () => {
    vi.clearAllMocks()
    const stringUtilsMocked = await import('@/utils/stringUtils')
    toTitleCaseMock = stringUtilsMocked.toTitleCase as ReturnType<typeof vi.fn>
  })

  const createMockItem = (overrides?: Partial<Item>): Item => ({
    id: 1,
    nome: 'CANETA ESFEROGRÁFICA AZUL',
    catMat: '123456',
    descricao: 'Caneta esferográfica',
    especificacao: 'Tinta azul, ponta fina',
    categoria: {
      id: 1,
      nome: 'Material de Escritório',
    } as any,
    linkImagem: 'https://example.com/image.jpg',
    precoSugerido: 1.50,
    isActive: true,
    ...overrides,
  })

  it('deve transformar o nome do item usando toTitleCase', () => {
    const item = createMockItem()
    const result = transformItem(item)

    expect(toTitleCaseMock).toHaveBeenCalledWith('CANETA ESFEROGRÁFICA AZUL')
    expect(result.nome).toBe('Caneta Esferográfica Azul')
  })

  it('deve manter todas as outras propriedades inalteradas', () => {
    const item = createMockItem()
    const result = transformItem(item)

    expect(result.id).toBe(item.id)
    expect(result.catMat).toBe(item.catMat)
    expect(result.descricao).toBe(item.descricao)
    expect(result.especificacao).toBe(item.especificacao)
    expect(result.categoria).toBe(item.categoria)
    expect(result.linkImagem).toBe(item.linkImagem)
    expect(result.precoSugerido).toBe(item.precoSugerido)
    expect(result.isActive).toBe(item.isActive)
  })

  it('deve retornar um novo objeto, não modificar o original', () => {
    const item = createMockItem()
    const originalNome = item.nome
    const result = transformItem(item)

    expect(result).not.toBe(item)
    expect(item.nome).toBe(originalNome)
    expect(result.nome).toBe('Caneta Esferográfica Azul')
  })

  it('deve transformar nome em minúsculas', () => {
    const item = createMockItem({ nome: 'caneta azul' })
    const result = transformItem(item)

    expect(toTitleCaseMock).toHaveBeenCalledWith('caneta azul')
    expect(result.nome).toBe('Caneta Azul')
  })

  it('deve transformar nome com maiúsculas e minúsculas misturadas', () => {
    const item = createMockItem({ nome: 'CaNeTa AzUl' })
    const result = transformItem(item)

    expect(toTitleCaseMock).toHaveBeenCalledWith('CaNeTa AzUl')
    expect(result.nome).toBe('Caneta Azul')
  })

  it('deve lidar com nome vazio', () => {
    const item = createMockItem({ nome: '' })
    const result = transformItem(item)

    expect(toTitleCaseMock).toHaveBeenCalledWith('')
    expect(result.nome).toBe('')
  })

  it('deve lidar com nome contendo caracteres especiais', () => {
    const item = createMockItem({ nome: 'CANETA-ESFEROGRÁFICA/AZUL' })
    const result = transformItem(item)

    expect(toTitleCaseMock).toHaveBeenCalledWith('CANETA-ESFEROGRÁFICA/AZUL')
  })

  it('deve lidar com nome contendo números', () => {
    const item = createMockItem({ nome: 'CANETA 123 AZUL' })
    const result = transformItem(item)

    expect(toTitleCaseMock).toHaveBeenCalledWith('CANETA 123 AZUL')
    expect(result.nome).toBe('Caneta 123 Azul')
  })

  it('deve lidar com nome contendo acentos', () => {
    const item = createMockItem({ nome: 'CANETA ESFEROGRÁFICA AÇÃO' })
    const result = transformItem(item)

    expect(toTitleCaseMock).toHaveBeenCalledWith('CANETA ESFEROGRÁFICA AÇÃO')
    expect(result.nome).toBe('Caneta Esferográfica Ação')
  })

  it('deve preservar o objeto categoria sem modificações', () => {
    const categoria = {
      id: 5,
      nome: 'Categoria Teste',
      descricao: 'Descrição',
    }
    const item = createMockItem({ categoria: categoria as any })
    const result = transformItem(item)

    expect(result.categoria).toBe(categoria)
    expect(result.categoria).toEqual(categoria)
  })

  it('deve lidar com isActive false', () => {
    const item = createMockItem({ isActive: false })
    const result = transformItem(item)

    expect(result.isActive).toBe(false)
  })

  it('deve lidar com precoSugerido zero', () => {
    const item = createMockItem({ precoSugerido: 0 })
    const result = transformItem(item)

    expect(result.precoSugerido).toBe(0)
  })

  it('deve lidar com precoSugerido decimal', () => {
    const item = createMockItem({ precoSugerido: 99.99 })
    const result = transformItem(item)

    expect(result.precoSugerido).toBe(99.99)
  })

  it('deve lidar com linkImagem vazio', () => {
    const item = createMockItem({ linkImagem: '' })
    const result = transformItem(item)

    expect(result.linkImagem).toBe('')
  })

  it('deve lidar com catMat vazio', () => {
    const item = createMockItem({ catMat: '' })
    const result = transformItem(item)

    expect(result.catMat).toBe('')
  })

  it('deve lidar com descricao vazia', () => {
    const item = createMockItem({ descricao: '' })
    const result = transformItem(item)

    expect(result.descricao).toBe('')
  })

  it('deve lidar com especificacao vazia', () => {
    const item = createMockItem({ especificacao: '' })
    const result = transformItem(item)

    expect(result.especificacao).toBe('')
  })

  it('deve chamar toTitleCase apenas uma vez', () => {
    const item = createMockItem()
    transformItem(item)

    expect(toTitleCaseMock).toHaveBeenCalledTimes(1)
  })

  it('deve transformar múltiplos itens mantendo independência', () => {
    const item1 = createMockItem({ id: 1, nome: 'ITEM UM' })
    const item2 = createMockItem({ id: 2, nome: 'ITEM DOIS' })

    const result1 = transformItem(item1)
    const result2 = transformItem(item2)

    expect(result1.id).toBe(1)
    expect(result1.nome).toBe('Item Um')
    expect(result2.id).toBe(2)
    expect(result2.nome).toBe('Item Dois')
  })

  it('deve manter a referência de arrays e objetos aninhados', () => {
    const categoria = { id: 1, nome: 'Cat' }
    const item = createMockItem({ categoria: categoria as any })
    const result = transformItem(item)

    // Spread operator mantém a referência de objetos aninhados
    expect(result.categoria).toBe(item.categoria)
  })

  it('deve lidar com nome contendo múltiplos espaços', () => {
    const item = createMockItem({ nome: 'CANETA  AZUL   TESTE' })
    const result = transformItem(item)

    expect(toTitleCaseMock).toHaveBeenCalledWith('CANETA  AZUL   TESTE')
  })

  it('deve lidar com id zero', () => {
    const item = createMockItem({ id: 0 })
    const result = transformItem(item)

    expect(result.id).toBe(0)
  })

  it('deve lidar com id negativo', () => {
    const item = createMockItem({ id: -1 })
    const result = transformItem(item)

    expect(result.id).toBe(-1)
  })

  it('deve lidar com nome muito longo', () => {
    const longName = 'A'.repeat(1000)
    const item = createMockItem({ nome: longName })
    const result = transformItem(item)

    expect(toTitleCaseMock).toHaveBeenCalledWith(longName)
  })
})