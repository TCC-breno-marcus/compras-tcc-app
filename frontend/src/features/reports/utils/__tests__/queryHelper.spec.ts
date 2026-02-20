import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mapQueryToFilters } from '../queryHelper'
import { getFirstQueryValue, getSortOrderFromQuery } from '@/utils/queryHelper'
import type { LocationQuery } from 'vue-router'

vi.mock('@/utils/queryHelper', () => ({
  getFirstQueryValue: vi.fn(),
  getSortOrderFromQuery: vi.fn(),
  getQueryAsArrayOfNumbers: vi.fn(),
}))

describe('Management Utils: queryHelper', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('mapQueryToFilters', () => {
    it('deve mapear corretamente todos os campos de uma query preenchida', () => {
      vi.mocked(getFirstQueryValue).mockImplementation((val) => String(val))
      vi.mocked(getSortOrderFromQuery).mockReturnValue('asc')

      const query: LocationQuery = {
        searchTerm: 'notebook',
        categoriaNome: 'informática',
        itemsType: 'patrimonial',
        siglaDepartamento: 'TI',
        sortOrder: 'asc',
        pageSize: '20',
        pageNumber: '2',
      }

      const result = mapQueryToFilters(query)

      // Verificações
      expect(result.searchTerm).toBe('notebook')
      expect(result.categoriaNome).toBe('informática')
      expect(result.itemsType).toBe('patrimonial')
      expect(result.siglaDepartamento).toBe('TI')
      expect(result.sortOrder).toBe('asc')
      expect(result.pageSize).toBe('20')
      expect(result.pageNumber).toBe('2')

      // Garante que os helpers foram chamados com as chaves corretas
      expect(getFirstQueryValue).toHaveBeenCalledWith('notebook')
      expect(getFirstQueryValue).toHaveBeenCalledWith('patrimonial')
    })

    it('deve lidar com query vazia ou campos ausentes', () => {
      // Setup: Mocks retornando valores "vazios" padrão
      vi.mocked(getFirstQueryValue).mockReturnValue('')
      vi.mocked(getSortOrderFromQuery).mockReturnValue(null)

      const query: LocationQuery = {}

      const result = mapQueryToFilters(query)

      expect(result.searchTerm).toBe('')
      expect(result.categoriaNome).toBe('')
      expect(result.itemsType).toBe('')
      expect(result.siglaDepartamento).toBe('')
      expect(result.sortOrder).toBeNull()
      expect(result.pageSize).toBe('')
      expect(result.pageNumber).toBe('')
    })

    it('deve repassar itemsType corretamente (geral/patrimonial)', () => {
      vi.mocked(getFirstQueryValue).mockReturnValue('geral')

      const query: LocationQuery = { itemsType: 'geral' }
      const result = mapQueryToFilters(query)

      expect(result.itemsType).toBe('geral')
    })

    it('deve usar getSortOrderFromQuery para o campo sortOrder', () => {
      const query: LocationQuery = { sortOrder: 'desc' }

      mapQueryToFilters(query)

      expect(getSortOrderFromQuery).toHaveBeenCalledWith('desc')
    })

    it('deve tratar arrays na query chamando o helper (ex: ?searchTerm=a&searchTerm=b)', () => {
      const queryArray: LocationQuery = { searchTerm: ['a', 'b'] }

      mapQueryToFilters(queryArray)

      expect(getFirstQueryValue).toHaveBeenCalledWith(['a', 'b'])
    })
  })
})
