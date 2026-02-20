// formatQuantity.test.ts
import { describe, it, expect } from 'vitest'
import { formatQuantity } from '../number'

describe('formatQuantity', () => {
  it('deve formatar zero corretamente', () => {
    expect(formatQuantity(0)).toBe('0')
  })

  it('deve formatar números positivos pequenos sem separador', () => {
    expect(formatQuantity(1)).toBe('1')
    expect(formatQuantity(99)).toBe('99')
    expect(formatQuantity(999)).toBe('999')
  })

  it('deve formatar milhares com separador de ponto', () => {
    expect(formatQuantity(1000)).toBe('1.000')
    expect(formatQuantity(1234)).toBe('1.234')
    expect(formatQuantity(9999)).toBe('9.999')
  })

  it('deve formatar milhões com separador de ponto', () => {
    expect(formatQuantity(1000000)).toBe('1.000.000')
    expect(formatQuantity(1234567)).toBe('1.234.567')
  })

  it('deve formatar bilhões com separador de ponto', () => {
    expect(formatQuantity(1000000000)).toBe('1.000.000.000')
    expect(formatQuantity(1234567890)).toBe('1.234.567.890')
  })

  it('deve formatar números negativos corretamente', () => {
    expect(formatQuantity(-1)).toBe('-1')
    expect(formatQuantity(-1000)).toBe('-1.000')
    expect(formatQuantity(-1234567)).toBe('-1.234.567')
  })

  it('deve arredondar números decimais para baixo', () => {
    expect(formatQuantity(1000.4)).toBe('1.000')
    expect(formatQuantity(1000.49)).toBe('1.000')
  })

  it('deve arredondar números decimais para cima', () => {
    expect(formatQuantity(1000.5)).toBe('1.001')
    expect(formatQuantity(1000.99)).toBe('1.001')
  })

  it('deve lidar com números muito grandes', () => {
    expect(formatQuantity(999999999999)).toBe('999.999.999.999')
  })

  it('deve lidar com números muito pequenos negativos', () => {
    expect(formatQuantity(-999999999999)).toBe('-999.999.999.999')
  })
})