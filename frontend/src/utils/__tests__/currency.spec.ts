import { describe, it, expect } from 'vitest'
import { formatCurrency } from '../currency'

describe('Utils: Currency', () => {
  it('deve formatar números positivos corretamente', () => {
    const result = formatCurrency(1200.50).replace(/\u00a0/g, ' ')
    expect(result).toBe('R$ 1.200,50')
  })

  it('deve formatar zero corretamente', () => {
    const result = formatCurrency(0).replace(/\u00a0/g, ' ')
    expect(result).toBe('R$ 0,00')
  })

  it('deve lidar com casas decimais (arredondamento)', () => {
    const result = formatCurrency(10.559).replace(/\u00a0/g, ' ')
    expect(result).toBe('R$ 10,56')
  })

  it('deve retornar R$ 0,00 para null ou undefined', () => {
    expect(formatCurrency(null).replace(/\u00a0/g, ' ')).toBe('R$ 0,00')
    expect(formatCurrency(undefined).replace(/\u00a0/g, ' ')).toBe('R$ 0,00')
  })

  it('deve formatar números negativos', () => {
    const result = formatCurrency(-50).replace(/\u00a0/g, ' ')
    expect(result).toBe('-R$ 50,00')
  })
})