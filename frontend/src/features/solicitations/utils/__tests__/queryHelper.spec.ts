import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mapQueryToFilters } from '../queryHelper' // Ajuste o caminho se necessário
import { getFirstQueryValue, getSortOrderFromQuery } from '@/utils/queryHelper'
import type { LocationQuery } from 'vue-router'

vi.mock('@/utils/queryHelper', () => ({
  getFirstQueryValue: vi.fn(),
  getSortOrderFromQuery: vi.fn(),
}))

describe('Solicitations Utils: queryHelper', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(getFirstQueryValue).mockImplementation((val) => val as string)
    vi.mocked(getSortOrderFromQuery).mockReturnValue(null)
  })

  describe('mapQueryToFilters', () => {
    it('deve mapear campos básicos corretamente (externalId, pageSize, pageNumber)', () => {
      const query: LocationQuery = {
        externalId: '123',
        pageSize: '10',
        pageNumber: '1',
      }

      const result = mapQueryToFilters(query, 'MySolicitationFilters')

      expect(result.externalId).toBe('123')
      expect(result.pageSize).toBe('10')
      expect(result.pageNumber).toBe('1')
    })

    it('deve validar e mapear o campo "tipo" apenas se for Geral ou Patrimonial', () => {
      // Caso 1: Geral
      vi.mocked(getFirstQueryValue).mockReturnValueOnce('Geral')
      expect(mapQueryToFilters({}, 'MySolicitationFilters').tipo).toBe('Geral')

      // Caso 2: Patrimonial
      vi.mocked(getFirstQueryValue).mockReturnValueOnce('Patrimonial')
      expect(mapQueryToFilters({}, 'MySolicitationFilters').tipo).toBe('Patrimonial')

      // Caso 3: Inválido
      vi.mocked(getFirstQueryValue).mockReturnValueOnce('Invalido')
      expect(mapQueryToFilters({}, 'MySolicitationFilters').tipo).toBe('')
    })

    it('deve processar statusIds transformando em array de números', () => {
      // Cenário A: Único valor na query (?statusIds=1)
      const querySingle: LocationQuery = { statusIds: '1' }
      const resultSingle = mapQueryToFilters(querySingle, 'MySolicitationFilters')
      expect(resultSingle.statusIds).toEqual([1])

      // Cenário B: Múltiplos valores (?statusIds=1&statusIds=2)
      const queryMulti: LocationQuery = { statusIds: ['1', '2'] }
      const resultMulti = mapQueryToFilters(queryMulti, 'MySolicitationFilters')
      expect(resultMulti.statusIds).toEqual([1, 2])

      // Cenário C: Valores inválidos ignorados
      const queryInvalid: LocationQuery = { statusIds: ['1', 'abc', '3'] }
      const resultInvalid = mapQueryToFilters(queryInvalid, 'MySolicitationFilters')
      expect(resultInvalid.statusIds).toEqual([1, 3])
    })

    it('deve processar o range de datas corretamente', () => {
      vi.mocked(getFirstQueryValue).mockImplementation((val) => {
        if (val === '2023-01-01') return '2023-01-01'
        if (val === '2023-01-31') return '2023-01-31'
        return ''
      })

      const query: LocationQuery = {
        dataInicial: '2023-01-01',
        dataFinal: '2023-01-31',
      }

      const result = mapQueryToFilters(query, 'MySolicitationFilters')

      expect(result.dateRange).toHaveLength(2)
      const range = result.dateRange as (Date | null)[]

      expect(range[0]).toEqual(new Date('2023-01-01T00:00:00'))
      expect(range[1]).toEqual(new Date('2023-01-31T00:00:00'))
    })

    it('deve lidar com range de data parcial (só data inicial)', () => {
      vi.mocked(getFirstQueryValue).mockImplementation((val) => {
        if (val === '2023-01-01') return '2023-01-01'
        return ''
      })

      const query: LocationQuery = { dataInicial: '2023-01-01' }
      const result = mapQueryToFilters(query, 'MySolicitationFilters')

      const range = result.dateRange as (Date | null)[]

      expect(range).toHaveLength(2)
      expect(range[0]).toEqual(new Date('2023-01-01T00:00:00'))
      expect(range[1]).toBeNull()
    })

    describe('Contexto: SolicitationFilters', () => {
      it('deve incluir pessoaId (convertido para número) e siglaDepartamento', () => {
        vi.mocked(getFirstQueryValue).mockImplementation((val) => String(val))

        const query: LocationQuery = {
          pessoaId: '55',
          siglaDepartamento: 'TI',
        }

        const result = mapQueryToFilters(query, 'SolicitationFilters') as any

        expect(result.pessoaId).toBe(55)
        expect(result.siglaDepartamento).toBe('TI')
      })

      it('deve retornar pessoaId como null se não fornecido', () => {
        vi.mocked(getFirstQueryValue).mockReturnValue('')

        const result = mapQueryToFilters({}, 'SolicitationFilters') as any

        expect(result.pessoaId).toBeNull()
      })
    })

    describe('Contexto: MySolicitationFilters', () => {
      it('NÃO deve incluir campos restritos (pessoaId, siglaDepartamento) mesmo se estiverem na URL', () => {
        vi.mocked(getFirstQueryValue).mockImplementation((val) => String(val))

        const query: LocationQuery = {
          pessoaId: '999',
          siglaDepartamento: 'RH',
        }

        const result = mapQueryToFilters(query, 'MySolicitationFilters') as any

        expect(result.pessoaId).toBeUndefined()
        expect(result.siglaDepartamento).toBeUndefined()
      })
    })
  })
})
