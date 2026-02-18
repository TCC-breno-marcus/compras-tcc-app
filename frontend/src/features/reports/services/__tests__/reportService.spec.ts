import { beforeEach, describe, expect, it, vi } from 'vitest'

const { apiClientMock } = vi.hoisted(() => ({
  apiClientMock: {
    get: vi.fn(),
  },
}))

vi.mock('@/services/apiClient', () => ({
  apiClient: apiClientMock,
}))

import { reportService } from '../reportService'

describe('Service: reportService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('deve serializar filtros de itens por departamento', async () => {
    apiClientMock.get.mockResolvedValue({ data: { data: [], totalCount: 0, pageNumber: 1, pageSize: 10 } })

    await reportService.getItemsPerDepartment({
      searchTerm: 'item',
      categoriaNome: '',
      itemsType: 'geral',
      siglaDepartamento: 'DCOMP',
      sortOrder: 'asc',
      pageSize: '10',
      pageNumber: '1',
    })

    const [, config] = apiClientMock.get.mock.calls[0]
    const params = config.params as URLSearchParams
    expect(params.get('searchTerm')).toBe('item')
    expect(params.get('itemsType')).toBe('geral')
    expect(params.get('siglaDepartamento')).toBe('DCOMP')
  })

  it('deve exportar itens por departamento no formato informado', async () => {
    const blob = new Blob(['csv'], { type: 'text/csv' })
    apiClientMock.get.mockResolvedValue({ data: blob })

    const result = await reportService.exportItemsPerDepartment('geral', 'csv')

    expect(apiClientMock.get).toHaveBeenCalledWith('/relatorio/itens-departamento/geral/exportar', {
      params: { formatoArquivo: 'csv' },
      responseType: 'blob',
    })
    expect(result).toBe(blob)
  })

  it('deve enviar DataInicio/DataFim no relatório de centro', async () => {
    apiClientMock.get.mockResolvedValue({ data: [] })

    await reportService.getCenterExpenses({ DataInicio: '2026-01-01', DataFim: '2026-01-31' })
    const [, config] = apiClientMock.get.mock.calls[0]
    const params = config.params as URLSearchParams

    expect(params.get('DataInicio')).toBe('2026-01-01')
    expect(params.get('DataFim')).toBe('2026-01-31')
  })

  it('deve lançar erro padronizado no consumo por categoria', async () => {
    apiClientMock.get.mockRejectedValue(new Error('falhou'))

    await expect(
      reportService.getCategoryConsumption({ DataInicio: '2026-01-01', DataFim: '2026-01-31' }),
    ).rejects.toThrow('Não foi possível carregar o consumo por categoria.')
  })
})

