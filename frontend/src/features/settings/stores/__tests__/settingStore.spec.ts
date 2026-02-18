import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { settingServiceMock, getChangedPropertiesMock, addTimeEndInDateMock } = vi.hoisted(() => ({
  settingServiceMock: {
    getSettings: vi.fn(),
    updateSettings: vi.fn(),
  },
  getChangedPropertiesMock: vi.fn(),
  addTimeEndInDateMock: vi.fn(),
}))

vi.mock('@/features/settings/services/settingService', () => ({
  settingService: settingServiceMock,
}))

vi.mock('@/utils/objectUtils', () => ({
  getChangedProperties: getChangedPropertiesMock,
}))

vi.mock('@/utils/dateUtils', () => ({
  addTimeEndInDate: addTimeEndInDateMock,
}))

import { useSettingStore } from '../settingStore'

describe('Store: settingStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('deve buscar configurações com sucesso', async () => {
    const store = useSettingStore()
    const settings = {
      prazoSubmissao: '2099-12-31T23:59:00Z',
      maxQuantidadePorItem: 5,
      maxItensDiferentesPorSolicitacao: 10,
      emailContatoPrincipal: 'contato@sistema.com',
      emailParaNotificacoes: 'notif@sistema.com',
    }
    settingServiceMock.getSettings.mockResolvedValue(settings)

    await store.fetchSettings()

    expect(store.settings).toEqual(settings)
    expect(store.isLoading).toBe(false)
  })

  it('deve montar payload parcial na atualização', async () => {
    const store = useSettingStore()
    store.settings = {
      prazoSubmissao: '2026-12-31',
      maxQuantidadePorItem: 2,
      maxItensDiferentesPorSolicitacao: 4,
      emailContatoPrincipal: 'a@sistema.com',
      emailParaNotificacoes: 'b@sistema.com',
    }

    getChangedPropertiesMock.mockReturnValue(['prazoSubmissao', 'maxQuantidadePorItem'])
    addTimeEndInDateMock.mockReturnValue('2026-12-31T23:59:00Z')
    const updated = {
      ...store.settings,
      prazoSubmissao: '2026-12-31T23:59:00Z',
      maxQuantidadePorItem: 3,
    }
    settingServiceMock.updateSettings.mockResolvedValue(updated)

    const ok = await store.updateSettings({
      prazoSubmissao: '2026-12-31',
      maxQuantidadePorItem: 3,
      emailContatoPrincipal: 'a@sistema.com',
    })

    expect(ok).toBe(true)
    expect(addTimeEndInDateMock).toHaveBeenCalledWith('2026-12-31')
    expect(settingServiceMock.updateSettings).toHaveBeenCalledWith({
      prazoSubmissao: '2026-12-31T23:59:00Z',
      maxQuantidadePorItem: 3,
    })
    expect(store.settings).toEqual(updated)
  })

  it('deve retornar undefined quando update falhar', async () => {
    const store = useSettingStore()
    store.settings = {
      prazoSubmissao: '2026-12-31',
      maxQuantidadePorItem: 2,
      maxItensDiferentesPorSolicitacao: 4,
      emailContatoPrincipal: 'a@sistema.com',
      emailParaNotificacoes: 'b@sistema.com',
    }
    getChangedPropertiesMock.mockReturnValue(['maxQuantidadePorItem'])
    settingServiceMock.updateSettings.mockRejectedValue(new Error('erro'))

    const result = await store.updateSettings({ maxQuantidadePorItem: 9 })

    expect(result).toBeUndefined()
    expect(store.isLoading).toBe(false)
  })

  it('deve indicar prazo expirado corretamente', () => {
    const store = useSettingStore()

    store.settings = {
      prazoSubmissao: '2000-01-01T00:00:00Z',
      maxQuantidadePorItem: 1,
      maxItensDiferentesPorSolicitacao: 1,
      emailContatoPrincipal: 'a@sistema.com',
      emailParaNotificacoes: 'b@sistema.com',
    }
    expect(store.deadlineHasExpired).toBe(true)

    store.settings = {
      ...store.settings,
      prazoSubmissao: '2099-01-01T00:00:00Z',
    }
    expect(store.deadlineHasExpired).toBe(false)
  })
})
