// stringUtils.test.ts
import { describe, it, expect } from 'vitest'
import { toTitleCase } from '../stringUtils'

describe('toTitleCase', () => {
  it('deve retornar string vazia quando input é vazio', () => {
    expect(toTitleCase('')).toBe('')
  })

  it('deve capitalizar primeira letra de palavra única minúscula', () => {
    expect(toTitleCase('hello')).toBe('Hello')
  })

  it('deve capitalizar primeira letra de palavra única maiúscula', () => {
    expect(toTitleCase('HELLO')).toBe('Hello')
  })

  it('deve capitalizar primeira letra de cada palavra', () => {
    expect(toTitleCase('hello world')).toBe('Hello World')
  })

  it('deve converter todas as letras para minúsculas antes de capitalizar', () => {
    expect(toTitleCase('HELLO WORLD')).toBe('Hello World')
  })

  it('deve lidar com múltiplas palavras', () => {
    expect(toTitleCase('the quick brown fox')).toBe('The Quick Brown Fox')
  })

  it('deve lidar com múltiplos espaços entre palavras', () => {
    expect(toTitleCase('hello  world')).toBe('Hello  World')
  })

  it('deve capitalizar após quebra de linha', () => {
    expect(toTitleCase('hello\nworld')).toBe('Hello\nWorld')
  })

  it('deve capitalizar após tab', () => {
    expect(toTitleCase('hello\tworld')).toBe('Hello\tWorld')
  })

  it('deve lidar com palavras com acentos', () => {
    expect(toTitleCase('josé maria')).toBe('José Maria')
  })

  it('deve lidar com caracteres especiais', () => {
    expect(toTitleCase('hello-world')).toBe('Hello-world')
  })

  it('deve capitalizar após pontuação seguida de espaço', () => {
    expect(toTitleCase('hello. world')).toBe('Hello. World')
  })

  it('deve lidar com strings com apenas espaços', () => {
    expect(toTitleCase('   ')).toBe('   ')
  })

  it('deve lidar com uma única letra', () => {
    expect(toTitleCase('a')).toBe('A')
  })

  it('deve lidar com números', () => {
    expect(toTitleCase('hello 123 world')).toBe('Hello 123 World')
  })

  it('deve capitalizar palavras curtas', () => {
    expect(toTitleCase('a b c')).toBe('A B C')
  })

  it('deve lidar com palavras começando com números', () => {
    expect(toTitleCase('123abc 456def')).toBe('123abc 456def')
  })

  it('deve preservar espaços no início', () => {
    expect(toTitleCase(' hello world')).toBe(' Hello World')
  })

  it('deve preservar espaços no final', () => {
    expect(toTitleCase('hello world ')).toBe('Hello World ')
  })

  it('deve lidar com texto em português com preposições', () => {
    expect(toTitleCase('departamento de tecnologia da informação')).toBe('Departamento De Tecnologia Da Informação')
  })

  it('deve lidar com mix de maiúsculas e minúsculas', () => {
    expect(toTitleCase('HeLLo WoRLd')).toBe('Hello World')
  })
})