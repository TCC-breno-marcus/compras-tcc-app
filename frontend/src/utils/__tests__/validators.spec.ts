import { describe, it, expect } from 'vitest'
import { emailValidator, cpfValidator } from '../validators'

//TODO: adicionar testes pra todas funcoes da pasta utils

describe('Utils: Validators', () => {
  
  describe('emailValidator', () => {
    it('deve retornar true para um email válido', () => {
      expect(emailValidator('teste@exemplo.com')).toBe(true)
      expect(emailValidator('nome.sobrenome@empresa.com.br')).toBe(true)
    })

    it('deve retornar false para email inválido', () => {
      expect(emailValidator('teste@')).toBe(false)
      expect(emailValidator('teste.com')).toBe(false)
      expect(emailValidator('')).toBe(false)
      expect(emailValidator('teste@.com')).toBe(false)
    })
  })

  describe('cpfValidator', () => {
    it('deve retornar true para um CPF válido (com pontuação)', () => {
      expect(cpfValidator('123.456.789-09')).toBe(true)
      expect(cpfValidator('381.081.060-60')).toBe(true) 
      expect(cpfValidator('959.737.990-26')).toBe(true) 
      expect(cpfValidator('070.652.570-10')).toBe(true) 
      expect(cpfValidator('147.816.240-65')).toBe(true) 
      expect(cpfValidator('835.404.260-00')).toBe(true) 
    })

    it('deve retornar true para um CPF válido (apenas números)', () => {
      expect(cpfValidator('96678700066')).toBe(true)
      expect(cpfValidator('38108106060')).toBe(true) 
      expect(cpfValidator('95973799026')).toBe(true) 
      expect(cpfValidator('07065257010')).toBe(true) 
      expect(cpfValidator('14781624065')).toBe(true) 
      expect(cpfValidator('83540426000')).toBe(true) 
    })

    it('deve retornar false para um CPF inválido (com pontuação)', () => {
      expect(cpfValidator('123.426.789-09')).toBe(false)
      expect(cpfValidator('936.562.260-20')).toBe(false) 
      expect(cpfValidator('959.237.990-26')).toBe(false) 
      expect(cpfValidator('070.252.570-10')).toBe(false) 
      expect(cpfValidator('147.812.240-65')).toBe(false) 
      expect(cpfValidator('835.424.260-00')).toBe(false) 
    })

    it('deve retornar false para um CPF inválido (apenas números)', () => {
      expect(cpfValidator('93656926020')).toBe(false)
      expect(cpfValidator('93656926020')).toBe(false) 
      expect(cpfValidator('95973799021')).toBe(false) 
      expect(cpfValidator('07065257011')).toBe(false) 
      expect(cpfValidator('14781624061')).toBe(false) 
      expect(cpfValidator('83540426001')).toBe(false) 
    })

    it('deve retornar false para CPF com tamanho incorreto', () => {
      expect(cpfValidator('123')).toBe(false)
      expect(cpfValidator('12345678901234')).toBe(false)
    })

    it('deve retornar false para CPF com todos os dígitos iguais', () => {
      expect(cpfValidator('111.111.111-11')).toBe(false)
      expect(cpfValidator('00000000000')).toBe(false)
    })
    
    it('deve retornar false se os dígitos verificadores estiverem errados', () => {
      expect(cpfValidator('936.569.260-21')).toBe(false) 
    })
  })
})