import { describe, it, expect } from 'vitest'
import { transformItemDepartment, transformSingleItemDepartment } from '../index'
import type { ItemDepartmentResponse } from '../../types'

const createMockItem = (overrides?: Partial<ItemDepartmentResponse>): ItemDepartmentResponse => {
  return {
    // Campos de Item (Partial)
    id: 1,
    nome: 'Cadeira Ergonômica',
    descricao: 'Cadeira de escritório',
    isActive: true,

    // Campos específicos de ItemDepartmentResponse
    categoriaNome: 'Mobiliário',
    quantidadeTotalSolicitada: 5,
    numeroDeSolicitacoes: 2,
    demandaPorDepartamento: [], // Array vazio por padrão para simplificar

    // Valores financeiros (alvos do teste)
    valorTotalSolicitado: 1000.5555,
    precoMedio: 200.5555,
    precoMinimo: 150.1234,
    precoMaximo: 250.9876,

    ...overrides,
  } as ItemDepartmentResponse
}

describe('transformSingleItemDepartment', () => {
  it('deve arredondar todos os campos financeiros para 2 casas decimais', () => {
    const item = createMockItem({
      valorTotalSolicitado: 10.556, 
      precoMedio: 10.554, 
      precoMinimo: 10.5,
      precoMaximo: 10, 
    })

    const result = transformSingleItemDepartment(item)

    expect(result.valorTotalSolicitado).toBe(10.56)
    expect(result.precoMedio).toBe(10.55)
    expect(result.precoMinimo).toBe(10.5)
    expect(result.precoMaximo).toBe(10)
  })

  it('deve resolver imprecisões de ponto flutuante (Epsilon)', () => {
    const item = createMockItem({
      valorTotalSolicitado: 1.005,
      precoMedio: 2.005,
    })

    const result = transformSingleItemDepartment(item)

    expect(result.valorTotalSolicitado).toBe(1.01)
    expect(result.precoMedio).toBe(2.01)
  })

  it('deve tratar valores undefined ou null como zero', () => {
    // Forçamos "undefined" via cast porque a tipagem diz que é number obrigatório,
    // mas na prática (runtime/API) pode vir vazio.
    const item = createMockItem({
      valorTotalSolicitado: undefined as unknown as number,
      precoMedio: null as unknown as number,
      precoMinimo: undefined as unknown as number,
      precoMaximo: null as unknown as number,
    })

    const result = transformSingleItemDepartment(item)

    expect(result.valorTotalSolicitado).toBe(0)
    expect(result.precoMedio).toBe(0)
    expect(result.precoMinimo).toBe(0)
    expect(result.precoMaximo).toBe(0)
  })

  it('deve preservar campos herdados de Item e campos extras', () => {
    const item = createMockItem({
      id: 99,
      nome: 'Mouse Gamer',
      categoriaNome: 'Periféricos',
      demandaPorDepartamento: [{ departamento: 'TI', quantidade: 5 }] as any,
    })

    const result = transformSingleItemDepartment(item)

    expect(result.id).toBe(99)
    expect(result.nome).toBe('Mouse Gamer')
    expect(result.categoriaNome).toBe('Periféricos')
    expect(result.demandaPorDepartamento).toHaveLength(1)
  })
})

describe('transformItemDepartment (Array)', () => {
  it('deve transformar uma lista de itens corretamente', () => {
    const items = [
      createMockItem({ valorTotalSolicitado: 10.129 }),
      createMockItem({ valorTotalSolicitado: 20.501 }),
    ]

    const result = transformItemDepartment(items)

    expect(result).toHaveLength(2)
    expect(result[0].valorTotalSolicitado).toBe(10.13)
    expect(result[1].valorTotalSolicitado).toBe(20.5)
  })

  it('deve retornar array vazio se receber array vazio', () => {
    const result = transformItemDepartment([])
    expect(result).toEqual([])
  })

  it('deve garantir a imutabilidade (novas referências)', () => {
    const items = [createMockItem()]
    const result = transformItemDepartment(items)

    expect(result).not.toBe(items)
    expect(result[0]).not.toBe(items[0])

    items[0].valorTotalSolicitado = 999
    expect(result[0].valorTotalSolicitado).not.toBe(999)
  })
})
