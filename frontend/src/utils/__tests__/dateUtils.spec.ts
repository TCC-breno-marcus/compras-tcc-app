import { describe, it, expect } from 'vitest'
import { formatDate, parseDateString, addTimeEndInDate } from '../dateUtils'

describe('Utils: DateUtils', () => {
  
  // --- Testes do formatDate ---
  describe('formatDate', () => {
    const mockDate = new Date('2023-12-25T14:30:00')

    it('deve retornar string vazia se a data for nula', () => {
      expect(formatDate(null)).toBe('')
    })

    it('deve formatar como "short" (padrão)', () => {
      // short: dd/mm/aaaa
      expect(formatDate(mockDate)).toBe('25/12/2023')
    })

    it('deve formatar como "iso"', () => {
      // iso: aaaa-mm-dd
      expect(formatDate(mockDate, 'iso')).toBe('2023-12-25')
    })

    it('deve formatar como "long"', () => {
      // long: "25 de dezembro de 2023 às 14:30"
      const result = formatDate(mockDate, 'long').toLowerCase()
      expect(result).toContain('25 de dezembro de 2023')
      expect(result).toContain('14:30')
    })
  })

  // --- Testes do parseDateString ---
  describe('parseDateString', () => {
    it('deve retornar null para strings vazias ou inválidas', () => {
      expect(parseDateString('')).toBeNull()
      expect(parseDateString(null)).toBeNull()
      expect(parseDateString('data-invalida')).toBeNull()
    })

    it('deve fazer parse de formato ISO (com T)', () => {
      const date = parseDateString('2023-12-25T10:00:00')
      expect(date).toBeInstanceOf(Date)
      expect(date?.getFullYear()).toBe(2023)
      expect(date?.getMonth()).toBe(11) // Dezembro é 11
    })

    it('deve fazer parse de formato YYYY-MM-DD (com traço)', () => {
      const date = parseDateString('2023-12-25')
      expect(date).toBeInstanceOf(Date)
      expect(date?.getDate()).toBe(25)
      expect(date?.getHours()).toBe(0) 
    })

    it('deve fazer parse de formato BR curto (dd/mm/aaaa)', () => {
      const date = parseDateString('25/12/2023')
      expect(date).toBeInstanceOf(Date)
      expect(date?.getDate()).toBe(25)
      expect(date?.getMonth()).toBe(11)
      expect(date?.getFullYear()).toBe(2023)
    })

    it('deve validar datas inexistentes no formato BR (ex: 32/01)', () => {
      expect(parseDateString('32/01/2023')).toBeNull()
      expect(parseDateString('30/02/2023')).toBeNull()
    })

    it('deve fazer parse de formato extenso ("dd de mes de aaaa")', () => {
      const date = parseDateString('25 de dezembro de 2023 às 14:30')
      expect(date).toBeInstanceOf(Date)
      expect(date?.getDate()).toBe(25)
      expect(date?.getMonth()).toBe(11)
      expect(date?.getHours()).toBe(14)
      expect(date?.getMinutes()).toBe(30)
    })
    
    it('deve lidar com formato extenso sem hora especificada', () => {
        const date = parseDateString('25 de dezembro de 2023')
        expect(date).toBeInstanceOf(Date)
        expect(date?.getHours()).toBe(0)
    })
  })

  // --- Testes do addTimeEndInDate ---
  describe('addTimeEndInDate', () => {
    it('deve adicionar o sufixo de fim do dia se não houver tempo', () => {
      expect(addTimeEndInDate('2023-12-25')).toBe('2023-12-25T23:59:00Z')
    })

    it('deve manter a string se já tiver "T"', () => {
      const input = '2023-12-25T10:00:00'
      expect(addTimeEndInDate(input)).toBe(input)
    })
  })
})