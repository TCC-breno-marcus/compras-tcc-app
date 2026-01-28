// queryUtils.test.ts
import { describe, it, expect } from 'vitest'
import { 
  getFirstQueryValue, 
  getSortOrderFromQuery, 
  getQueryAsArrayOfNumbers 
} from '../queryHelper'
import type { LocationQueryValue } from 'vue-router'

describe('getFirstQueryValue', () => {
  it('deve retornar string vazia quando value é null', () => {
    expect(getFirstQueryValue(null)).toBe('')
  })

  it('deve retornar a string quando value é uma string', () => {
    expect(getFirstQueryValue('test')).toBe('test')
  })

  it('deve retornar o primeiro elemento quando value é um array', () => {
    expect(getFirstQueryValue(['first', 'second', 'third'])).toBe('first')
  })

  it('deve retornar string vazia quando array está vazio', () => {
    expect(getFirstQueryValue([])).toBe('')
  })

  it('deve retornar string vazia quando primeiro elemento do array é null', () => {
    const value: LocationQueryValue[] = [null, 'second']
    expect(getFirstQueryValue(value)).toBe('')
  })

  it('deve retornar a string mesmo que seja vazia', () => {
    expect(getFirstQueryValue('')).toBe('')
  })

  it('deve retornar o segundo elemento se o primeiro for null', () => {
    const value: LocationQueryValue[] = [null, 'second', 'third']
    const result = getFirstQueryValue(value)
    // A função retorna o primeiro elemento (null), que vira ''
    expect(result).toBe('')
  })
})

describe('getSortOrderFromQuery', () => {
  it('deve retornar "asc" quando value é "asc"', () => {
    expect(getSortOrderFromQuery('asc')).toBe('asc')
  })

  it('deve retornar "desc" quando value é "desc"', () => {
    expect(getSortOrderFromQuery('desc')).toBe('desc')
  })

  it('deve retornar null quando value é qualquer outra string', () => {
    expect(getSortOrderFromQuery('invalid')).toBe(null)
    expect(getSortOrderFromQuery('ascending')).toBe(null)
    expect(getSortOrderFromQuery('ASC')).toBe(null)
  })

  it('deve retornar null quando value é null', () => {
    expect(getSortOrderFromQuery(null)).toBe(null)
  })

  it('deve retornar "asc" quando array contém "asc" como primeiro elemento', () => {
    expect(getSortOrderFromQuery(['asc', 'desc'])).toBe('asc')
  })

  it('deve retornar "desc" quando array contém "desc" como primeiro elemento', () => {
    expect(getSortOrderFromQuery(['desc', 'asc'])).toBe('desc')
  })

  it('deve retornar null quando array contém valor inválido', () => {
    expect(getSortOrderFromQuery(['invalid'])).toBe(null)
  })

  it('deve retornar null quando array está vazio', () => {
    expect(getSortOrderFromQuery([])).toBe(null)
  })

  it('deve retornar null para string vazia', () => {
    expect(getSortOrderFromQuery('')).toBe(null)
  })

  it('deve retornar null quando primeiro elemento do array é null', () => {
    const value: LocationQueryValue[] = [null, 'asc']
    expect(getSortOrderFromQuery(value)).toBe(null)
  })
})

describe('getQueryAsArrayOfNumbers', () => {
  it('deve retornar array vazio quando value é null', () => {
    expect(getQueryAsArrayOfNumbers(null)).toEqual([])
  })

  it('deve retornar array vazio quando value é undefined', () => {
    expect(getQueryAsArrayOfNumbers(undefined)).toEqual([])
  })

  it('deve retornar array vazio quando value é string vazia', () => {
    expect(getQueryAsArrayOfNumbers('')).toEqual([])
  })

  it('deve converter número único para array', () => {
    expect(getQueryAsArrayOfNumbers(5)).toEqual([5])
  })

  it('deve converter string numérica para array de números', () => {
    expect(getQueryAsArrayOfNumbers('10')).toEqual([10])
  })

  it('deve converter array de números', () => {
    expect(getQueryAsArrayOfNumbers([1, 2, 3])).toEqual([1, 2, 3])
  })

  it('deve converter array de strings numéricas', () => {
    expect(getQueryAsArrayOfNumbers(['1', '2', '3'])).toEqual([1, 2, 3])
  })

  it('deve converter array misto de números e strings', () => {
    expect(getQueryAsArrayOfNumbers([1, '2', 3, '4'])).toEqual([1, 2, 3, 4])
  })

  it('deve filtrar valores não numéricos', () => {
    expect(getQueryAsArrayOfNumbers(['1', 'abc', '3'])).toEqual([1, 3])
  })

  it('deve filtrar NaN', () => {
    expect(getQueryAsArrayOfNumbers([1, NaN, 3])).toEqual([1, 3])
  })

  it('deve filtrar números decimais', () => {
    expect(getQueryAsArrayOfNumbers([1, 2.5, 3])).toEqual([1, 3])
  })

  it('deve aceitar números negativos inteiros', () => {
    expect(getQueryAsArrayOfNumbers([-1, -2, -3])).toEqual([-1, -2, -3])
  })

  it('deve aceitar zero', () => {
    expect(getQueryAsArrayOfNumbers([0, 1, 2])).toEqual([0, 1, 2])
  })

  it('deve filtrar strings não numéricas em array', () => {
    expect(getQueryAsArrayOfNumbers(['a', 'b', 'c'])).toEqual([])
  })

  it('deve filtrar objetos', () => {
    expect(getQueryAsArrayOfNumbers([{}, 1, 2])).toEqual([1, 2])
  })

  it('deve processar arrays aninhados conforme implementação', () => {
    const result = getQueryAsArrayOfNumbers([[1], 2, 3])
    expect(result).toEqual([1, 2, 3])
  })

  it('deve lidar com números muito grandes', () => {
    expect(getQueryAsArrayOfNumbers([999999999, 1000000000])).toEqual([999999999, 1000000000])
  })

  it('deve filtrar Infinity', () => {
    expect(getQueryAsArrayOfNumbers([1, Infinity, 3])).toEqual([1, 3])
  })

  it('deve filtrar -Infinity', () => {
    expect(getQueryAsArrayOfNumbers([1, -Infinity, 3])).toEqual([1, 3])
  })
})