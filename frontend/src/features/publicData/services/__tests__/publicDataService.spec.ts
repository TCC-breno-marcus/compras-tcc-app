import { beforeEach, describe, expect, it, vi } from 'vitest'

const { apiClientMock } = vi.hoisted(() => ({
  apiClientMock: {
    get: vi.fn(),
  },
}))

vi.mock('@/services/apiClient', () => ({
  apiClient: apiClientMock,
}))

import { publicDataService } from '../publicDataService'

describe('Service: publicDataService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('deve serializar filtros de consulta pública', async () => {
    apiClientMock.get.mockResolvedValue({
      data: {
        data: [],
        totalCount: 0,
        pageNumber: 1,
        pageSize: 25,
        totalPages: 1,
        totalItensSolicitados: 0,
        valorTotalSolicitado: 0,
      },
    })

    await publicDataService.getSolicitations({
      statusId: '1',
      siglaDepartamento: 'DCOMP',
      itemsType: 'geral',
      pageNumber: '2',
      pageSize: '10',
      valorMinimo: 100,
      valorMaximo: null,
    })

    const [, config] = apiClientMock.get.mock.calls[0]
    const params = config.params as URLSearchParams

    expect(params.get('statusId')).toBe('1')
    expect(params.get('siglaDepartamento')).toBe('DCOMP')
    expect(params.get('itemsType')).toBe('geral')
    expect(params.get('pageNumber')).toBe('2')
    expect(params.get('pageSize')).toBe('10')
    expect(params.get('valorMinimo')).toBe('100')
    expect(params.get('valorMaximo')).toBeNull()
  })

  it('deve exportar CSV com responseType blob', async () => {
    const blob = new Blob(['csv'], { type: 'text/csv' })
    apiClientMock.get.mockResolvedValue({ data: blob })

    const result = await publicDataService.exportSolicitations({ pageSize: '25' }, 'csv')

    expect(apiClientMock.get).toHaveBeenCalledWith('/dados-publicos/solicitacoes', {
      params: expect.any(URLSearchParams),
      responseType: 'blob',
    })

    const [, config] = apiClientMock.get.mock.calls[0]
    const params = config.params as URLSearchParams
    expect(params.get('formatoArquivo')).toBe('csv')
    expect(result).toBe(blob)
  })

  it('deve exportar PDF com responseType blob', async () => {
    const blob = new Blob(['pdf'], { type: 'application/pdf' })
    apiClientMock.get.mockResolvedValue({ data: blob })

    const result = await publicDataService.exportSolicitations({ pageSize: '25' }, 'pdf')

    expect(apiClientMock.get).toHaveBeenCalledWith('/dados-publicos/solicitacoes', {
      params: expect.any(URLSearchParams),
      responseType: 'blob',
    })

    const [, config] = apiClientMock.get.mock.calls[0]
    const params = config.params as URLSearchParams
    expect(params.get('formatoArquivo')).toBe('pdf')
    expect(result).toBe(blob)
  })

  it('deve exportar JSON como Blob serializado', async () => {
    apiClientMock.get.mockResolvedValue({
      data: {
        data: [{ id: 10 }],
        totalCount: 1,
        pageNumber: 1,
        pageSize: 25,
        totalPages: 1,
        totalItensSolicitados: 5,
        valorTotalSolicitado: 100,
      },
    })

    const result = await publicDataService.exportSolicitations({}, 'json')

    const [, config] = apiClientMock.get.mock.calls[0]
    const params = config.params as URLSearchParams
    expect(params.get('formatoArquivo')).toBe('json')
    expect(config.responseType).toBeUndefined()
    expect(result).toBeInstanceOf(Blob)
    expect(result.type).toContain('application/json')
  })

  it('deve lançar erro padronizado ao falhar consulta', async () => {
    apiClientMock.get.mockRejectedValue(new Error('falha'))

    await expect(publicDataService.getSolicitations()).rejects.toThrow(
      'Não foi possível carregar os dados públicos no momento.',
    )
  })
})
