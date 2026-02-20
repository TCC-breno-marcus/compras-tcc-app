import { beforeEach, describe, expect, it, vi } from 'vitest'

const { apiClientMock } = vi.hoisted(() => ({
  apiClientMock: {
    get: vi.fn(),
  },
}))

vi.mock('@/services/apiClient', () => ({
  apiClient: apiClientMock,
}))

import { dashService } from '../dashService'

describe('Service: dashService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('deve buscar dashboard no endpoint correto', async () => {
    apiClientMock.get.mockResolvedValue({ data: { kpis: {} } })
    const result = await dashService.getDashboards()

    expect(apiClientMock.get).toHaveBeenCalledWith('/dashboard')
    expect(result).toEqual({ kpis: {} })
  })

  it('deve lançar erro padronizado ao falhar', async () => {
    apiClientMock.get.mockRejectedValue(new Error('erro'))

    await expect(dashService.getDashboards()).rejects.toThrow('Não foi possível buscar dashboards.')
  })
})

