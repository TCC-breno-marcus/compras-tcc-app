import { beforeEach, describe, expect, it, vi } from 'vitest'

const { apiClientMock } = vi.hoisted(() => ({
  apiClientMock: {
    get: vi.fn(),
    patch: vi.fn(),
    post: vi.fn(),
  },
}))

vi.mock('@/services/apiClient', () => ({
  apiClient: apiClientMock,
}))

import { settingService } from '../settingService'

describe('Service: settingService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('deve buscar configurações', async () => {
    apiClientMock.get.mockResolvedValue({ data: { prazoSubmissao: '2026-12-31' } })

    const result = await settingService.getSettings()

    expect(apiClientMock.get).toHaveBeenCalledWith('/configuracao')
    expect(result).toEqual({ prazoSubmissao: '2026-12-31' })
  })

  it('deve atualizar configurações parcialmente', async () => {
    apiClientMock.patch.mockResolvedValue({ data: { maxQuantidadePorItem: 2 } })

    const result = await settingService.updateSettings({ maxQuantidadePorItem: 2 } as any)

    expect(apiClientMock.patch).toHaveBeenCalledWith('/configuracao', { maxQuantidadePorItem: 2 })
    expect(result).toEqual({ maxQuantidadePorItem: 2 })
  })

  it('deve executar rotina de arquivamento', async () => {
    apiClientMock.post.mockResolvedValue({ data: { message: 'ok' } })

    const result = await settingService.archiveOldSolicitations()

    expect(apiClientMock.post).toHaveBeenCalledWith('/solicitacao/manutencao/encerrar-anos-anteriores')
    expect(result).toEqual({ message: 'ok' })
  })

  it('deve lançar erro padronizado no getSettings quando falhar', async () => {
    apiClientMock.get.mockRejectedValue(new Error('boom'))

    await expect(settingService.getSettings()).rejects.toThrow(
      'Não foi possível buscar o prazo de submissão.',
    )
  })
})

