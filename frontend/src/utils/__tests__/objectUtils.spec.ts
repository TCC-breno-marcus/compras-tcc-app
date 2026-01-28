// dataComparison.test.ts
import { describe, it, expect } from 'vitest'
import { dataHasBeenChanged, getChangedProperties } from '../objectUtils'

describe('getChangedProperties', () => {
  it('deve retornar array vazio quando nenhuma propriedade mudou', () => {
    const origin = { name: 'John', age: 30 }
    const edited = { name: 'John', age: 30 }

    expect(getChangedProperties(origin, edited)).toEqual([])
  })

  it('deve retornar chaves alteradas quando valores primitivos mudam', () => {
    const origin = { name: 'John', age: 30 }
    const edited = { name: 'Jane', age: 30 }

    expect(getChangedProperties(origin, edited)).toEqual(['name'])
  })

  it('deve retornar múltiplas chaves alteradas', () => {
    const origin = { name: 'John', age: 30, city: 'NY' }
    const edited = { name: 'Jane', age: 25, city: 'NY' }

    expect(getChangedProperties(origin, edited)).toEqual(['name', 'age'])
  })

  it('deve retornar todas as chaves quando origin é null', () => {
    const edited = { name: 'John', age: 30 }

    expect(getChangedProperties(null, edited)).toEqual(['name', 'age'])
  })

  it('deve detectar mudanças em objetos aninhados', () => {
    const origin = { user: { name: 'John', age: 30 } }
    const edited = { user: { name: 'Jane', age: 30 } }

    expect(getChangedProperties(origin, edited)).toEqual(['user'])
  })

  it('deve retornar array vazio para objetos aninhados iguais', () => {
    const origin = { user: { name: 'John', age: 30 } }
    const edited = { user: { name: 'John', age: 30 } }

    expect(getChangedProperties(origin, edited)).toEqual([])
  })

  it('deve detectar mudanças em arrays', () => {
    const origin = { tags: ['a', 'b'] }
    const edited = { tags: ['a', 'b', 'c'] }

    expect(getChangedProperties(origin, edited)).toEqual(['tags'])
  })

  it('deve retornar array vazio para arrays iguais', () => {
    const origin = { tags: ['a', 'b', 'c'] }
    const edited = { tags: ['a', 'b', 'c'] }

    expect(getChangedProperties(origin, edited)).toEqual([])
  })

  it('deve lidar com objeto edited vazio', () => {
    const origin = { name: 'John', age: 30 }
    const edited = {}

    expect(getChangedProperties(origin, edited)).toEqual([])
  })

  it('deve detectar novas propriedades adicionadas', () => {
    const origin = { name: 'John' }
    const edited = { name: 'John', age: 30 }

    expect(getChangedProperties(origin, edited)).toEqual(['age'])
  })

  it('deve lidar com valores booleanos', () => {
    const origin = { active: true, verified: false }
    const edited = { active: false, verified: false }

    expect(getChangedProperties(origin, edited)).toEqual(['active'])
  })

  it('deve lidar com números zero', () => {
    const origin = { count: 0 }
    const edited = { count: 1 }

    expect(getChangedProperties(origin, edited)).toEqual(['count'])
  })

  it('deve lidar com strings vazias', () => {
    const origin = { name: '' }
    const edited = { name: 'John' }

    expect(getChangedProperties(origin, edited)).toEqual(['name'])
  })

  it('deve detectar mudanças em objetos aninhados profundos', () => {
    const origin = { 
      user: { 
        profile: { 
          address: { city: 'NY' } 
        } 
      } 
    }
    const edited = { 
      user: { 
        profile: { 
          address: { city: 'LA' } 
        } 
      } 
    }

    expect(getChangedProperties(origin, edited)).toEqual(['user'])
  })
})

describe('dataHasBeenChanged', () => {
  it('deve retornar false quando nenhuma propriedade mudou', () => {
    const origin = { name: 'John', age: 30 }
    const edited = { name: 'John', age: 30 }

    expect(dataHasBeenChanged(origin, edited)).toBe(false)
  })

  it('deve retornar true quando pelo menos uma propriedade mudou', () => {
    const origin = { name: 'John', age: 30 }
    const edited = { name: 'Jane', age: 30 }

    expect(dataHasBeenChanged(origin, edited)).toBe(true)
  })

  it('deve retornar true quando origin é null', () => {
    const edited = { name: 'John' }

    expect(dataHasBeenChanged(null, edited)).toBe(true)
  })

  it('deve retornar false quando edited está vazio', () => {
    const origin = { name: 'John', age: 30 }
    const edited = {}

    expect(dataHasBeenChanged(origin, edited)).toBe(false)
  })

  it('deve retornar true quando objetos aninhados mudam', () => {
    const origin = { user: { name: 'John' } }
    const edited = { user: { name: 'Jane' } }

    expect(dataHasBeenChanged(origin, edited)).toBe(true)
  })

  it('deve retornar false quando objetos aninhados são iguais', () => {
    const origin = { user: { name: 'John' } }
    const edited = { user: { name: 'John' } }

    expect(dataHasBeenChanged(origin, edited)).toBe(false)
  })
})